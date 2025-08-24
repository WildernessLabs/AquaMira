namespace AquaMira;

public class CerusConfiguration
{
    public int ModbusAddress { get; set; }
    public bool IsSimulated { get; set; }
    public string Name { get; set; }
    public int SenseIntervalSeconds { get; set; } = 60;
}
