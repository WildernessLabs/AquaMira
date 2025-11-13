namespace AquaMira;

public class GroPointConfiguration
{
    public int ModbusAddress { get; set; }
    public bool IsSimulated { get; set; }
    public string Name { get; set; }
    public string SensorType { get; set; }
    public int SenseIntervalSeconds { get; set; } = 60;
}
