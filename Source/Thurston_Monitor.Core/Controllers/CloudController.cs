using Meadow;
using Meadow.Cloud;
using Meadow.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Thurston_Monitor.Core;

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

    public async Task ReportDeviceStartup()
    {
        var deviceInfo = new Dictionary<string, object>
        {
            { "DeviceName", Resolver.Device.Information.DeviceName },
            { "Device ID", Resolver.Device.Information.UniqueID }
        };

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

    public async Task ReportSensorConfiguration(SensorConfiguration configuration)
    {
        var configDictionary = new Dictionary<string, object>();

        if (configuration.ConfigurableAnalogs == null)
        {
            configDictionary.Add("ConfigurableAnalogs", "null");
        }
        else if (configuration.ConfigurableAnalogs.Channels.Length == 0)
        {
            configDictionary.Add("ConfigurableAnalogs.Channels", "Zero channels");
        }
        else
        {
            configDictionary.Add("ConfigurableAnalogs.IsSimulated",
                configuration.ConfigurableAnalogs.IsSimulated);

            for (var i = 0; i < configuration.ConfigurableAnalogs.Channels.Length; i++)
            {
                configDictionary.Add(
                    $"ConfigurableAnalogs.Channel{i}.Name",
                    configuration.ConfigurableAnalogs.Channels[i].Name);
                configDictionary.Add(
                    $"ConfigurableAnalogs.Channel{i}.Type",
                    configuration.ConfigurableAnalogs.Channels[i].ChannelType);
                configDictionary.Add(
                    $"ConfigurableAnalogs.Channel{i}.Unit",
                    configuration.ConfigurableAnalogs.Channels[i].UnitType);
            }
        }

        if (configuration.T322iInputs == null)
        {
            configDictionary.Add("T322iInputs", "null");
        }
        else if (configuration.T322iInputs.Channels.Length == 0)
        {
            configDictionary.Add("T322iInputs.Channels", "Zero channels");
        }
        else
        {
            configDictionary.Add("T322iInputs.IsSimulated",
                configuration.T322iInputs.IsSimulated);

            for (var i = 0; i < configuration.T322iInputs.Channels.Length; i++)
            {
                configDictionary.Add(
                    $"T322iInputs.Channel{i}.Name",
                    configuration.T322iInputs.Channels[i].Name);
                configDictionary.Add(
                    $"T322iInputs.Channel{i}.Type",
                    configuration.T322iInputs.Channels[i].ChannelType);
                configDictionary.Add(
                    $"T322iInputs.Channel{i}.Unit",
                    configuration.T322iInputs.Channels[i].UnitType);
            }
        }

        foreach (var device in configuration.ModbusDevices)
        {
            configDictionary.Add(
                $"Modbus.{device.Driver}.IsSimulated",
                device.IsSimulated);
            configDictionary.Add(
                $"Modbus.{device.Driver}.Address",
                device.Address);
        }

        foreach (var input in configuration.DigitalInputs)
        {
            configDictionary.Add(
                $"DigitalInputs.Channel{input.ChannelNumber}.IsSimulated",
                input.IsSimulated);
            configDictionary.Add(
                $"DigitalInputs.Channel{input.ChannelNumber}.Name",
                input.Name);
        }

        foreach (var input in configuration.FrequencyInputs)
        {
            configDictionary.Add(
                $"FrequencyInputs.Channel{input.ChannelNumber}.IsSimulated",
                input.IsSimulated);
            configDictionary.Add(
                $"FrequencyInputs.Channel{input.ChannelNumber}.Name",
                input.Name);
            configDictionary.Add(
                $"FrequencyInputs.Channel{input.ChannelNumber}.Unit",
                input.UnitType);
        }

        var evt = new CloudEvent
        {
            EventId = (int)EventIds.DeviceStarted,
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