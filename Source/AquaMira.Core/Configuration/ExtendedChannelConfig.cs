namespace AquaMira.Core;

public enum ChannelInputType
{
    Current_4_20,
    Current_0_20,
    Voltage_0_10,
    Count,
    DiscreteInput
}

public class ChannelConfig
{
    public int ChannelNumber { get; set; }
    public ChannelInputType ChannelType { get; set; }
    public double Scale { get; set; } = 1.0;
    public double Offset { get; set; } = 0.0;
    public string UnitType { get; set; }
    public string Name { get; set; }
}

public class ExtendedChannelConfig : ChannelConfig
{
    public int SenseIntervalSeconds { get; set; }
}
