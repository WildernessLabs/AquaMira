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

    private Temperature sondeTemp = new Temperature(-99);
    private Conductivity conductivity = Conductivity.Zero;
    private ConcentrationInWater salinity = ConcentrationInWater.Zero;
    private PotentialHydrogen pH = PotentialHydrogen.Neutral;
    private Voltage batteryVoltage = Voltage.Zero;
    private Voltage cablePower = Voltage.Zero;
    private Scalar doSat = Scalar.Zero;
    private ConcentrationInWater domgL = ConcentrationInWater.Zero;

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
        if (config.Parameters.Contains("Conductivity", StringComparer.OrdinalIgnoreCase) ||
           config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<Conductivity>(
                $"{config.Name}.Conductivity",
                sonde,
                () => Task.FromResult<IUnit?>(conductivity),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }
        if (config.Parameters.Contains("Salinity", StringComparer.OrdinalIgnoreCase) ||
           config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<ConcentrationInWater>(
                $"{config.Name}.Salinity",
                sonde,
                () => Task.FromResult<IUnit?>(salinity),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }
        if (config.Parameters.Contains("pH", StringComparer.OrdinalIgnoreCase) ||
              config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<PotentialHydrogen>(
                $"{config.Name}.pH",
                sonde,
                () => Task.FromResult<IUnit?>(pH),
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
            nodes.Add(new UnitizedSensingNode<ConcentrationInWater>(
                $"{config.Name}.DOmgL",
                sonde,
                () => Task.FromResult<IUnit?>(domgL),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }





        if (config.Parameters.Contains("Chloriform", StringComparer.OrdinalIgnoreCase) ||
           config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
        }
        if (config.Parameters.Contains("LFChloriform", StringComparer.OrdinalIgnoreCase) ||
           config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
        }
        if (config.Parameters.Contains("SpCond", StringComparer.OrdinalIgnoreCase) ||
              config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
        }
        if (config.Parameters.Contains("BGARFU", StringComparer.OrdinalIgnoreCase) ||
              config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
        }
        if (config.Parameters.Contains("TDS", StringComparer.OrdinalIgnoreCase) ||
              config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
        }
        if (config.Parameters.Contains("Turbidity", StringComparer.OrdinalIgnoreCase) ||
              config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
        }
        if (config.Parameters.Contains("TSS", StringComparer.OrdinalIgnoreCase) ||
              config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
        }
        if (config.Parameters.Contains("WiperPos", StringComparer.OrdinalIgnoreCase) ||
              config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
        }
        if (config.Parameters.Contains("pHmV", StringComparer.OrdinalIgnoreCase) ||
              config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
        }
        if (config.Parameters.Contains("Battery", StringComparer.OrdinalIgnoreCase) ||
              config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
        }
        if (config.Parameters.Contains("CablePower", StringComparer.OrdinalIgnoreCase) ||
              config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
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
        if (config.Parameters.Contains("LFChloriform", StringComparer.OrdinalIgnoreCase) ||
           config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<Scalar>(
                $"{config.Name}.LFChloriform",
                sonde,
                () => Task.FromResult<IUnit?>(lfCond),
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

    private bool TryExtractValue<TUnit>((ParameterCode ParameterCode, IUnit Value)[]? readings, ParameterCode desiredCode, ref TUnit field)
        where TUnit : IUnit
    {
        if (readings != null)
        {
            var reading = readings.FirstOrDefault(d => d.ParameterCode == ParameterCode.TemperatureC);
            if (reading.ParameterCode == desiredCode && reading.Value is TUnit t)
            {
                field = t;
                return true;
            }
        }
        return false;
    }

    private async Task ManageSondeData()
    {
        if (sonde != null)
        {
            try
            {
                await ReportSondeConfiguration();

                // var statusArray = await sonde.GetParameterStatus();

                Resolver.Log.Trace($"Reading sonde information...");
                var data = await sonde.GetCurrentData();

                TryExtractValue(data, ParameterCode.TemperatureC, ref sondeTemp);
                TryExtractValue(data, ParameterCode.TemperatureF, ref sondeTemp);
                TryExtractValue(data, ParameterCode.TemperatureK, ref sondeTemp);
                TryExtractValue(data, ParameterCode.ConductivityuScm, ref conductivity);
                TryExtractValue(data, ParameterCode.Salinity, ref salinity);

                TryExtractValue(data, ParameterCode.pH, ref pH);
                TryExtractValue(data, ParameterCode.BatteryVoltage, ref batteryVoltage);
                TryExtractValue(data, ParameterCode.ExternalPower, ref cablePower);

                TryExtractValue(data, ParameterCode.ODOPercentSat, ref doSat);
                TryExtractValue(data, ParameterCode.ODOmgL, ref doSat);

                // TODO: TryExtractValue(data, ParameterCode.ChlorophyllRFU, 



                var chlorTuple = data.FirstOrDefault(d => d.ParameterCode == ParameterCode.ChlorophyllRFU);
                var lfCondTuple = data.FirstOrDefault(d => d.ParameterCode == ParameterCode.nLFConductivityuScm);
                var momgLTuple = data.FirstOrDefault(d => d.ParameterCode == ParameterCode.ODOmgL);
                var spCondTuple = data.FirstOrDefault(d => d.ParameterCode == ParameterCode.SpecificConductanceuScm);
                var bgarfuTuple = data.FirstOrDefault(d => d.ParameterCode == ParameterCode.BGAPErfu);
                var tdstuple = data.FirstOrDefault(d => d.ParameterCode == ParameterCode.TDSmgL);
                var turbTuple = data.FirstOrDefault(d => d.ParameterCode == ParameterCode.TurbidityFNU);
                var tssTuple = data.FirstOrDefault(d => d.ParameterCode == ParameterCode.TSSmgL);
                var wiperTuple = data.FirstOrDefault(d => d.ParameterCode == ParameterCode.WiperPosition);
                var pHmVTuple = data.FirstOrDefault(d => d.ParameterCode == ParameterCode.pHmV);
                var cablePowerTuple = data.FirstOrDefault(d => d.ParameterCode == ParameterCode.ExternalPower);


                if (chlorTuple != default)
                {
                    chlor = ConvertReadingToScalar(chlorTuple);
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
