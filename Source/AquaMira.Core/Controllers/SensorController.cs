using AquaMira.Core.Contracts;
using Meadow;
using Meadow.Peripherals.Sensors;
using Meadow.Units;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AquaMira.Core;

public class SensorController
{
    private readonly IAquaMiraHardware hardware;

    private readonly Dictionary<int, List<ISensingNode>> sensingNodes = new();
    private readonly ConfigurationController configurationController;
    private readonly Dictionary<string, ISensingNodeController> registeredSensingNodeControllers = new();
    private readonly SemaphoreSlim nodeSemaphore = new(1, 1);
    private bool controllersLoaded = false;

    public event EventHandler<Dictionary<string, object>>? SensorValuesUpdated;

    public Dictionary<int, IVolumetricFlowSensor> FlowSensors { get; } = new();
    public Task SensorProc { get; set; }

    public SensorController(IAquaMiraHardware hardware, ConfigurationController configurationController)
    {
        this.hardware = hardware;
        this.configurationController = configurationController;
    }

    public void RegisterSensingNodeController<TController>(string configurationName)
        where TController : ISensingNodeController, new()
    {
        RegisterSensingNodeController(typeof(TController), configurationName);
    }

    public void RegisterSensingNodeController((Type ControllerType, string ConfigurationName) descriptor)
    {
        RegisterSensingNodeController(descriptor.ControllerType, descriptor.ConfigurationName);
    }

    public void RegisterSensingNodeController(Type controllerType, string configurationName)
    {
        if (!controllerType.Implements<ISensingNodeController>())
        {
            throw new ArgumentException($"Type {controllerType.Name} does not implement ISensingNodeController");
        }

        nodeSemaphore.Wait();
        var key = configurationName;
        try
        {
            if (registeredSensingNodeControllers.ContainsKey(key))
            {
                Resolver.Log.Warn($"Sensing node controller {key} is already registered", Constants.LoggingSource);
                return;
            }

            var configJson = configurationController.GetConfigurationNode(configurationName);

            if (configJson == null)
            {
                Resolver.Log.Warn($"No configuration found for {configurationName}", Constants.LoggingSource);
            }

            // Create instance using reflection (since we can't use generics here)
            var controller = (ISensingNodeController)Activator.CreateInstance(controllerType)!;

            registeredSensingNodeControllers.Add(key, controller);

            Resolver.Log.Info($"Registered sensing node controller: {key}", Constants.LoggingSource);
        }
        catch (Exception ex)
        {
            Resolver.Log.Error($"Failed to register sensing node controller {key}: {ex.Message}", Constants.LoggingSource);
        }
        finally
        {
            nodeSemaphore.Release();
        }
    }

    public async Task LoadSensingNodeControllers(IAquaMiraHardware hardware)
    {
        // make sure this is called once and only once.  Ever.
        if (controllersLoaded)
        {
            Resolver.Log.Warn("Sensing node controllers have already been loaded", Constants.LoggingSource);
            return;
        }

        await nodeSemaphore.WaitAsync();
        try
        {
            var configurationTasks = new List<Task>();

            foreach (var key in registeredSensingNodeControllers.Keys)
            {
                var configJson = configurationController.GetConfigurationNode(key);
                if (configJson == null)
                {
                    Resolver.Log.Warn($"No configuration found for {key}", Constants.LoggingSource);
                    continue;
                }

                // it's possible that a node controller cannot come up immediately (i.e. a communication bus is unavailable, etc)
                // so we need to background the configure call and add the nodes as they become available.
                var task = ConfigureController(key, configJson, hardware);
                configurationTasks.Add(task);
            }

            // Wait for all configuration tasks to complete
            controllersLoaded = true;
        }
        finally
        {
            nodeSemaphore.Release();
        }
    }

    private async Task ConfigureController(string key, string configJson, IAquaMiraHardware hardware)
    {
        try
        {
            var nodes = await registeredSensingNodeControllers[key].ConfigureFromJson(configJson, hardware);
            AddSensingNodes(nodes);
        }
        catch (Exception ex)
        {
            Resolver.Log.Error($"Error loading sensing node controller {key}: {ex.Message}", Constants.LoggingSource);
        }
    }

    internal void Start()
    {
        if (!controllersLoaded && registeredSensingNodeControllers.Count > 0)
        {
            Resolver.Log.Warn("Sensing node controllers have not been loaded. Call LoadSensingNodeControllers() before starting the sensor controller.", Constants.LoggingSource);
        }

        SensorProc = Task.Run(SensorReadProc);
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
            Resolver.Log.Info($"Adding sensing node {node.Name} with period {node.QueryPeriod.TotalSeconds} seconds", Constants.LoggingSource);

            var interval = (int)node.QueryPeriod.TotalSeconds;

            if (!sensingNodes.ContainsKey(interval))
            {
                sensingNodes.Add(interval, new List<ISensingNode>());
            }
            sensingNodes[interval].Add(node);
        }
    }

    /// <summary>
    /// This method walks through list of sensors in the to read list
    /// calls the function that does the reading, and then saves the results
    /// to the telemetry list. Finally, it passes the telemetry data to the 
    /// storage controller.
    /// </summary>
    /// <returns></returns>
    private async Task SensorReadProc()
    {
        int tick = 0;

        var telemetryList = new Dictionary<string, object>();

        while (!Resolver.App.CancellationToken.IsCancellationRequested)
        {
            telemetryList.Clear();

            lock (sensingNodes)
            {
                try
                {
                    foreach (var period in sensingNodes.Keys)
                    {
                        if (tick % period == 0)
                        {
                            foreach (var node in sensingNodes[period])
                            {
                                Resolver.Log.Debug($"Reading sensor {node.Name}...", Constants.LoggingSource);

                                if (node is IUnitizedSensingNode usn)
                                {
                                    try
                                    {
                                        var value = usn.ReadAsCanonicalUnit();

                                        if (!telemetryList.ContainsKey(usn.Name))
                                        {
                                            telemetryList.Add(usn.Name, value);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Resolver.Log.Error($"Error reading from node: {node.Name}: {ex.Message}", Constants.LoggingSource);
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
                                            Resolver.Log.Info($"Error reading from {node.Sensor.GetType().Name}", Constants.LoggingSource);
                                            continue;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Resolver.Log.Error($"Error reading from node: {node.Name}: {ex.Message}", Constants.LoggingSource);
                                        continue;
                                    }

                                    if (value is Dictionary<string, object> valueDictionary)
                                    {
                                        foreach (var sensorItem in valueDictionary)
                                        {
                                            try
                                            {
                                                if (sensorItem.Value is IUnit unit)
                                                {
                                                    if (!telemetryList.ContainsKey(sensorItem.Key))
                                                    {
                                                        telemetryList.Add(sensorItem.Key, unit.ToCanonical());
                                                    }
                                                }
                                                else
                                                {
                                                    if (!telemetryList.ContainsKey(sensorItem.Key))
                                                    {
                                                        telemetryList.Add(sensorItem.Key, sensorItem.Value);
                                                    }
                                                }
                                            }
                                            catch (Exception vdx)
                                            {
                                                Resolver.Log.Warn($"Failed to read {sensorItem.Value}: {vdx.Message}", Constants.LoggingSource);
                                            }
                                        }
                                    }
                                    else if (value is IUnit unitValue)
                                    {
                                        if (!telemetryList.ContainsKey(node.Name))
                                        {
                                            try
                                            {
                                                telemetryList.Add(node.Name, unitValue.ToCanonical());
                                            }
                                            catch (Exception uvx)
                                            {
                                                Resolver.Log.Warn($"Failed to read {node.Name}: {uvx.Message}", Constants.LoggingSource);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Resolver.Log.Warn($"Sensor {node.Name} returned a value of type {value.GetType().Name} which is not a recognized unit type. Returning as-is.", Constants.LoggingSource);
                                    }
                                }
                                else
                                {
                                    Resolver.Log.Warn($"Sensor {node.Name} is not a recognized sensing node type. Cannot read value.", Constants.LoggingSource);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Resolver.Log.Error($"Error reading telemetry: {ex.Message}", Constants.LoggingSource);
                }
            }

            if (telemetryList.Count > 0)
            {
                SensorValuesUpdated?.Invoke(this, telemetryList);
            }

            if (sensingNodes.Count > 0)
            {
                tick++;
            }
            await Task.Delay(1000);
        }

        Resolver.Log.Error($"Sensor read loop exited", Constants.LoggingSource);
    }
}