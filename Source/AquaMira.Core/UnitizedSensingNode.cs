using Meadow.Units;
using System;
using System.Linq;
using System.Reflection;

namespace AquaMira.Core;

public class UnitizedSensingNode<TUnit> : SensingNode, IUnitizedSensingNode
    where TUnit : IUnit
{
    public Enum? CanonicalUnit { get; set; }

    public double ReadAsCanonicalUnit()
    {
        var value = ReadDelegate.Invoke();

        if (value == null)
        {
            return 0;
        }

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
