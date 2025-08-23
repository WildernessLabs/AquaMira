using System;

namespace AquaMira.Core;

public interface ISensingNode
{
    string Name { get; }
    int NodeId { get; }
    object Sensor { get; }
    Func<object?> ReadDelegate { get; }
    TimeSpan QueryPeriod { get; }
}
