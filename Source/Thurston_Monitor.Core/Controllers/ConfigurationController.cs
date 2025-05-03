using Meadow.Foundation;
using Meadow.Foundation.Serialization;
using System;
using System.IO;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Thurston_Monitor.Tests")]

namespace Thurston_Monitor.Core;

public interface IIntervalReadSensor
{
    int SenseIntervalSeconds { get; }
}

public class DigitalInputConfig : IIntervalReadSensor
{
    public int ChannelNumber { get; set; }
    public string Name { get; set; }
    public bool IsSimulated { get; set; }
    public int SenseIntervalSeconds { get; set; }
}

public class ModbusDeviceConfig : IIntervalReadSensor
{
    public string Driver { get; set; }
    public int Address { get; set; }
    public string Name { get; set; }
    public int SenseIntervalSeconds { get; set; }
    public bool IsSimulated { get; set; }
}

public class FrequencyInputConfig : IIntervalReadSensor
{
    public int ChannelNumber { get; set; }
    public string UnitType { get; set; }
    public double Scale { get; set; }
    public double Offset { get; set; }
    public string Name { get; set; }
    public bool IsSimulated { get; set; }
    public int SenseIntervalSeconds { get; set; }
}

public class AnalogModuleConfig
{
    public bool IsSimulated { get; set; }
    public ExtendedChannelConfig[] Channels { get; set; }
}

public class ExtendedChannelConfig : ChannelConfig
{
    public int SenseIntervalSeconds { get; set; }
}

public class SensorConfiguration
{
    public DigitalInputConfig[] DigitalInputs { get; set; } = Array.Empty<DigitalInputConfig>();
    public AnalogModuleConfig? ConfigurableAnalogs { get; set; }
    public FrequencyInputConfig[] FrequencyInputs { get; set; } = Array.Empty<FrequencyInputConfig>();
    public ModbusDeviceConfig[] ModbusDevices { get; set; } = Array.Empty<ModbusDeviceConfig>();
}

public class ConfigurationController
{
    public const string DefaultSettingsFileName = "sensor-config.json";

    private string SettingsFileName { get; set; }

    public SensorConfiguration SensorConfiguration { get; private set; }

    internal ConfigurationController(string? configPath)
    {
        SettingsFileName = configPath ?? DefaultSettingsFileName;

        if (!File.Exists(SettingsFileName))
        {
            throw new ArgumentException();
        }

        Load();
    }

    public ConfigurationController()
        : this(null)
    {
    }

    private void Load()
    {
        if (File.Exists(SettingsFileName))
        {
            var json = File.ReadAllText(SettingsFileName);
            SensorConfiguration = MicroJson.Deserialize<SensorConfiguration>(json);
        }
    }
}