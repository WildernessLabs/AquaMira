using AquaMira.Core;
using AquaMira.Core.Contracts;
using Meadow;
using Meadow.Foundation.Serialization;
using Meadow.Foundation.VFDs;
using Meadow.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AquaMira;

public class CerusNodeController : ISensingNodeController
{
    private IXDrive? cerusXDrive;

    public async Task<IEnumerable<ISensingNode>> ConfigureFromJson(string configJson, IAquaMiraHardware hardware)
    {
        CerusConfiguration config;
        try
        {
            config = MicroJson.Deserialize<CerusConfiguration>(configJson);
            if (config == null)
            {
                Resolver.Log?.Error("Failed to deserialize CerusConfiguration configuration from JSON");
                return Enumerable.Empty<ISensingNode>();
            }
        }
        catch (Exception ex)
        {
            Resolver.Log?.Error("Failed to deserialize CerusConfiguration configuration from JSON");
            return Enumerable.Empty<ISensingNode>();
        }

        if (config.IsSimulated)
        {
            cerusXDrive = new SimulatedXDrive();
        }
        else
        {
            cerusXDrive = new CerusXDrive(
                hardware.GetModbusSerialClient(),
                (byte)config.ModbusAddress);
        }

        await cerusXDrive.Connect();

        var nodes = new List<ISensingNode>
        {
            new UnitizedSensingNode<Current>(
                $"{config.Name}.OutputCurrent",
                cerusXDrive,
                () => cerusXDrive.ReadOutputCurrent().GetAwaiter().GetResult(),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)),
            new UnitizedSensingNode<Temperature>(
                $"{config.Name}.AmbientTemp",
                cerusXDrive,
                () => cerusXDrive.ReadAmbientTemperature().GetAwaiter().GetResult(),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)),
            new UnitizedSensingNode<Voltage>(
                $"{config.Name}.DCBusVoltage",
                cerusXDrive,
                () => cerusXDrive.ReadDCBusVoltage().GetAwaiter().GetResult(),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)),
            new UnitizedSensingNode<Frequency>(
                $"{config.Name}.OutputFrequency",
                cerusXDrive,
                () => cerusXDrive.ReadOutputFrequency().GetAwaiter().GetResult(),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)),
            new UnitizedSensingNode<Voltage>(
                $"{config.Name}.OutputVoltage",
                cerusXDrive,
                () => cerusXDrive.ReadOutputVoltage().GetAwaiter().GetResult(),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)),
        };

        return nodes;
    }
}