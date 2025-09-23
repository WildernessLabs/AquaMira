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
    private Conductivity specificConductivity = Conductivity.Zero;
    private Conductivity nlfConductivity = Conductivity.Zero;
    private ConcentrationInWater salinity = ConcentrationInWater.Zero;
    private PotentialHydrogen pH = PotentialHydrogen.Neutral;
    private Voltage pHmV = Voltage.Zero;
    private Voltage orp = Voltage.Zero;
    private Voltage batteryVoltage = Voltage.Zero;
    private Voltage cablePower = Voltage.Zero;
    private Voltage wiperPosition = Voltage.Zero;
    private Current wiperPeakCurrent = Current.Zero;
    private Scalar doSat = Scalar.Zero;
    private ConcentrationInWater domgL = ConcentrationInWater.Zero;
    private Scalar doLocal = Scalar.Zero;

    // TDS parameters
    private ConcentrationInWater tdsMgL = ConcentrationInWater.Zero;
    private ConcentrationInWater tdsGl = ConcentrationInWater.Zero;
    private ConcentrationInWater tdsKgL = ConcentrationInWater.Zero;

    // TSS parameters
    private ConcentrationInWater tssMgL = ConcentrationInWater.Zero;
    private ConcentrationInWater tssGl = ConcentrationInWater.Zero;

    // Pressure and depth
    private Pressure pressurePsia = new Pressure(0);
    private Pressure pressurePsig = new Pressure(0);
    private Length depthMeters = Length.Zero;
    private Length depthFeet = Length.Zero;
    private Length verticalPositionM = Length.Zero;
    private Length verticalPositionFt = Length.Zero;

    // Turbidity
    private Turbidity turbidityNtu = new Turbidity(0);
    private Scalar turbidityFnu = Scalar.Zero;
    private Scalar turbidityRaw = Scalar.Zero;

    // Nutrients
    private ConcentrationInWater nh3 = ConcentrationInWater.Zero;
    private ConcentrationInWater nh4 = ConcentrationInWater.Zero;
    private Voltage nh4Mv = Voltage.Zero;
    private ConcentrationInWater no3 = ConcentrationInWater.Zero;
    private Voltage no3Mv = Voltage.Zero;
    private ConcentrationInWater chloride = ConcentrationInWater.Zero;
    private Voltage chlorideMv = Voltage.Zero;
    private ConcentrationInWater potassium = ConcentrationInWater.Zero;
    private Voltage potassiumMv = Voltage.Zero;

    // Chlorophyll and algae
    private ConcentrationInWater chlorophyllUgL = ConcentrationInWater.Zero;
    private Scalar chlorophyllRfu = Scalar.Zero;
    private Scalar chlorophyllCellsMl = Scalar.Zero;
    private Scalar chlorophyllRaw = Scalar.Zero;
    private ConcentrationInWater bgaPcUgL = ConcentrationInWater.Zero;
    private Scalar bgaPcRfu = Scalar.Zero;
    private Scalar bgaPcRaw = Scalar.Zero;
    private ConcentrationInWater bgaPeUgL = ConcentrationInWater.Zero;
    private Scalar bgaPeRfu = Scalar.Zero;
    private Scalar bgaPeRaw = Scalar.Zero;
    private Scalar talPcCellsMl = Scalar.Zero;
    private Scalar talPeCellsMl = Scalar.Zero;

    // Fluorescence and organic matter
    private Scalar fdomRfu = Scalar.Zero;
    private Scalar fdomQsu = Scalar.Zero;
    private Scalar fdomRaw = Scalar.Zero;
    private ConcentrationInWater rhodamine = ConcentrationInWater.Zero;

    // PAR (Photosynthetically Active Radiation)
    private Scalar parChannel1 = Scalar.Zero;
    private Scalar parChannel2 = Scalar.Zero;

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

        // TDS Parameters
        if (config.Parameters.Contains("TDSmgL", StringComparer.OrdinalIgnoreCase) ||
           config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<ConcentrationInWater>(
                $"{config.Name}.TDSmgL",
                sonde,
                () => Task.FromResult<IUnit?>(tdsMgL),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }
        if (config.Parameters.Contains("TDSgL", StringComparer.OrdinalIgnoreCase) ||
           config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<ConcentrationInWater>(
                $"{config.Name}.TDSgL",
                sonde,
                () => Task.FromResult<IUnit?>(tdsGl),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }
        if (config.Parameters.Contains("TDSkgL", StringComparer.OrdinalIgnoreCase) ||
           config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<ConcentrationInWater>(
                $"{config.Name}.TDSkgL",
                sonde,
                () => Task.FromResult<IUnit?>(tdsKgL),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }

        // TSS Parameters
        if (config.Parameters.Contains("TSSmgL", StringComparer.OrdinalIgnoreCase) ||
           config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<ConcentrationInWater>(
                $"{config.Name}.TSSmgL",
                sonde,
                () => Task.FromResult<IUnit?>(tssMgL),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }
        if (config.Parameters.Contains("TSSgL", StringComparer.OrdinalIgnoreCase) ||
           config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<ConcentrationInWater>(
                $"{config.Name}.TSSgL",
                sonde,
                () => Task.FromResult<IUnit?>(tssGl),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }

        // Conductivity variants
        if (config.Parameters.Contains("SpecificConductivity", StringComparer.OrdinalIgnoreCase) ||
           config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<Conductivity>(
                $"{config.Name}.SpecificConductivity",
                sonde,
                () => Task.FromResult<IUnit?>(specificConductivity),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }
        if (config.Parameters.Contains("nLFConductivity", StringComparer.OrdinalIgnoreCase) ||
           config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<Conductivity>(
                $"{config.Name}.nLFConductivity",
                sonde,
                () => Task.FromResult<IUnit?>(nlfConductivity),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }

        // pH millivolts and ORP
        if (config.Parameters.Contains("pHmV", StringComparer.OrdinalIgnoreCase) ||
           config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<Voltage>(
                $"{config.Name}.pHmV",
                sonde,
                () => Task.FromResult<IUnit?>(pHmV),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }
        if (config.Parameters.Contains("ORP", StringComparer.OrdinalIgnoreCase) ||
           config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<Voltage>(
                $"{config.Name}.ORP",
                sonde,
                () => Task.FromResult<IUnit?>(orp),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }

        // Pressure and depth
        if (config.Parameters.Contains("PressurePsia", StringComparer.OrdinalIgnoreCase) ||
           config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<Pressure>(
                $"{config.Name}.PressurePsia",
                sonde,
                () => Task.FromResult<IUnit?>(pressurePsia),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }
        if (config.Parameters.Contains("PressurePsig", StringComparer.OrdinalIgnoreCase) ||
           config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<Pressure>(
                $"{config.Name}.PressurePsig",
                sonde,
                () => Task.FromResult<IUnit?>(pressurePsig),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }
        if (config.Parameters.Contains("DepthMeters", StringComparer.OrdinalIgnoreCase) ||
           config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<Length>(
                $"{config.Name}.DepthMeters",
                sonde,
                () => Task.FromResult<IUnit?>(depthMeters),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }
        if (config.Parameters.Contains("DepthFeet", StringComparer.OrdinalIgnoreCase) ||
           config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<Length>(
                $"{config.Name}.DepthFeet",
                sonde,
                () => Task.FromResult<IUnit?>(depthFeet),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }

        // Turbidity
        if (config.Parameters.Contains("TurbidityNTU", StringComparer.OrdinalIgnoreCase) ||
           config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<Turbidity>(
                $"{config.Name}.TurbidityNTU",
                sonde,
                () => Task.FromResult<IUnit?>(turbidityNtu),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }
        if (config.Parameters.Contains("TurbidityFNU", StringComparer.OrdinalIgnoreCase) ||
           config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<Scalar>(
                $"{config.Name}.TurbidityFNU",
                sonde,
                () => Task.FromResult<IUnit?>(turbidityFnu),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }

        // Nutrients
        if (config.Parameters.Contains("NH3", StringComparer.OrdinalIgnoreCase) ||
           config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<ConcentrationInWater>(
                $"{config.Name}.NH3",
                sonde,
                () => Task.FromResult<IUnit?>(nh3),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }
        if (config.Parameters.Contains("NH4", StringComparer.OrdinalIgnoreCase) ||
           config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<ConcentrationInWater>(
                $"{config.Name}.NH4",
                sonde,
                () => Task.FromResult<IUnit?>(nh4),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }
        if (config.Parameters.Contains("NO3", StringComparer.OrdinalIgnoreCase) ||
           config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<ConcentrationInWater>(
                $"{config.Name}.NO3",
                sonde,
                () => Task.FromResult<IUnit?>(no3),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }
        if (config.Parameters.Contains("Chloride", StringComparer.OrdinalIgnoreCase) ||
           config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<ConcentrationInWater>(
                $"{config.Name}.Chloride",
                sonde,
                () => Task.FromResult<IUnit?>(chloride),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }
        if (config.Parameters.Contains("Potassium", StringComparer.OrdinalIgnoreCase) ||
           config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<ConcentrationInWater>(
                $"{config.Name}.Potassium",
                sonde,
                () => Task.FromResult<IUnit?>(potassium),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }

        // Chlorophyll
        if (config.Parameters.Contains("ChlorophyllugL", StringComparer.OrdinalIgnoreCase) ||
           config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<ConcentrationInWater>(
                $"{config.Name}.ChlorophyllugL",
                sonde,
                () => Task.FromResult<IUnit?>(chlorophyllUgL),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }
        if (config.Parameters.Contains("ChlorophyllRFU", StringComparer.OrdinalIgnoreCase) ||
           config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<Scalar>(
                $"{config.Name}.ChlorophyllRFU",
                sonde,
                () => Task.FromResult<IUnit?>(chlorophyllRfu),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }

        // Blue-green algae
        if (config.Parameters.Contains("BGAPCugL", StringComparer.OrdinalIgnoreCase) ||
           config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<ConcentrationInWater>(
                $"{config.Name}.BGAPCugL",
                sonde,
                () => Task.FromResult<IUnit?>(bgaPcUgL),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }
        if (config.Parameters.Contains("BGAPEugL", StringComparer.OrdinalIgnoreCase) ||
           config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<ConcentrationInWater>(
                $"{config.Name}.BGAPEugL",
                sonde,
                () => Task.FromResult<IUnit?>(bgaPeUgL),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }

        // Additional dissolved oxygen
        if (config.Parameters.Contains("ODOPercentLocal", StringComparer.OrdinalIgnoreCase) ||
           config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<Scalar>(
                $"{config.Name}.ODOPercentLocal",
                sonde,
                () => Task.FromResult<IUnit?>(doLocal),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }

        // Fluorescence and organic matter
        if (config.Parameters.Contains("fDOMrfu", StringComparer.OrdinalIgnoreCase) ||
           config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<Scalar>(
                $"{config.Name}.fDOMrfu",
                sonde,
                () => Task.FromResult<IUnit?>(fdomRfu),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }
        if (config.Parameters.Contains("fDOMqsu", StringComparer.OrdinalIgnoreCase) ||
           config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<Scalar>(
                $"{config.Name}.fDOMqsu",
                sonde,
                () => Task.FromResult<IUnit?>(fdomQsu),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }
        if (config.Parameters.Contains("RhodamineugL", StringComparer.OrdinalIgnoreCase) ||
           config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<ConcentrationInWater>(
                $"{config.Name}.RhodamineugL",
                sonde,
                () => Task.FromResult<IUnit?>(rhodamine),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }

        // PAR (Photosynthetically Active Radiation)
        if (config.Parameters.Contains("PARChannel1", StringComparer.OrdinalIgnoreCase) ||
           config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<Scalar>(
                $"{config.Name}.PARChannel1",
                sonde,
                () => Task.FromResult<IUnit?>(parChannel1),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }
        if (config.Parameters.Contains("PARChannel2", StringComparer.OrdinalIgnoreCase) ||
           config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<Scalar>(
                $"{config.Name}.PARChannel2",
                sonde,
                () => Task.FromResult<IUnit?>(parChannel2),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }

        // Additional system monitoring
        if (config.Parameters.Contains("WiperPosition", StringComparer.OrdinalIgnoreCase) ||
           config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<Voltage>(
                $"{config.Name}.WiperPosition",
                sonde,
                () => Task.FromResult<IUnit?>(wiperPosition),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }
        if (config.Parameters.Contains("WiperPeakCurrent", StringComparer.OrdinalIgnoreCase) ||
           config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<Current>(
                $"{config.Name}.WiperPeakCurrent",
                sonde,
                () => Task.FromResult<IUnit?>(wiperPeakCurrent),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }

        // Vertical position
        if (config.Parameters.Contains("VerticalPositionm", StringComparer.OrdinalIgnoreCase) ||
           config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<Length>(
                $"{config.Name}.VerticalPositionm",
                sonde,
                () => Task.FromResult<IUnit?>(verticalPositionM),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }
        if (config.Parameters.Contains("VerticalPositionft", StringComparer.OrdinalIgnoreCase) ||
           config.Parameters.Contains("All", StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(new UnitizedSensingNode<Length>(
                $"{config.Name}.VerticalPositionft",
                sonde,
                () => Task.FromResult<IUnit?>(verticalPositionFt),
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)));
        }

        readTimer = new Timer(
            async _ => await ManageSondeData(),
            null,
            0,
            config.SenseIntervalSeconds * 500); // read 2x the interval

        return Task.FromResult<IEnumerable<ISensingNode>>(nodes);
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
            Resolver.Log.Trace($"Sample period: {samplePeriod}", "ysi-sonde");

            reportCompleted = true;
        }
    }

    private bool TryExtractValue<TUnit>((ParameterCode ParameterCode, IUnit Value)[]? readings, ParameterCode desiredCode, ref TUnit field)
        where TUnit : IUnit
    {
        if (readings != null)
        {
            var reading = readings.FirstOrDefault(d => d.ParameterCode == desiredCode);
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

                // Temperature
                TryExtractValue(data, ParameterCode.TemperatureC, ref sondeTemp);
                TryExtractValue(data, ParameterCode.TemperatureF, ref sondeTemp);
                TryExtractValue(data, ParameterCode.TemperatureK, ref sondeTemp);

                // Conductivity
                TryExtractValue(data, ParameterCode.ConductivityuScm, ref conductivity);
                TryExtractValue(data, ParameterCode.ConductivitymScm, ref conductivity);
                TryExtractValue(data, ParameterCode.SpecificConductanceuScm, ref specificConductivity);
                TryExtractValue(data, ParameterCode.SpecificConductancemScm, ref specificConductivity);
                TryExtractValue(data, ParameterCode.nLFConductivityuScm, ref nlfConductivity);
                TryExtractValue(data, ParameterCode.nLFConductivitymScm, ref nlfConductivity);

                // Basic water quality
                TryExtractValue(data, ParameterCode.Salinity, ref salinity);
                TryExtractValue(data, ParameterCode.pH, ref pH);
                TryExtractValue(data, ParameterCode.pHmV, ref pHmV);
                TryExtractValue(data, ParameterCode.ORP, ref orp);

                // Power and system
                TryExtractValue(data, ParameterCode.BatteryVoltage, ref batteryVoltage);
                TryExtractValue(data, ParameterCode.ExternalPower, ref cablePower);
                TryExtractValue(data, ParameterCode.WiperPosition, ref wiperPosition);
                TryExtractValue(data, ParameterCode.WiperPeakCurrent, ref wiperPeakCurrent);

                // Dissolved oxygen
                TryExtractValue(data, ParameterCode.ODOPercentSat, ref doSat);
                TryExtractValue(data, ParameterCode.ODOmgL, ref domgL);
                TryExtractValue(data, ParameterCode.ODOPercentLocal, ref doLocal);

                // TDS
                TryExtractValue(data, ParameterCode.TDSmgL, ref tdsMgL);
                TryExtractValue(data, ParameterCode.TDSgL, ref tdsGl);
                TryExtractValue(data, ParameterCode.TDSkgL, ref tdsKgL);

                // TSS
                TryExtractValue(data, ParameterCode.TSSmgL, ref tssMgL);
                TryExtractValue(data, ParameterCode.TSSgL, ref tssGl);

                // Pressure and depth
                TryExtractValue(data, ParameterCode.PressurePsia, ref pressurePsia);
                TryExtractValue(data, ParameterCode.PressurePsig, ref pressurePsig);
                TryExtractValue(data, ParameterCode.DepthMeters, ref depthMeters);
                TryExtractValue(data, ParameterCode.DepthFeet, ref depthFeet);
                TryExtractValue(data, ParameterCode.VerticalPositionm, ref verticalPositionM);
                TryExtractValue(data, ParameterCode.VerticalPositionft, ref verticalPositionFt);

                // Turbidity
                TryExtractValue(data, ParameterCode.TurbidityNTU, ref turbidityNtu);
                TryExtractValue(data, ParameterCode.TurbidityFNU, ref turbidityFnu);
                TryExtractValue(data, ParameterCode.TurbidityRaw, ref turbidityRaw);

                // Nutrients
                TryExtractValue(data, ParameterCode.NH3, ref nh3);
                TryExtractValue(data, ParameterCode.NH4, ref nh4);
                TryExtractValue(data, ParameterCode.NH4mV, ref nh4Mv);
                TryExtractValue(data, ParameterCode.NO3, ref no3);
                TryExtractValue(data, ParameterCode.NO3mV, ref no3Mv);
                TryExtractValue(data, ParameterCode.Chloride, ref chloride);
                TryExtractValue(data, ParameterCode.ChlorideMV, ref chlorideMv);
                TryExtractValue(data, ParameterCode.PotassiummgL, ref potassium);
                TryExtractValue(data, ParameterCode.PotassiummV, ref potassiumMv);

                // Chlorophyll
                TryExtractValue(data, ParameterCode.ChlorophyllugL, ref chlorophyllUgL);
                TryExtractValue(data, ParameterCode.ChlorophyllRFU, ref chlorophyllRfu);
                TryExtractValue(data, ParameterCode.ChlorophyllcellsmL, ref chlorophyllCellsMl);
                TryExtractValue(data, ParameterCode.ChlorophyllRaw, ref chlorophyllRaw);

                // Blue-green algae
                TryExtractValue(data, ParameterCode.BGAPCugL, ref bgaPcUgL);
                TryExtractValue(data, ParameterCode.BGAPCrfu, ref bgaPcRfu);
                TryExtractValue(data, ParameterCode.BGAPCRaw, ref bgaPcRaw);
                TryExtractValue(data, ParameterCode.BGAPEugL, ref bgaPeUgL);
                TryExtractValue(data, ParameterCode.BGAPErfu, ref bgaPeRfu);
                TryExtractValue(data, ParameterCode.BGAPERaw, ref bgaPeRaw);
                TryExtractValue(data, ParameterCode.TALPCcellsmL, ref talPcCellsMl);
                TryExtractValue(data, ParameterCode.TALPEcellsmL, ref talPeCellsMl);

                // Fluorescence and organic matter
                TryExtractValue(data, ParameterCode.fDOMrfu, ref fdomRfu);
                TryExtractValue(data, ParameterCode.fDOMqsu, ref fdomQsu);
                TryExtractValue(data, ParameterCode.fDOMRaw, ref fdomRaw);
                TryExtractValue(data, ParameterCode.RhodamineugL, ref rhodamine);

                // PAR
                TryExtractValue(data, ParameterCode.PARChannel1, ref parChannel1);
                TryExtractValue(data, ParameterCode.PARChannel2, ref parChannel2);

            }
            catch (Exception ex)
            {
                Resolver.Log.Warn($"Unable to read sonde information: {ex.Message}");
            }
        }
    }
}
