using System;

namespace AquaMira.Core;

public class SensingNode : ISensingNode
{
    public TimeSpan QueryPeriod { get; }
    public int NodeId { get; }
    public object Sensor { get; }
    public Func<object?> ReadDelegate { get; }
    public string Name { get; }

    public SensingNode(string nodeName, object sensor, Func<object?> readDelegate, TimeSpan queryPeriod)
    {
        Name = nodeName;
        NodeId = sensor.GetType().GetHashCode() | nodeName.GetHashCode();
        Sensor = sensor;
        ReadDelegate = readDelegate;
        QueryPeriod = queryPeriod;
    }
}
