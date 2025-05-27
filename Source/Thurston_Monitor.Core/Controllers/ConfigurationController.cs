using Meadow;
using Meadow.Foundation.Serialization;
using System;
using System.IO;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Thurston_Monitor.Tests")]

namespace Thurston_Monitor.Core;

public class ConfigurationController
{
    public const string DefaultSettingsFileName = "sensor-config.json";

    private string SettingsFileName { get; set; }

    public SensorConfiguration SensorConfiguration { get; private set; }

    internal ConfigurationController(string? configPath)
    {
        SettingsFileName = configPath ?? DefaultSettingsFileName;

        Resolver.Log.Info($"Loading sensor config from {SettingsFileName}");

        if (!File.Exists(SettingsFileName))
        {
            // TODO: this needs to get logged and sent to the cloud
            throw new FileNotFoundException($"Config file {SettingsFileName} not found");
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
            try
            {
                var json = File.ReadAllText(SettingsFileName);
                SensorConfiguration = MicroJson.Deserialize<SensorConfiguration>(json);
            }
            catch (Exception ex)
            {
                Resolver.Log.Error($"Unable to load configuration: {ex.Message}");

                // TODO: this needs to get logged and sent to the cloud if possible
            }
        }
    }

    public static ThurstonAppSettings AppSettings { get; } = new();

}

public class ThurstonAppSettings
{
    public string? ModbusSerialPort => Resolver.App.Settings.TryGetValue("Thurston.Modbus.SerialPort", out string s) ? s : null;
    public int? ModbusBaudRate => Resolver.App.Settings.TryGetValue("Thurston.Modbus.BaudRate", out string s) ? int.Parse(s) : null;
}