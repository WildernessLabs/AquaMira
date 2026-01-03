using Meadow;
using Meadow.Cloud;
using Meadow.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AquaMira.Core;

public class CloudController : ILogProvider, IDisposable
{
    public enum EventIds
    {
        DeviceStarted = 101,
        SensorConfig = 102,
        DeviceData = 201,
    }

    public const int CloudFailureCheckPeriodMinutes = 1;
    public const int CloudFailureThresholdMinutes = 10;
    public const int CloudFailureEventCooldownMinutes = 5;

    public event EventHandler? CloudSendFailure;

    private readonly IMeadowCloudService cloudService;
    private readonly StorageController storageController;
    private bool disposed = false;

    private DateTimeOffset lastEventRaised = DateTimeOffset.MinValue;
    private readonly DateTimeOffset controllerStartTime = DateTimeOffset.UtcNow;
    private readonly Timer? statusCheckTimer;

    public CloudController(
        IMeadowCloudService cloudService,
        ICommandService commandService,
        StorageController storageController,
        INetworkController networkController)
    {
        this.cloudService = cloudService;
        this.storageController = storageController;

        storageController.Records.ItemAdded += Records_ItemAdded;

        Resolver.Log.AddProvider(this);

        // Start timer to check cloud send status
        statusCheckTimer = new Timer(CheckCloudSendStatus, null, TimeSpan.FromMinutes(CloudFailureCheckPeriodMinutes), TimeSpan.FromMinutes(1));
    }

    public void Dispose()
    {
        if (!disposed)
        {
            statusCheckTimer?.Dispose();
            storageController.Records.ItemAdded -= Records_ItemAdded;
            Resolver.Log.RemoveProvider(this);
            disposed = true;
        }
    }

    private void CheckCloudSendStatus(object? state)
    {
        TimeSpan timeSinceLastSend;

        // If no successful send has occurred yet, measure from controller start time
        if (cloudService.LastSuccessfulSend == null)
        {
            timeSinceLastSend = DateTimeOffset.UtcNow - controllerStartTime;

            // we could have a huge delta if we got NTP time between start and now
            // check against threshold * 2 to avoid false positives
            if (timeSinceLastSend.TotalMinutes > (CloudFailureThresholdMinutes * 2))
            {   // not true! 
                timeSinceLastSend = TimeSpan.Zero;
            }
            Resolver.Log.Trace($"No sends yet. Controller started at {controllerStartTime:HH:mm:ss}, current time {DateTimeOffset.UtcNow:HH:mm:ss}, elapsed {timeSinceLastSend.TotalMinutes:F2} minutes", Constants.LoggingSource);
        }
        else
        {
            timeSinceLastSend = DateTimeOffset.UtcNow - cloudService.LastSuccessfulSend.Value;
            Resolver.Log.Trace($"Last send at {cloudService.LastSuccessfulSend:HH:mm:ss}, current time {DateTimeOffset.UtcNow:HH:mm:ss}, elapsed {timeSinceLastSend.TotalMinutes:F2} minutes", Constants.LoggingSource);
        }

        var timeSinceLastEvent = DateTimeOffset.UtcNow - lastEventRaised;

        Resolver.Log.Trace($"Cloud data has not been sent for {timeSinceLastSend.TotalMinutes:F0} minutes", Constants.LoggingSource);

        // If data hasn't been sent for N minutes, and if it's more than the threshold, raise event
        if (timeSinceLastSend.TotalMinutes >= CloudFailureThresholdMinutes && timeSinceLastEvent.TotalMinutes >= CloudFailureEventCooldownMinutes)
        {
            lastEventRaised = DateTimeOffset.UtcNow;
            Resolver.Log.Warn($"Cloud data has not been sent successfully for {timeSinceLastSend.TotalMinutes:F0} minutes", Constants.LoggingSource);
            CloudSendFailure?.Invoke(this, EventArgs.Empty);
        }
    }

    private void Records_ItemAdded(object sender, EventArgs e)
    {
        // TODO: use a periodic timer instead to send at a slower rate?

        var batch = storageController.Records.Peek();

        while (batch != null)
        {
            var evt = new CloudEvent
            {
                EventId = (int)EventIds.DeviceData,
                Description = "Device Data",
                Measurements = batch.Values,
                Timestamp = batch.BatchTime
            };
            try
            {
                Resolver.Log.Info($"Sending {evt.Measurements.Count} values");
                cloudService.SendEvent(evt);
                storageController.Records.Remove(1);

                batch = storageController.Records.Peek();
            }
            catch (Exception ex)
            {
                // error sending or some such
                Resolver.Log.Info($"Failed to send to cloud: {ex.Message}");
                break;
            }
        }

    }

    public Task LogError(Exception exception, string? message = null)
    {
        var log = new CloudLog
        {
            Timestamp = DateTime.UtcNow,
            Message = message ?? exception.Message,
            Exception = exception.ToString()
        };
        return cloudService.SendLog(log, CloudTelemetryPriority.High);
    }

    public Task LogError(string message)
    {
        var log = new CloudLog
        {
            Timestamp = DateTime.UtcNow,
            Message = message,
            Severity = "error"
        };
        return cloudService.SendLog(log, CloudTelemetryPriority.High);
    }

    public Task LogWarning(string message)
    {
        var log = new CloudLog
        {
            Timestamp = DateTime.UtcNow,
            Message = message,
            Severity = "warning"
        };
        return cloudService.SendLog(log, CloudTelemetryPriority.High);
    }

    public async Task ReportDeviceStartup(IMeadowDevice device)
    {
        var deviceInfo = new Dictionary<string, object>
        {
            { "DeviceName", Resolver.Device.Information.DeviceName },
            { "Device ID", Resolver.Device.Information.UniqueID }
        };

        if (device.ReliabilityService != null)
        {
            if (device.ReliabilityService.SystemResetCount > 0)
            {
                deviceInfo.Add("ResetCount", device.ReliabilityService.SystemResetCount);
            }
            if (device.ReliabilityService.SystemPowerCycleCount > 0)
            {
                deviceInfo.Add("PowerCycleCount", device.ReliabilityService.SystemPowerCycleCount);
            }
            if (device.ReliabilityService.LastBootWasFromCrash)
            {
                deviceInfo.Add("LastBootWasFromCrash", true);
            }
            deviceInfo.Add("LastResetReason", device.ReliabilityService.LastResetReason.ToString());
        }

        var evt = new CloudEvent
        {
            EventId = (int)EventIds.DeviceStarted,
            Description = "Device Startup",
            Measurements = deviceInfo,
            Timestamp = DateTime.UtcNow
        };

        try
        {
            await cloudService.SendEvent(evt);
        }
        catch (Exception ex)
        {
            // we'll end up here if cloud features aren't enabled
            Resolver.Log.Info($"Failed to send to cloud: {ex.Message}");
        }
    }

    public async Task ReportSensorConfiguration(SensorController sensorController)
    {
        var configDictionary = new Dictionary<string, object>();

        // TODO: improve this (is simulated, etc) - maybe the controller should have a Descriptor property or similar?

        foreach (var nodeList in sensorController.Nodes)
        {
            foreach (var node in nodeList.Value)
            {
                configDictionary.Add($"{node.Name}", $"{node.QueryPeriod.TotalSeconds:N0} seconds");
            }
        }

        var evt = new CloudEvent
        {
            EventId = (int)EventIds.SensorConfig,
            Description = "Sensor Configuration",
            Measurements = configDictionary,
            Timestamp = DateTime.UtcNow
        };

        try
        {
            await cloudService.SendEvent(evt);
        }
        catch (Exception ex)
        {
            // we'll end up here if cloud features aren't enabled
            Resolver.Log.Info($"Failed to send to cloud: {ex.Message}");
        }
    }

    public void Log(LogLevel level, string message, string? messageGroup)
    {
        switch (level)
        {
            case LogLevel.Error:
                LogError(message);
                break;
            case LogLevel.Warning:
                LogWarning(message);
                break;
        }
    }
}