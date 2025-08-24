
using System;

namespace AquaMira.Core;

public class SensorConfiguration
{
    public DigitalInputConfig[] DigitalInputs { get; set; } = Array.Empty<DigitalInputConfig>();
    public AnalogModuleConfig? ConfigurableAnalogs { get; set; }
    public FrequencyInputConfig[] FrequencyInputs { get; set; } = Array.Empty<FrequencyInputConfig>();
    public ModbusDeviceConfig[] ModbusDevices { get; set; } = Array.Empty<ModbusDeviceConfig>();

    public static SensorConfiguration Default
    {
        get
        {
            return new SensorConfiguration
            {
                DigitalInputs = Array.Empty<DigitalInputConfig>(),
                ConfigurableAnalogs = null,
                FrequencyInputs = Array.Empty<FrequencyInputConfig>(),
                ModbusDevices = Array.Empty<ModbusDeviceConfig>(),
            };
        }
    }
}
