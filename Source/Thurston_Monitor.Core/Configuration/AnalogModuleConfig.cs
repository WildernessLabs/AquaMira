namespace Thurston_Monitor.Core;

public class AnalogModuleConfig
{
    public bool IsSimulated { get; set; }
    public ExtendedChannelConfig[] Channels { get; set; }
}
