using AquaMira.Core;
using AquaMira.Core.Contracts;
using Meadow;
using Meadow.Foundation.Sensors.Power;
using Meadow.Foundation.Serialization;
using Meadow.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AquaMira;

public class SPM1xPowerNodeController : ISensingNodeController
{
    private ISpm1x? spm1x;

    public Task<IEnumerable<ISensingNode>> ConfigureFromJson(string configJson, IAquaMiraHardware hardware)
    {
        Resolver.Log?.Info("Configuring SPM1x power node controller from JSON");

        SPM1xConfiguration config;
        try
        {
            config = MicroJson.Deserialize<SPM1xConfiguration>(configJson);
            if (config == null)
            {
                Resolver.Log?.Error("Failed to deserialize SPM1x configuration from JSON");
                return Task.FromResult(Enumerable.Empty<ISensingNode>());
            }
        }
        catch (Exception ex)
        {
            Resolver.Log?.Error("Failed to deserialize SPM1x configuration from JSON");
            return Task.FromResult(Enumerable.Empty<ISensingNode>());
        }

        if (config.IsSimulated)
        {
            spm1x = new SimulatedSpm1x();
        }
        else
        {
            spm1x = new Spm1x(
                hardware.GetModbusSerialClient(),
                (byte)config.ModbusAddress);
        }

        var currentNode = new UnitizedSensingNode<Current>(
            $"{config.Name}.Current",
            spm1x,
            async () =>
            {
                try
                {
                    return await spm1x.ReadCurrent();
                }
                catch (TimeoutException)
                {
                    // timeouts shouldn't log to the cloud
                    return null;
                }
            },
            TimeSpan.FromSeconds(config.SenseIntervalSeconds));

        var voltageNode = new UnitizedSensingNode<Voltage>(
            $"{config.Name}.Voltage",
            spm1x,
            async () =>
            {
                try
                {
                    return await spm1x.ReadVoltage();
                }
                catch (TimeoutException)
                {
                    // timeouts shouldn't log to the cloud
                    return null;
                }
            },
            TimeSpan.FromSeconds(config.SenseIntervalSeconds));

        return Task.FromResult<IEnumerable<ISensingNode>>(
            new ISensingNode[]
            {
                currentNode,
                voltageNode
            });
    }
}
