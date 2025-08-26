using AquaMira.Core;
using AquaMira.Core.Contracts;
using Meadow;
using Meadow.Foundation.Sensors.Environmental.Ysi;
using Meadow.Foundation.Serialization;
using Meadow.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AquaMira;

public class YsiSondeNodeController : ISensingNodeController
{
    private IExoSonde? sonde;
    private Temperature? sondeTemp = null;
    private Scalar chlor;
    private Scalar cond;
    private Scalar lfCond;
    private Scalar doSat;
    private Scalar domgL;
    private Scalar salintiy;
    private Scalar spCond;
    private Scalar bgarfu;
    private Scalar tds;
    private Scalar turb;
    private Scalar tss;
    private Scalar wiper;
    private Scalar pH;
    private Scalar pHmV;
    private Scalar battery;
    private Scalar cablePower;
    private Timer readTimer;

    public Task<IEnumerable<ISensingNode>> ConfigureFromJson(string configJson, IAquaMiraHardware hardware)
    {
        YsiSondeConfiguration config;
        try
        {
            config = MicroJson.Deserialize<YsiSondeConfiguration>(configJson);
            if (config == null)
            {
                Resolver.Log?.Error("Failed to deserialize YsiSondConfiguration configuration from JSON");
                return Task.FromResult(Enumerable.Empty<ISensingNode>());
            }
        }
        catch (Exception ex)
        {
            Resolver.Log?.Error($"Failed to deserialize YsiSondConfiguration configuration from JSON: {ex.Message}");
            return Task.FromResult(Enumerable.Empty<ISensingNode>());
        }

        if (config.IsSimulated)
        {
            sonde = new SimulatedExo();
        }
        else
        {
            sonde = new Exo(
                hardware.GetModbusSerialClient(),
                (byte)config.ModbusAddress);
        }

        var nodes = new List<ISensingNode>();

        if (config.Parameters.Contains("Temperature", StringComparer.OrdinalIgnoreCase) ||
           config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<Temperature>(
                $"{config.Name}.Temperature",
                sonde,
                () => sondeTemp,
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }
        if (config.Parameters.Contains("Chloriform", StringComparer.OrdinalIgnoreCase) ||
           config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<Scalar>(
                $"{config.Name}.Chloriform",
                sonde,
                () => chlor,
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }
        if (config.Parameters.Contains("Conductivity", StringComparer.OrdinalIgnoreCase) ||
           config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<Scalar>(
                $"{config.Name}.Conductivity",
                sonde,
                () => cond,
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }
        if (config.Parameters.Contains("LFChloriform", StringComparer.OrdinalIgnoreCase) ||
           config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<Scalar>(
                $"{config.Name}.LFChloriform",
                sonde,
                () => lfCond,
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }
        if (config.Parameters.Contains("DOSat", StringComparer.OrdinalIgnoreCase) ||
           config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<Scalar>(
                $"{config.Name}.DOSat",
                sonde,
                () => doSat,
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }
        if (config.Parameters.Contains("DOmgL", StringComparer.OrdinalIgnoreCase) ||
           config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<Scalar>(
                $"{config.Name}.DOmgL",
                sonde,
                () => domgL,
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }
        if (config.Parameters.Contains("Salinity", StringComparer.OrdinalIgnoreCase) ||
           config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<Scalar>(
                $"{config.Name}.Salinity",
                sonde,
                () => salintiy,
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }
        if (config.Parameters.Contains("SpCond", StringComparer.OrdinalIgnoreCase) ||
              config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<Scalar>(
                $"{config.Name}.SpCond",
                sonde,
                () => spCond,
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }
        if (config.Parameters.Contains("BGARFU", StringComparer.OrdinalIgnoreCase) ||
              config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<Scalar>(
                $"{config.Name}.BGARFU",
                sonde,
                () => bgarfu,
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }
        if (config.Parameters.Contains("TDS", StringComparer.OrdinalIgnoreCase) ||
              config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<Scalar>(
            $"{config.Name}.TDS",
            sonde,
            () => tds,
            TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }
        if (config.Parameters.Contains("Turbidity", StringComparer.OrdinalIgnoreCase) ||
              config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<Scalar>(
                $"{config.Name}.Turbidity",
                sonde,
                () => turb,
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }
        if (config.Parameters.Contains("TSS", StringComparer.OrdinalIgnoreCase) ||
              config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<Scalar>(
                $"{config.Name}.TSS",
                sonde,
                () => tss,
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }
        if (config.Parameters.Contains("WiperPos", StringComparer.OrdinalIgnoreCase) ||
              config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<Scalar>(
                $"{config.Name}.WiperPos",
                sonde,
                () => wiper,
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }
        if (config.Parameters.Contains("pH", StringComparer.OrdinalIgnoreCase) ||
              config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<Scalar>(
                $"{config.Name}.pH",
                sonde,
                () => pH,
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }
        if (config.Parameters.Contains("pHmV", StringComparer.OrdinalIgnoreCase) ||
              config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<Scalar>(
                $"{config.Name}.pHmV",
                sonde,
                () => pHmV,
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }
        if (config.Parameters.Contains("Battery", StringComparer.OrdinalIgnoreCase) ||
              config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<Scalar>(
                $"{config.Name}.Battery",
                sonde,
                () => battery,
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }
        if (config.Parameters.Contains("CablePower", StringComparer.OrdinalIgnoreCase) ||
              config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<Scalar>(
                $"{config.Name}.CablePower",
                sonde,
                () => cablePower,
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }

        readTimer = new Timer(
            async _ => await ManageSondeData(),
            null,
            0,
            config.SenseIntervalSeconds * 500); // read 2x the interval

        return Task.FromResult<IEnumerable<ISensingNode>>(nodes);
    }

    private async Task ManageSondeData()
    {
        if (sonde != null)
        {
            try
            {
                Resolver.Log.Trace($"Reading sonde information...");
                var data = await sonde.GetCurrentData();

                var tempFTuple = data.FirstOrDefault(d => d.ParameterCode == ParameterCode.TemperatureC);
                var chlorTuple = data.FirstOrDefault(d => d.ParameterCode == ParameterCode.ChlorophyllRFU);
                var condTuple = data.FirstOrDefault(d => d.ParameterCode == ParameterCode.ConductivityuScm);
                var lfCondTuple = data.FirstOrDefault(d => d.ParameterCode == ParameterCode.nLFConductivityuScm);
                var doSatTuple = data.FirstOrDefault(d => d.ParameterCode == ParameterCode.ODOPercentSat);
                var momgLTuple = data.FirstOrDefault(d => d.ParameterCode == ParameterCode.ODOmgL);
                var salTuple = data.FirstOrDefault(d => d.ParameterCode == ParameterCode.Salinity);
                var spCondTuple = data.FirstOrDefault(d => d.ParameterCode == ParameterCode.SpecificConductanceuScm);
                var bgarfuTuple = data.FirstOrDefault(d => d.ParameterCode == ParameterCode.BGAPErfu);
                var tdstuple = data.FirstOrDefault(d => d.ParameterCode == ParameterCode.TDSmgL);
                var turbTuple = data.FirstOrDefault(d => d.ParameterCode == ParameterCode.TurbidityFNU);
                var tssTuple = data.FirstOrDefault(d => d.ParameterCode == ParameterCode.TSSmgL);
                var wiperTuple = data.FirstOrDefault(d => d.ParameterCode == ParameterCode.WiperPosition);
                var pHTuple = data.FirstOrDefault(d => d.ParameterCode == ParameterCode.pH);
                var pHmVTuple = data.FirstOrDefault(d => d.ParameterCode == ParameterCode.pHmV);
                var batteryTuple = data.FirstOrDefault(d => d.ParameterCode == ParameterCode.BatteryVoltage);
                var cablePowerTuple = data.FirstOrDefault(d => d.ParameterCode == ParameterCode.ExternalPower);

                if (tempFTuple != default)
                {
                    sondeTemp = new Temperature(Convert.ToDouble(tempFTuple.Value), Temperature.UnitType.Celsius);
                }
                if (sondeTemp == null)
                {
                    sondeTemp = new Temperature(Convert.ToDouble(-99));
                }

                if (chlorTuple != default)
                {
                    chlor = (Scalar)(double)chlorTuple.Value;
                }

                if (condTuple != default)
                {
                    cond = (Scalar)(float)condTuple.Value;
                }

                if (lfCondTuple != default)
                {
                    lfCond = (Scalar)(float)lfCondTuple.Value;
                }

                if (doSatTuple != default)
                {
                    doSat = (Scalar)(float)doSatTuple.Value;
                }

                if (momgLTuple != default)
                {
                    domgL = (Scalar)(float)momgLTuple.Value;
                }

                if (salTuple != default)
                {
                    salintiy = (Scalar)(float)salTuple.Value;
                }

                if (spCondTuple != default)
                {
                    spCond = (Scalar)(float)spCondTuple.Value;
                }

                if (bgarfuTuple != default)
                {
                    bgarfu = (Scalar)(float)bgarfuTuple.Value;
                }

                if (tdstuple != default)
                {
                    tds = (Scalar)(float)tdstuple.Value;
                }

                if (turbTuple != default)
                {
                    turb = (Scalar)(float)turbTuple.Value;
                }

                if (tssTuple != default)
                {
                    tss = (Scalar)(float)tssTuple.Value;
                }

                if (wiperTuple != default)
                {
                    wiper = (Scalar)(float)wiperTuple.Value;
                }

                if (pHTuple != default)
                {
                    pH = (Scalar)(float)pHTuple.Value;
                }

                if (pHmVTuple != default)
                {
                    pHmV = (Scalar)(float)pHmVTuple.Value;
                }

                if (batteryTuple != default)
                {
                    battery = (Scalar)(float)batteryTuple.Value;
                }

                if (cablePowerTuple != default)
                {
                    cablePower = (Scalar)(float)cablePowerTuple.Value;
                }
            }
            catch (Exception ex)
            {
                Resolver.Log.Warn($"Unable to read sonde information: {ex.Message}");
            }
        }
    }
}
