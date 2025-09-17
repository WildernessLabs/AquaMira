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
    private Scalar? chlor;
    private Scalar? cond;
    private Scalar? lfCond;
    private Scalar? doSat;
    private Scalar? domgL;
    private Scalar? salintiy;
    private readonly Scalar? spCond;
    private Scalar? bgarfu;
    private readonly Scalar? tds;
    private readonly Scalar? turb;
    private readonly Scalar? tss;
    private readonly Scalar? wiper;
    private readonly Scalar? pH;
    private readonly Scalar? pHmV;
    private readonly Scalar? battery;
    private readonly Scalar? cablePower;
    private Timer readTimer;
    private bool reportCompleted = false;

    public Task<IEnumerable<ISensingNode>> ConfigureFromJson(string configJson, IAquaMiraHardware hardware)
    {
        Resolver.Log?.Info("Configuring YsiSonde node controller from JSON");

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
                () => Task.FromResult<IUnit?>(sondeTemp),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }
        if (config.Parameters.Contains("Chloriform", StringComparer.OrdinalIgnoreCase) ||
           config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<Scalar>(
                $"{config.Name}.Chloriform",
                sonde,
                () => Task.FromResult<IUnit?>(chlor),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }
        if (config.Parameters.Contains("Conductivity", StringComparer.OrdinalIgnoreCase) ||
           config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<Scalar>(
                $"{config.Name}.Conductivity",
                sonde,
                () => Task.FromResult<IUnit?>(cond),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }
        if (config.Parameters.Contains("LFChloriform", StringComparer.OrdinalIgnoreCase) ||
           config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<Scalar>(
                $"{config.Name}.LFChloriform",
                sonde,
                () => Task.FromResult<IUnit?>(lfCond),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }
        if (config.Parameters.Contains("DOSat", StringComparer.OrdinalIgnoreCase) ||
           config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<Scalar>(
                $"{config.Name}.DOSat",
                sonde,
                () => Task.FromResult<IUnit?>(doSat),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }
        if (config.Parameters.Contains("DOmgL", StringComparer.OrdinalIgnoreCase) ||
           config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<Scalar>(
                $"{config.Name}.DOmgL",
                sonde,
                () => Task.FromResult<IUnit?>(domgL),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }
        if (config.Parameters.Contains("Salinity", StringComparer.OrdinalIgnoreCase) ||
           config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<Scalar>(
                $"{config.Name}.Salinity",
                sonde,
                () => Task.FromResult<IUnit?>(salintiy),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }
        if (config.Parameters.Contains("SpCond", StringComparer.OrdinalIgnoreCase) ||
              config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<Scalar>(
                $"{config.Name}.SpCond",
                sonde,
                () => Task.FromResult<IUnit?>(spCond),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }
        if (config.Parameters.Contains("BGARFU", StringComparer.OrdinalIgnoreCase) ||
              config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<Scalar>(
                $"{config.Name}.BGARFU",
                sonde,
                () => Task.FromResult<IUnit?>(bgarfu),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }
        if (config.Parameters.Contains("TDS", StringComparer.OrdinalIgnoreCase) ||
              config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<Scalar>(
            $"{config.Name}.TDS",
            sonde,
            () => Task.FromResult<IUnit?>(tds),
            TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }
        if (config.Parameters.Contains("Turbidity", StringComparer.OrdinalIgnoreCase) ||
              config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<Scalar>(
                $"{config.Name}.Turbidity",
                sonde,
                () => Task.FromResult<IUnit?>(turb),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }
        if (config.Parameters.Contains("TSS", StringComparer.OrdinalIgnoreCase) ||
              config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<Scalar>(
                $"{config.Name}.TSS",
                sonde,
                () => Task.FromResult<IUnit?>(tss),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }
        if (config.Parameters.Contains("WiperPos", StringComparer.OrdinalIgnoreCase) ||
              config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<Scalar>(
                $"{config.Name}.WiperPos",
                sonde,
                () => Task.FromResult<IUnit?>(wiper),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }
        if (config.Parameters.Contains("pH", StringComparer.OrdinalIgnoreCase) ||
              config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<Scalar>(
                $"{config.Name}.pH",
                sonde,
                () => Task.FromResult<IUnit?>(pH),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }
        if (config.Parameters.Contains("pHmV", StringComparer.OrdinalIgnoreCase) ||
              config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<Scalar>(
                $"{config.Name}.pHmV",
                sonde,
                () => Task.FromResult<IUnit?>(pHmV),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }
        if (config.Parameters.Contains("Battery", StringComparer.OrdinalIgnoreCase) ||
              config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<Scalar>(
                $"{config.Name}.Battery",
                sonde,
                () => Task.FromResult<IUnit?>(battery),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }
        if (config.Parameters.Contains("CablePower", StringComparer.OrdinalIgnoreCase) ||
              config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<Scalar>(
                $"{config.Name}.CablePower",
                sonde,
                () => Task.FromResult<IUnit?>(cablePower),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }

        readTimer = new Timer(
            async _ => await ManageSondeData(),
            null,
            0,
            config.SenseIntervalSeconds * 500); // read 2x the interval

        return Task.FromResult<IEnumerable<ISensingNode>>(nodes);
    }

    private Scalar? ConvertReadingToScalar((ParameterCode parameter, object reading) value)
    {
        try
        {
            return (Scalar)Convert.ToDouble(value.reading);
        }
        catch
        {
            Resolver.Log.Warn($"Unable to convert {value.parameter} reading {value.reading} to Scalar");
            return null;
        }
    }

    private async Task ReportSondeConfiguration()
    {
        if (sonde != null)
        {
            if (reportCompleted)
            {
                return;
            }

            var configuredParameters = await sonde.GetParametersToRead();

            Resolver.Log.Info($"Sonde is configured to record the following:", "ysi-sonde");

            foreach (var param in configuredParameters)
            {
                Resolver.Log.Trace($" - {param}", "ysi-sonde");
            }

            var samplePeriod = await sonde.GetSamplePeriod();
            Resolver.Log.Trace($"Sample period: {samplePeriod:HH:mm}", "ysi-sonde");

            reportCompleted = true;
        }
    }

    private async Task ManageSondeData()
    {
        if (sonde != null)
        {
            try
            {
                await ReportSondeConfiguration();

                var statusArray = await sonde.GetParameterStatus();

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
                    chlor = ConvertReadingToScalar(chlorTuple);
                }

                if (condTuple != default)
                {
                    cond = ConvertReadingToScalar(condTuple);
                }

                if (lfCondTuple != default)
                {
                    lfCond = ConvertReadingToScalar(lfCondTuple);
                }

                if (doSatTuple != default)
                {
                    doSat = ConvertReadingToScalar(doSatTuple);
                }

                if (momgLTuple != default)
                {
                    domgL = ConvertReadingToScalar(momgLTuple);
                }

                if (salTuple != default)
                {
                    salintiy = ConvertReadingToScalar(salTuple);
                }

                if (spCondTuple != default)
                {
                    ConvertReadingToScalar(spCondTuple);
                }

                if (bgarfuTuple != default)
                {
                    bgarfu = ConvertReadingToScalar(bgarfuTuple);
                }

                if (tdstuple != default)
                {
                    ConvertReadingToScalar(tdstuple);
                }

                if (turbTuple != default)
                {
                    ConvertReadingToScalar(turbTuple);
                }

                if (tssTuple != default)
                {
                    ConvertReadingToScalar(tssTuple);
                }

                if (wiperTuple != default)
                {
                    ConvertReadingToScalar(wiperTuple);
                }

                if (pHTuple != default)
                {
                    ConvertReadingToScalar(pHTuple);
                }

                if (pHmVTuple != default)
                {
                    ConvertReadingToScalar(pHmVTuple);
                }

                if (batteryTuple != default)
                {
                    ConvertReadingToScalar(batteryTuple);
                }

                if (cablePowerTuple != default)
                {
                    ConvertReadingToScalar(cablePowerTuple);
                }
            }
            catch (Exception ex)
            {
                Resolver.Log.Warn($"Unable to read sonde information: {ex.Message}");
            }
        }
    }
}
