namespace AquaMira.Core;

public class DigitalInputConfig : IIntervalReadSensor
{
    public int ChannelNumber { get; set; }
    public string Name { get; set; }
    public bool IsSimulated { get; set; }
    public int SenseIntervalSeconds { get; set; }
}
