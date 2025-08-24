using AquaMira.Core;

namespace AquaMira;

public class T322iConfiguration
{
    public int ModbusAddress { get; set; }
    public bool IsSimulated { get; set; }
    public ExtendedChannelConfig[] Channels { get; set; }
}
