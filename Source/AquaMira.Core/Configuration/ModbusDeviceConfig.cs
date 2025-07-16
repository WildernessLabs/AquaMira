namespace AquaMira.Core;

public class ModbusDeviceConfig : IIntervalReadSensor
{
    public string Driver { get; set; }
    public int Address { get; set; }
    public string Name { get; set; }
    public int SenseIntervalSeconds { get; set; }
    public bool IsSimulated { get; set; }
}
