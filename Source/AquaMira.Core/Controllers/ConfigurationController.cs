using Meadow;
using Meadow.Foundation.Serialization;
using System;
using System.IO;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("AquaMira.Tests")]

namespace AquaMira.Core;

public class ConfigurationController
{
    public const string DefaultSettingsFileName = "sensor-config.json";

    private string SettingsFileName { get; set; }

    public SensorConfiguration SensorConfiguration { get; private set; }

    internal ConfigurationController(string? configPath)
    {
        SettingsFileName = configPath ?? DefaultSettingsFileName;

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
            Resolver.Log.Info($"Loading sensor config from {SettingsFileName}");

            try
            {
                var json = File.ReadAllText(SettingsFileName);
                SensorConfiguration = MicroJson.Deserialize<SensorConfiguration>(json);
            }
            catch (Exception ex)
            {
                Resolver.Log.Error($"Unable to load configuration: {ex.Message}");

                SensorConfiguration = SensorConfiguration.Default;
            }
        }
        else
        {
            Resolver.Log.Error($"Config file {SettingsFileName} not found. Using default configuration.");

            SensorConfiguration = SensorConfiguration.Default;
        }
    }

    public static AquaMiraAppSettings AppSettings { get; } = new();

}

public class AquaMiraAppSettings
{
    public string? ModbusSerialPort => Resolver.App.Settings.TryGetValue("AquaMira.Modbus.SerialPort", out string s) ? s : null;
    public int? ModbusBaudRate => Resolver.App.Settings.TryGetValue("AquaMira.Modbus.BaudRate", out string s) ? int.Parse(s) : null;
    public int NetworkSignalRefreshSeconds => Resolver.App.Settings.TryGetValue("AquaMira.Network.SignalRefreshSeconds", out string s) ? int.Parse(s) : 60;
}