using Meadow.Peripherals.Sensors;
using Sensors.Flow.HallEffect.Simulation;
using System;
using System.Collections.Generic;
using Thurston_Monitor.Core.Contracts;

namespace Thurston_Monitor.Core;

public class SensorController
{
    private readonly IThurston_MonitorHardware hardware;

    public Dictionary<string, IVolumetricFlowSensor> FlowSensors { get; } = new();

    public SensorController(IThurston_MonitorHardware hardware)
    {
        this.hardware = hardware;
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
                FlowSensors.Add(input.Description, sensor);
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
}