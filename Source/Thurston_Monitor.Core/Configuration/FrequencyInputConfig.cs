namespace Thurston_Monitor.Core;

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
