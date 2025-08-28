using AquaMira.Core;
using AquaMira.Core.Contracts;
using Meadow;
using Meadow.Foundation.Sensors.Environmental;
using Meadow.Foundation.Serialization;
using Meadow.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AquaMira;

public class KellerTransducerNodeController
{
    private IKellerTransducer? kellerTransducer;

    public Task<IEnumerable<ISensingNode>> ConfigureFromJson(string configJson, IAquaMiraHardware hardware)
    {
        Resolver.Log?.Info("Configuring KellerTransducer node controller from JSON");

        KellerConfiguration config;
        try
        {
            config = MicroJson.Deserialize<KellerConfiguration>(configJson);
            if (config == null)
            {
                Resolver.Log?.Error("Failed to deserialize KellerConfiguration configuration from JSON");
                return Task.FromResult(Enumerable.Empty<ISensingNode>());
            }
        }
        catch (Exception ex)
        {
            Resolver.Log?.Error("Failed to deserialize KellerConfiguration configuration from JSON");
            return Task.FromResult(Enumerable.Empty<ISensingNode>());
        }

        if (config.IsSimulated)
        {
            kellerTransducer = new SimulatedKellerTransducer();
        }
        else
        {
            kellerTransducer = new KellerTransducer(
                hardware.GetModbusSerialClient(),
                (byte)config.ModbusAddress);
        }

        var nodes = new List<ISensingNode>
        {
            new UnitizedSensingNode<Pressure>(
                $"{config.Name}.PressureChannel1",
                kellerTransducer,
                () => kellerTransducer.ReadPressure(PressureChannel.P1).GetAwaiter().GetResult(),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)),
            new UnitizedSensingNode<Temperature>(
                $"{config.Name}.TemperatureChannel1",
                kellerTransducer,
                () => kellerTransducer.ReadTemperature(TemperatureChannel.TOB1).GetAwaiter().GetResult(),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds))
        };

        return Task.FromResult<IEnumerable<ISensingNode>>(nodes);
    }
}
