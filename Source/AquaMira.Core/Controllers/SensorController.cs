using AquaMira.Core.Contracts;
using Meadow;
using Meadow.Foundation.IOExpanders;
using Meadow.Peripherals.Sensors;
using Meadow.Units;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AquaMira.Core;

public class SensorController
{
    private readonly IAquaMiraHardware hardware;

    private readonly Dictionary<int, List<ISensingNode>> sensingNodes = new();

    public event EventHandler<Dictionary<string, object>>? SensorValuesUpdated;

    private ISensingNodeController? t3Controller;

    public Dictionary<int, IVolumetricFlowSensor> FlowSensors { get; } = new();
    public Dictionary<int, ICompositeSensor> ModbusSensors { get; } = new();
    public Task SensorProc { get; set; }

    public SensorController(IAquaMiraHardware hardware)
    {
        this.hardware = hardware;

        SensorProc = Task.Run(SensorReadProc);
    }

    public async Task ApplySensorConfig(SensorConfiguration configuration)
    {
        ConfigureModbusDevices(configuration.ModbusDevices);
        ConfigureDigitalInputs(configuration.DigitalInputs);

        if (configuration.T322iInputs != null)
        {
            try
            {
                IT322ai t3Module;
                if (configuration.T322iInputs.IsSimulated)
                {
                    Resolver.Log.Info("Using simulated T3-22i module");
                    t3Module = new SimulatedT322ai();
                }
                else
                {
                    Resolver.Log.Info($"Using T3-22i module at address {configuration.T322iInputs.ModbusAddress}");
                    var client = hardware.GetModbusSerialClient();
                    if (!client.IsConnected)
                    {
                        await client.Connect();
                    }
                    t3Module = new T322ai(client, (byte)configuration.T322iInputs.ModbusAddress);
                }

                t3Controller = new T322InputController(t3Module);
                var nodes = await t3Controller.ConfigureInputs(configuration.T322iInputs.Channels);
                AddSensingNodes(nodes);
            }
            catch (Exception ex)
            {
                Resolver.Log.Error($"Error configuring T3-22i inputs: {ex.Message}");
                t3Controller = null;
            }
        }
        else
        {
            Resolver.Log.Warn($"No T322i configured for this device");
        }
    }

    private void ConfigureDigitalInputs(IEnumerable<DigitalInputConfig> inputConfigs)
    {
        foreach (var config in inputConfigs)
        {
            try
            {
                Resolver.Log.Info($"digital input: {config.Name} on channel {config.ChannelNumber}");

                var input = config.IsSimulated
                    ? new RandomizedSimulatedDigitalInputPort(config.Name)
                    : hardware.InputController.GetInputForChannel(config.ChannelNumber);

                if (input == null)
                {
                    // TODO: log this!
                    continue;
                }

                // TODO: separate handling for interruptable inputs?
                //if (input is IDigitalInterruptPort) { }
                var node = new UnitizedSensingNode<Scalar>(
                    config.Name,
                    input, () =>
                    {
                        return new Scalar(input.State ? 1 : 0);
                    },
                    TimeSpan.FromSeconds(config.SenseIntervalSeconds));

                AddSensingNode(node);
            }
            catch (Exception ex)
            {
                // TODO: log this to the cloud (it's probably an unsupported device/bad config)
            }
        }
    }

    public void AddSensingNodes(IEnumerable<ISensingNode> nodes)
    {
        foreach (var node in nodes)
        {
            AddSensingNode(node);
        }
    }

    public void AddSensingNode(ISensingNode node)
    {
        lock (sensingNodes)
        {
            Resolver.Log.Info($"Adding sensing node {node.Name} with period {node.QueryPeriod.TotalSeconds} seconds");

            var interval = (int)node.QueryPeriod.TotalSeconds;

            if (!sensingNodes.ContainsKey(interval))
            {
                sensingNodes.Add(interval, new List<ISensingNode>());
            }
            sensingNodes[interval].Add(node);
        }
    }

    private void ConfigureModbusDevices(IEnumerable<ModbusDeviceConfig> modbusDevices)
    {
        foreach (var device in modbusDevices)
        {
            try
            {
                var sensor = ModbusDeviceFactory.CreateSensor(device, hardware);
                var node = new SensingNode(device.Name, sensor, () =>
                {
                    try
                    {
                        return sensor.GetCurrentValues();
                    }
                    catch (Exception mex)
                    {
                        Resolver.Log.Error($"Error reading Modbus device {device.Name} values: {mex.Message}");
                        return null;
                    }
                }, TimeSpan.FromSeconds(device.SenseIntervalSeconds));

                ModbusSensors.Add(node.NodeId, sensor);

                AddSensingNode(node);
            }
            catch (Exception ex)
            {
                // TODO: log this to the cloud (it's probably an unsupported device/bad config)
            }
        }
    }

    /// <summary>
    /// This menthod walks through list of sensors in the to read list
    /// calls the function that does the reading, and then saves the results
    /// to the telemetry list. Finally, it passes the telemetry data to the 
    /// storage controller.
    /// </summary>
    /// <returns></returns>
    private async Task SensorReadProc()
    {
        int tick = 0;

        var telemetryList = new Dictionary<string, object>();

        while (true)
        {
            telemetryList.Clear();

            lock (sensingNodes)
            {
                foreach (var period in sensingNodes.Keys)
                {
                    if (tick % period == 0)
                    {
                        foreach (var node in sensingNodes[period])
                        {
                            Resolver.Log.Debug($"Reading sensor {node.Name}...");

                            if (node is IUnitizedSensingNode usn)
                            {
                                try
                                {
                                    var value = usn.ReadAsCanonicalUnit();

                                    if (value == null)
                                    {
                                        Resolver.Log.Info($"Error reading from {node.Sensor.GetType().Name}");
                                        continue;
                                    }
                                    telemetryList.Add(usn.Name, value);
                                }
                                catch (Exception ex)
                                {
                                    Resolver.Log.Error($"Error reading from node: {node.Name}: {ex.Message}");
                                    continue;
                                }
                            }
                            else if (node is ISensingNode sensingNode)
                            {
                                object? value;

                                try
                                {
                                    value = node.ReadDelegate();
                                    if (value == null)
                                    {
                                        Resolver.Log.Info($"Error reading from {node.Sensor.GetType().Name}", "sensors");
                                        continue;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Resolver.Log.Error($"Error reading from node: {node.Name}: {ex.Message}");
                                    continue;
                                }

                                if (value is Dictionary<string, object> valueDictionary)
                                {
                                    foreach (var sensorItem in valueDictionary)
                                    {
                                        if (sensorItem.Value is IUnit unit)
                                        {
                                            telemetryList.Add(sensorItem.Key, unit.ToCanonical());
                                        }
                                        else
                                        {
                                            telemetryList.Add(sensorItem.Key, sensorItem.Value);
                                        }
                                    }
                                }
                                else if (value is IUnit unitValue)
                                {
                                    telemetryList.Add(node.Name, unitValue.ToCanonical());
                                }
                                else
                                {
                                }
                            }
                            else
                            {

                            }
                        }
                    }
                }
            }

            if (telemetryList.Count > 0)
            {
                SensorValuesUpdated?.Invoke(this, telemetryList);
            }

            tick++;
            await Task.Delay(1000);
        }
    }

    //private static Enum? GetCanonicalUnitTypeValue(object unitObject)
    //{
    //    // Get the type of the object
    //    Type objectType = unitObject.GetType();

    //    // Find the IUnit interface implementation
    //    Type iunitInterface = objectType.GetInterfaces()
    //        .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IUnit<,>));

    //    if (iunitInterface != null)
    //    {
    //        // Use reflection to invoke the method
    //        MethodInfo method = iunitInterface.GetMethod("GetCanonicalUnitType");
    //        if (method != null)
    //        {
    //            return (Enum)method.Invoke(unitObject, null);
    //        }
    //    }

    //    return null;
    //}

    //private static Type? GetUnitTypeFromReading(object reading)
    //{
    //    // Get the type of the object
    //    Type objectType = reading.GetType();

    //    // Find the IUnit interface implementation
    //    Type iunitInterface = objectType.GetInterfaces()
    //        .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IUnit<,>));

    //    if (iunitInterface != null)
    //    {
    //        // Get the generic arguments of the interface
    //        Type[] genericArgs = iunitInterface.GetGenericArguments();

    //        // The second generic argument is TUnit
    //        if (genericArgs.Length >= 2)
    //        {
    //            return genericArgs[1]; // TUnit is the second generic parameter
    //        }
    //    }

    //    return null; // If no IUnit interface is found
    //}
}