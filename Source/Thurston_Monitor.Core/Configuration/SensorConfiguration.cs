
using System;

namespace Thurston_Monitor.Core;

public class SensorConfiguration
{
    public DigitalInputConfig[] DigitalInputs { get; set; } = Array.Empty<DigitalInputConfig>();
    public AnalogModuleConfig? ConfigurableAnalogs { get; set; }
    public FrequencyInputConfig[] FrequencyInputs { get; set; } = Array.Empty<FrequencyInputConfig>();
    public ModbusDeviceConfig[] ModbusDevices { get; set; } = Array.Empty<ModbusDeviceConfig>();
    public T322iConfiguration? T322iInputs { get; set; }
}
