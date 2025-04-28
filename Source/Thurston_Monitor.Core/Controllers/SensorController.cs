using Meadow;
using Meadow.Peripherals.Sensors;
using Meadow.Units;
using Sensors.Flow.HallEffect.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Thurston_Monitor.Core.Contracts;

namespace Thurston_Monitor.Core;

public class SensorController
{
    private readonly IThurston_MonitorHardware hardware;
    private readonly StorageController storageController;

    private readonly Dictionary<int, List<(int ID, object Sensor, Func<object> ReadDelegate)>> queryPeriodDictionary = new();
    private readonly Dictionary<int, Enum?> canonicalUnitsForSensorsMap = new();

    public Dictionary<int, IVolumetricFlowSensor> FlowSensors { get; } = new();

    public SensorController(IThurston_MonitorHardware hardware, StorageController storageController)
    {
        this.hardware = hardware;

        _ = Task.Run(SensorReadProc);
    }

    private int GenerateSensorId(object sensor, string sensorName)
    {
        return sensor.GetType().GetHashCode() | sensorName.GetHashCode();
    }

    public void ApplySensorConfig(SensorConfiguration configuration)
    {
        foreach (var modbusConfig in configuration.ModbusDevices)
        {
        }
        foreach (var input in configuration.FrequencyInputs)
        {
            if (input.IsSimulated)
            {
                var sensor = new SimulatedHallEffectFlowSensor();
                sensor.StartSimulation(SimulationBehavior.Sawtooth);
                var id = GenerateSensorId(sensor, input.Description);
                FlowSensors.Add(id, sensor);
                if (!queryPeriodDictionary.ContainsKey(input.SenseIntervalSeconds))
                {
                    queryPeriodDictionary.Add(input.SenseIntervalSeconds, new List<(int ID, object Sensor, Func<object>)>());
                }
                queryPeriodDictionary[input.SenseIntervalSeconds].Add(new(id, sensor, () => sensor.Read().GetAwaiter().GetResult()));
            }
            else
            {
                throw new NotSupportedException("Non-simulated frequency inputs are not supported on the desktop");
            }
        }
        foreach (var analog in configuration.ChannelConfigurations)
        {
        }
    }

    private async Task SensorReadProc()
    {
        int tick = 0;

        while (true)
        {
            foreach (var period in queryPeriodDictionary.Keys)
            {
                if (tick % period == 0)
                {
                    foreach (var sensor in queryPeriodDictionary[period])
                    {
                        var value = sensor.ReadDelegate();
                        if (!canonicalUnitsForSensorsMap.ContainsKey(sensor.ID))
                        {
                            // var unit = GetUnitTypeFromReading(value);
                            var canonicalUnit = GetCanonicalUnitTypeValue(value);
                            canonicalUnitsForSensorsMap.Add(sensor.ID, canonicalUnit);
                        }
                        Resolver.Log.Info($"[{tick:D4}]Read sensor: {sensor.ID}:{sensor.GetType().Name}:{value}:{canonicalUnitsForSensorsMap[sensor.ID]}");

                    }
                }
            }

            tick++;
            await Task.Delay(1000);
        }
    }

    private static Enum? GetCanonicalUnitTypeValue(object unitObject)
    {
        // Get the type of the object
        Type objectType = unitObject.GetType();

        // Find the IUnit interface implementation
        Type iunitInterface = objectType.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IUnit<,>));

        if (iunitInterface != null)
        {
            // Use reflection to invoke the method
            MethodInfo method = iunitInterface.GetMethod("GetCanonicalUnitType");
            if (method != null)
            {
                return (Enum)method.Invoke(unitObject, null);
            }
        }

        return null;
    }

    private static Type? GetUnitTypeFromReading(object reading)
    {
        // Get the type of the object
        Type objectType = reading.GetType();

        // Find the IUnit interface implementation
        Type iunitInterface = objectType.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IUnit<,>));

        if (iunitInterface != null)
        {
            // Get the generic arguments of the interface
            Type[] genericArgs = iunitInterface.GetGenericArguments();

            // The second generic argument is TUnit
            if (genericArgs.Length >= 2)
            {
                return genericArgs[1]; // TUnit is the second generic parameter
            }
        }

        return null; // If no IUnit interface is found
    }
}