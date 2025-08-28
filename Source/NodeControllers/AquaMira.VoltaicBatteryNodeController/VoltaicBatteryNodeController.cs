using AquaMira.Core;
using AquaMira.Core.Contracts;
using Meadow;
using Meadow.Foundation.Batteries.Voltaic;
using Meadow.Foundation.Serialization;
using Meadow.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AquaMira;

public class VoltaicBatteryNodeController
{
    private IV10x? battery;

    public Task<IEnumerable<ISensingNode>> ConfigureFromJson(string configJson, IAquaMiraHardware hardware)
    {
        Resolver.Log?.Info("Configuring Voltaic battery node controller from JSON");

        VoltaicConfiguration config;
        try
        {
            config = MicroJson.Deserialize<VoltaicConfiguration>(configJson);
            if (config == null)
            {
                Resolver.Log?.Error("Failed to deserialize VoltaicConfiguration configuration from JSON");
                return Task.FromResult(Enumerable.Empty<ISensingNode>());
            }
        }
        catch (Exception ex)
        {
            Resolver.Log?.Error("Failed to deserialize VoltaicConfiguration configuration from JSON");
            return Task.FromResult(Enumerable.Empty<ISensingNode>());
        }

        if (config.IsSimulated)
        {
            throw new NotImplementedException("No simulated Voltaic battery implementation available");
            // battery = new SimulatedVoltaicBattery();
        }
        else
        {
            battery = new V10x(
                hardware.GetModbusSerialClient(),
                (byte)config.ModbusAddress);
        }

        var nodes = new List<ISensingNode>
        {
            new UnitizedSensingNode<Current>(
                $"{config.Name}.InputCurrent",
                battery,
                () => battery.InputCurrent,
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)),
            new UnitizedSensingNode<Voltage>(
                $"{config.Name}.BatteryVoltage",
                battery,
                () => battery.BatteryVoltage,
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)),
            new UnitizedSensingNode<Current>(
                $"{config.Name}.InputCurrent",
                battery,
                () => battery.InputCurrent,
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)),
            new UnitizedSensingNode<Voltage>(
                $"{config.Name}.InputVoltage",
                battery,
                () => battery.InputVoltage,
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)),
            new UnitizedSensingNode<Temperature>(
                $"{config.Name}.ControllerTemp",
                battery,
                () => battery.ControllerTemp,
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)),
            new UnitizedSensingNode<Temperature>(
                $"{config.Name}.EnvironmentTemp",
                battery,
                () => battery.EnvironmentTemp,
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)),
        };

        return Task.FromResult<IEnumerable<ISensingNode>>(nodes);
    }
}
