using Meadow.Units;
using System;
using System.Linq;
using System.Reflection;

namespace AquaMira.Core;

public interface IUnitizedSensingNode : ISensingNode
{
    double ReadAsCanonicalUnit();
    Enum? CanonicalUnit { get; set; }
}

public interface ISensingNode
{
    string Name { get; }
    int NodeId { get; }
    object Sensor { get; }
    Func<object?> ReadDelegate { get; }
    TimeSpan QueryPeriod { get; }
}

public class UnitizedSensingNode<TUnit> : SensingNode, IUnitizedSensingNode
    where TUnit : IUnit
{
    public Enum? CanonicalUnit { get; set; }

    public double ReadAsCanonicalUnit()
    {
        var value = ReadDelegate.Invoke();
        if (value is IUnit unit)
        {
            if (CanonicalUnit == null)
            {
                CanonicalUnit = GetCanonicalUnitTypeValue(value);
            }
            return unit.ToCanonical();
        }
        else
        {
            throw new InvalidOperationException("Read value is not a valid unit type.");
        }
    }

    private Enum? GetCanonicalUnitTypeValue(object value)
    {
        // Get the type of the object
        Type objectType = typeof(TUnit);

        // Find the IUnit interface implementation
        Type iunitInterface = objectType.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IUnit<,>));

        if (iunitInterface != null)
        {
            // Use reflection to invoke the method
            MethodInfo method = iunitInterface.GetMethod("GetCanonicalUnitType");
            if (method != null)
            {
                return (Enum)method.Invoke(value, null);
            }
        }

        return null;
    }

    public UnitizedSensingNode(string nodeName, object sensor, Func<IUnit?> readDelegate, TimeSpan queryPeriod)
        : base(nodeName, sensor, readDelegate, queryPeriod)
    {
    }
}

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
