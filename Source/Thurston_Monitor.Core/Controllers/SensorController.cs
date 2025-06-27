using Meadow;
using Meadow.Foundation;
using Meadow.Foundation.IOExpanders;
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

    private readonly Dictionary<int, List<(int ID, object Sensor, Func<object?> ReadDelegate)>> queryPeriodDictionary = new();
    private readonly Dictionary<int, Enum?> canonicalUnitsForSensorsMap = new();
    private readonly Dictionary<int, string> sensorIdToNameMap = new();

    public Dictionary<int, IVolumetricFlowSensor> FlowSensors { get; } = new();
    public IProgrammableAnalogInputModule? ProgrammableAnalogInputModule { get; private set; }
    public Dictionary<int, ICompositeSensor> ModbusSensors { get; } = new();
    public IT322ai? T3Module { get; private set; }
    public Task SensorProc { get; set; }

    public SensorController(IThurston_MonitorHardware hardware, StorageController storageController)
    {
        this.hardware = hardware;
        this.storageController = storageController;

        SensorProc = Task.Run(SensorReadProc);
    }

    private int GenerateSensorId(object sensor, string sensorName)
    {
        var id = sensor.GetType().GetHashCode() | sensorName.GetHashCode();
        if (!sensorIdToNameMap.ContainsKey(id))
        {
            sensorIdToNameMap.Add(id, sensorName);
        }
        return id;
    }

    public string? GetSensorName(int sensorId)
    {
        if (sensorIdToNameMap.ContainsKey(sensorId))
        {
            return sensorIdToNameMap[sensorId];
        }
        return null;
    }

    public async Task ApplySensorConfig(SensorConfiguration configuration)
    {
        ConfigureModbusDevices(configuration.ModbusDevices);
        ConfigureFrequencyInputs(configuration.FrequencyInputs);
        ConfigureConfigurableAnalogs(configuration.ConfigurableAnalogs);
        ConfigureDigitalInputs(configuration.DigitalInputs);
        await ConfigureT322iInputs(configuration.T322iInputs);
    }

    private void ConfigureDigitalInputs(IEnumerable<DigitalInputConfig> inputConfigs)
    {
        foreach (var config in inputConfigs)
        {
            try
            {
                Resolver.Log.Info($"digital input: {config.Name} on channel {config.ChannelNumber}");
                var id = GenerateSensorId(config, config.Name);

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

                AddSensorToQueryList(config.SenseIntervalSeconds, new(id, input, () =>
                {
                    return new Scalar(input.State ? 1 : 0);
                }));

            }
            catch (Exception ex)
            {
                // TODO: log this to the cloud (it's probably an unsupported device/bad config)
            }
        }
    }

    private void AddSensorToQueryList(int interval, (int id, object sensor, Func<object?> queryDelegate) tuple)
    {
        if (!queryPeriodDictionary.ContainsKey(interval))
        {
            queryPeriodDictionary.Add(interval, new List<(int ID, object Sensor, Func<object?>)>());
        }
        queryPeriodDictionary[interval].Add(tuple);
    }

    private void ConfigureModbusDevices(IEnumerable<ModbusDeviceConfig> modbusDevices)
    {
        foreach (var device in modbusDevices)
        {
            try
            {
                var sensor = ModbusDeviceFactory.CreateSensor(device, hardware);
                var id = GenerateSensorId(sensor, device.Name);
                ModbusSensors.Add(id, sensor);

                AddSensorToQueryList(device.SenseIntervalSeconds, new(id, sensor, () =>
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
                }));
            }
            catch (Exception ex)
            {
                // TODO: log this to the cloud (it's probably an unsupported device/bad config)
            }
        }
    }

    private async Task ConfigureT322iInputs(T322iConfiguration? moduleConfig)
    {
        if (moduleConfig == null)
        {
            Resolver.Log.Warn($"No T322i exists for this device");
            return;
        }

        if (moduleConfig.IsSimulated)
        {
            T3Module = new SimulatedT322ai();
        }
        else
        {
            try
            {
                var client = hardware.GetModbusSerialClient();
                if (!client.IsConnected)
                {
                    await client.Connect();
                }
                T3Module = new T322ai(client, (byte)moduleConfig.ModbusAddress);

                // read the serial number to verify comms
                Resolver.Log.Info($"Connecting to a T3-22i at {moduleConfig.ModbusAddress}...");
                var sn = await T3Module.ReadSerialNumber();
                Resolver.Log.Info($"T3-22i SN: {sn}");
            }
            catch (Exception ex)
            {
                Resolver.Log.Error($"Unable to connect to T3-22i: {ex.Message}");
                T3Module = null;
            }
        }

        if (T3Module != null)
        {
            foreach (var analog in moduleConfig.Channels)
            {
                try
                {
                    var capture = analog;
                    var id = GenerateSensorId(analog, analog.Name);

                    switch (analog.ChannelType)
                    {
                        case ConfigurableAnalogInputChannelType.Current_4_20:
                        case ConfigurableAnalogInputChannelType.Current_0_20:
                            // verify the pin is valid
                            var pin = T3Module.Pins.FirstOrDefault(p => (int)p.Key == analog.ChannelNumber);
                            if (pin == null)
                            {
                                Resolver.Log.Error($"No T3 Pin for requested channel {analog.ChannelNumber}");
                                break;
                            }
                            // create an input
                            var cinput = await T3Module.CreateCurrentInputPort(pin);
                            // register the input for reading
                            AddSensorToQueryList(analog.SenseIntervalSeconds, new(id, cinput, () =>
                            {
                                try
                                {
                                    var rawCurrent = cinput.Read().GetAwaiter().GetResult();
                                    return InputToUnitConverter.ConvertCurrentToUnit(
                                        rawCurrent,
                                        analog.UnitType,
                                        analog.Scale,
                                        analog.Offset);
                                }
                                catch (Exception rex)
                                {
                                    Resolver.Log.Error($"Failed to read analog input channel {cinput.Pin.Name}: {rex.Message}");
                                    return null;
                                }
                            }));
                            break;
                        case ConfigurableAnalogInputChannelType.Voltage_0_10:
                            break;
                    }

                }
                catch (Exception ex)
                {
                    // TODO: log this!
                    Resolver.Log.Error($"Failed to configure analog input channel {analog.ChannelNumber}");
                }
            }
        }
    }

    private void ConfigureConfigurableAnalogs(AnalogModuleConfig? moduleConfig)
    {
        if (moduleConfig == null)
        {
            Resolver.Log.Warn($"No AnalogModuleConfig exists for this device");
            return;
        }

        if (moduleConfig.IsSimulated)
        {
            var m = new SimulatedProgrammableAnalogInputModule();
            m.StartSimulation();
            ProgrammableAnalogInputModule = m;
        }
        else
        {
            throw new NotSupportedException("Configurable Analog Inputs not supported");
            //module = new ProgrammableAnalogInputModule();
        }

        foreach (var analog in moduleConfig.Channels)
        {
            try
            {
                ProgrammableAnalogInputModule.ConfigureChannel(analog);
                var id = GenerateSensorId(analog, analog.Name);
                var capture = analog;
                AddSensorToQueryList(analog.SenseIntervalSeconds, new(id, ProgrammableAnalogInputModule, () =>
                {
                    return ProgrammableAnalogInputModule.ReadChannelAsConfiguredUnit(capture.ChannelNumber);
                }));
            }
            catch (Exception ex)
            {
                // TODO: log this!
                Resolver.Log.Error($"Failed to configure analog input channel {analog.ChannelNumber}");
            }
        }
    }

    private void ConfigureFrequencyInputs(IEnumerable<FrequencyInputConfig> inputConfigs)
    {
        foreach (var input in inputConfigs)
        {
            if (input.IsSimulated)
            {
                var sensor = new SimulatedHallEffectFlowSensor();
                sensor.StartSimulation(SimulationBehavior.Sawtooth);
                var id = GenerateSensorId(sensor, input.Name);
                FlowSensors.Add(id, sensor);
                lock (queryPeriodDictionary)
                {
                    if (!queryPeriodDictionary.ContainsKey(input.SenseIntervalSeconds))
                    {
                        queryPeriodDictionary.Add(input.SenseIntervalSeconds, new List<(int ID, object Sensor, Func<object?>)>());
                    }
                    queryPeriodDictionary[input.SenseIntervalSeconds].Add(new(id, sensor, () => sensor.Read().GetAwaiter().GetResult()));
                }
            }
            else
            {
                throw new NotSupportedException("Non-simulated frequency inputs are not supported on this platform");
            }
        }
    }

    private async Task SensorReadProc()
    {
        int tick = 0;

        var telemetryList = new Dictionary<string, object>();

        while (true)
        {
            telemetryList.Clear();

            lock (queryPeriodDictionary)
            {
                foreach (var period in queryPeriodDictionary.Keys)
                {
                    if (tick % period == 0)
                    {
                        foreach (var sensor in queryPeriodDictionary[period])
                        {
                            var value = sensor.ReadDelegate();

                            if (value == null)
                            {
                                Resolver.Log.Info($"Error reading from {sensor.Sensor.GetType().Name}");
                                continue;
                            }

                            if (value is Dictionary<string, object> valueDictionary)
                            {
                                Resolver.Log.Info($"[{tick:D4}]Composite sensor returned {valueDictionary.Count} values");

                                foreach (var sensorItem in valueDictionary)
                                {
                                    // we have to create a separate ID for each value because the units can vary between items
                                    var valueId = GenerateSensorId(sensorItem, sensorItem.Key);
                                    if (!canonicalUnitsForSensorsMap.ContainsKey(valueId))
                                    {
                                        var canonicalUnit = GetCanonicalUnitTypeValue(sensorItem.Value);
                                        canonicalUnitsForSensorsMap.Add(valueId, canonicalUnit);
                                    }

                                    try
                                    {
                                        if (sensorItem.Value is IUnit unit)
                                        {
                                            telemetryList.Add(sensorIdToNameMap[valueId], unit.ToCanonical());
                                        }
                                        else
                                        {
                                            telemetryList.Add(sensorIdToNameMap[valueId], value);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        // TODO: log this
                                    }
                                }
                            }
                            else
                            {
                                if (!canonicalUnitsForSensorsMap.ContainsKey(sensor.ID))
                                {
                                    var canonicalUnit = GetCanonicalUnitTypeValue(value);
                                    canonicalUnitsForSensorsMap.Add(sensor.ID, canonicalUnit);
                                }

                                Resolver.Log.Info($"[{tick:D4}]Read sensor: {sensor.ID}:{sensor.GetType().Name}:{value}:{canonicalUnitsForSensorsMap[sensor.ID]}");

                                try
                                {
                                    if (value is IUnit unit)
                                    {
                                        telemetryList.Add(sensorIdToNameMap[sensor.ID], unit.ToCanonical());
                                    }
                                    else
                                    {
                                        telemetryList.Add(sensorIdToNameMap[sensor.ID], value);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // TODO: log this
                                }
                            }
                        }
                    }
                }
            }
            storageController.RecordSensorValues(telemetryList);

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