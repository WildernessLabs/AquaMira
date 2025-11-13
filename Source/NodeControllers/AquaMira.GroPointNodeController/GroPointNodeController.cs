using AquaMira.Core;
using AquaMira.Core.Contracts;
using Meadow;
using Meadow.Foundation.Sensors.Environmental.GroPoint;
using Meadow.Foundation.Serialization;
using Meadow.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AquaMira;

public class GroPointNodeController : ISensingNodeController
{
    private Gplp2625? sensor;
    private Temperature[] temperatures;
    private float[] moistures;

    public async Task<IEnumerable<ISensingNode>> ConfigureFromJson(string configJson, IAquaMiraHardware hardware)
    {
        Resolver.Log?.Info("Configuring GroPoint node controller from JSON");

        GroPointConfiguration config;
        try
        {
            config = MicroJson.Deserialize<GroPointConfiguration>(configJson);
            if (config == null)
            {
                Resolver.Log?.Error("Failed to deserialize GroPointConfiguration configuration from JSON");
                return Enumerable.Empty<ISensingNode>();
            }
        }
        catch (Exception ex)
        {
            Resolver.Log?.Error("Failed to deserialize GroPointConfiguration configuration from JSON");
            return Enumerable.Empty<ISensingNode>();
        }

        if (config.IsSimulated)
        {
            throw new NotImplementedException("GroPoint simulation is not implemented");
        }
        else
        {
            sensor = new Gplp2625_S_T_8(
                    hardware.GetModbusSerialClient(),
                    (byte)config.ModbusAddress);

            // TODO: support other GroPoint sensors as needed
            //var sensor = config.SensorType.ToLower() switch
            //{
            //    _ => new Gplp2625_S_T_8(
            //        hardware.GetModbusSerialClient(),
            //        (byte)config.ModbusAddress),
            //};
        }

        var nodes = new List<ISensingNode>();
        for (var i = 0; i < sensor.TempSensorCount; i++)
        {
            var sensorIndex = i; // capture for closure
            nodes.Add(
                new UnitizedSensingNode<Temperature>(
                    $"{config.Name}.SoilTemp{sensorIndex + 1}",
                    sensor,
                    () => Task.FromResult<IUnit?>(temperatures[sensorIndex]),
                    TimeSpan.FromSeconds(config.SenseIntervalSeconds)
                ));
        }
        for (var i = 0; i < sensor.MoistureSegmentCount; i++)
        {
            var sensorIndex = i; // capture for closure
            nodes.Add(
                new UnitizedSensingNode<Scalar>(
                    $"{config.Name}.SoilMoisture{sensorIndex + 1}",
                    sensor,
                    () => Task.FromResult<IUnit?>(new Scalar(moistures[sensorIndex])),
                    TimeSpan.FromSeconds(config.SenseIntervalSeconds)
                ));
        }

        var readTimer = new Timer(
            async _ => await ManageSensorData(),
            null,
            0,
            config.SenseIntervalSeconds * 500); // read 2x the interval


        return nodes;
    }

    private async Task ManageSensorData()
    {
        if (sensor != null)
        {
            try
            {
                temperatures = await sensor.ReadTemperatures();
                moistures = await sensor.ReadMoistures();
            }
            catch (Exception ex)
            {
                Resolver.Log?.Error($"Error reading GroPoint sensor data: {ex.Message}");
            }
        }
    }
}