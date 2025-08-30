using Meadow;
using Meadow.Cloud;
using Meadow.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AquaMira.Core;

public class StateController
{
    public bool IsError { get; private set; }
    public bool IsWarn { get; private set; }

}

public class CloudController : ILogProvider
{
    public enum EventIds
    {
        DeviceStarted = 101,
        SensorConfig = 102,
        DeviceData = 201,
    }

    private readonly IMeadowCloudService cloudService;
    private readonly ICommandService commandService;
    private readonly StorageController storageController;
    private readonly INetworkController networkController;

    public CloudController(
        IMeadowCloudService cloudService,
        ICommandService commandService,
        StorageController storageController,
        INetworkController networkController)
    {
        this.cloudService = cloudService;
        this.commandService = commandService;
        this.storageController = storageController;
        this.networkController = networkController;

        storageController.Records.ItemAdded += Records_ItemAdded;

        Resolver.Log.AddProvider(this);
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
        return cloudService.SendLog(log);
    }

    public Task LogError(string message)
    {
        var log = new CloudLog
        {
            Timestamp = DateTime.UtcNow,
            Message = message,
            Severity = "error"
        };
        return cloudService.SendLog(log);
    }

    public Task LogWarning(string message)
    {
        var log = new CloudLog
        {
            Timestamp = DateTime.UtcNow,
            Message = message,
            Severity = "warning"
        };
        return cloudService.SendLog(log);
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