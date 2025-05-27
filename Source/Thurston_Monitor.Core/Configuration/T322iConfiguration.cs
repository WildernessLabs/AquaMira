namespace Thurston_Monitor.Core;

public class T322iConfiguration
{
    public int ModbusAddress { get; set; }
    public bool IsSimulated { get; set; }
    public ExtendedChannelConfig[] Channels { get; set; }
}
