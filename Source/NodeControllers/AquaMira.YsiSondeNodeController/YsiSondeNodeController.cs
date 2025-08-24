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

public class YsiSondeNodeController
{
    private IExoSonde? sonde;
    private Timer readTimer;
    public Task<IEnumerable<ISensingNode>> ConfigureFromJson(string configJson, IAquaMiraHardware hardware)
    {
        YsiSondConfiguration config;
        try
        {
            config = MicroJson.Deserialize<YsiSondConfiguration>(configJson);
            if (config == null)
            {
                Resolver.Log?.Error("Failed to deserialize YsiSondConfiguration configuration from JSON");
                return Task.FromResult(Enumerable.Empty<ISensingNode>());
            }
        }
        catch (Exception ex)
        {
            Resolver.Log?.Error("Failed to deserialize YsiSondConfiguration configuration from JSON");
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

        var nodes = new List<ISensingNode>
        {
            new UnitizedSensingNode<Temperature>(
                $"{config.Name}.Temperature",
                sonde,
                () => sondeTemp,
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)),
            new UnitizedSensingNode<Scalar>(
                $"{config.Name}.Chloriform",
                sonde,
                () => sondeChlor,
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)),
            new UnitizedSensingNode<Scalar>(
                $"{config.Name}.Conductivity",
                sonde,
                () => sondeCond,
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)),

            new UnitizedSensingNode<Scalar>(
                $"{config.Name}.LFChloriform",
                sonde,
                () => sondeLFCond,
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)),
            new UnitizedSensingNode<Scalar>(
                $"{config.Name}.DOSat",
                sonde,
                () => sondeDOSat,
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)),
            new UnitizedSensingNode<Scalar>(
                $"{config.Name}.DOmgL",
                sonde,
                () => sondeDOmgL,
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)),
            new UnitizedSensingNode<Scalar>(
                $"{config.Name}.Salinity",
                sonde,
                () => sondeSal,
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)),
            new UnitizedSensingNode<Scalar>(
                $"{config.Name}.SpCond",
                sonde,
                () => sondeSpCond,
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)),
            new UnitizedSensingNode<Scalar>(
                $"{config.Name}.BGARFU",
                sonde,
                () => sondeBGARFU,
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)),
            new UnitizedSensingNode<Scalar>(
                $"{config.Name}.TotalDisolvedSolids",
                sonde,
                () => sondeTDS,
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)),
            new UnitizedSensingNode<Scalar>(
                $"{config.Name}.Turbidity",
                sonde,
                () => sondeTurb,
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)),
            new UnitizedSensingNode<Scalar>(
                $"{config.Name}.TotalSuspendedSolids",
                sonde,
                () => sondeTSS,
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)),
            new UnitizedSensingNode<Scalar>(
                $"{config.Name}.WiperPos",
                sonde,
                () => sondeWiper,
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)),
            new UnitizedSensingNode<Scalar>(
                $"{config.Name}.PH",
                sonde,
                () => sondePH,
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)),
            new UnitizedSensingNode<Scalar>(
                $"{config.Name}.PHmV",
                sonde,
                () => sondePHmV,
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)),
            new UnitizedSensingNode<Scalar>(
                $"{config.Name}.Battery",
                sonde,
                () => sondeBattery,
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)),
            new UnitizedSensingNode<Scalar>(
                $"{config.Name}.CablePower",
                sonde,
                () => sondeCablePower,
                TimeSpan.FromSeconds(config.SenseIntervalSeconds)),
        };

        readTimer = new Timer(
            async _ => await ManageSondeData(),
            null,
            0,
            config.SenseIntervalSeconds * 500);

        return Task.FromResult<IEnumerable<ISensingNode>>(nodes);
    }

    private Temperature? sondeTemp = null;
    private Scalar? sondeChlor;
    private Scalar sondeCond;
    private Scalar sondeLFCond;
    private Scalar sondeDOSat;
    private Scalar sondeDOmgL;
    private Scalar sondeSal;
    private Scalar sondeSpCond;
    private Scalar sondeBGARFU;
    private Scalar sondeTDS;
    private Scalar sondeTurb;
    private Scalar sondeTSS;
    private Scalar sondeWiper;
    private Scalar sondePH;
    private Scalar sondePHmV;
    private Scalar sondeBattery;
    private Scalar sondeCablePower;

    private async Task ManageSondeData()
    {
        if (sonde != null)
        {
            try
            {
                Resolver.Log.Trace($"Reading sonde information...");
                var sondeData = await sonde.GetCurrentData();

                var sondeTempFTuple = sondeData.FirstOrDefault(d => d.ParameterCode == Meadow.Foundation.Sensors.Environmental.Ysi.ParameterCode.TemperatureC);
                var sondeChlorTuple = sondeData.FirstOrDefault(d => d.ParameterCode == Meadow.Foundation.Sensors.Environmental.Ysi.ParameterCode.ChlorophyllRFU);
                var sondeCondTuple = sondeData.FirstOrDefault(d => d.ParameterCode == Meadow.Foundation.Sensors.Environmental.Ysi.ParameterCode.ConductivityuScm);
                var sondeLFCondTuple = sondeData.FirstOrDefault(d => d.ParameterCode == Meadow.Foundation.Sensors.Environmental.Ysi.ParameterCode.nLFConductivityuScm);
                var sondeDOSatTuple = sondeData.FirstOrDefault(d => d.ParameterCode == Meadow.Foundation.Sensors.Environmental.Ysi.ParameterCode.ODOPercentSat);
                var sondeDOmgLTuple = sondeData.FirstOrDefault(d => d.ParameterCode == Meadow.Foundation.Sensors.Environmental.Ysi.ParameterCode.ODOmgL);
                var sondeSalTuple = sondeData.FirstOrDefault(d => d.ParameterCode == Meadow.Foundation.Sensors.Environmental.Ysi.ParameterCode.Salinity);
                var sondeSpCondTuple = sondeData.FirstOrDefault(d => d.ParameterCode == Meadow.Foundation.Sensors.Environmental.Ysi.ParameterCode.SpecificConductanceuScm);
                var sondeBGARFUTuple = sondeData.FirstOrDefault(d => d.ParameterCode == Meadow.Foundation.Sensors.Environmental.Ysi.ParameterCode.BGAPErfu);
                var sondeTDSTuple = sondeData.FirstOrDefault(d => d.ParameterCode == Meadow.Foundation.Sensors.Environmental.Ysi.ParameterCode.TDSmgL);
                var sondeTurbTuple = sondeData.FirstOrDefault(d => d.ParameterCode == Meadow.Foundation.Sensors.Environmental.Ysi.ParameterCode.TurbidityFNU);
                var sondeTSSTuple = sondeData.FirstOrDefault(d => d.ParameterCode == Meadow.Foundation.Sensors.Environmental.Ysi.ParameterCode.TSSmgL);
                var sondeWiperTuple = sondeData.FirstOrDefault(d => d.ParameterCode == Meadow.Foundation.Sensors.Environmental.Ysi.ParameterCode.WiperPosition);
                var sondePHTuple = sondeData.FirstOrDefault(d => d.ParameterCode == Meadow.Foundation.Sensors.Environmental.Ysi.ParameterCode.pH);
                var sondePHmVTuple = sondeData.FirstOrDefault(d => d.ParameterCode == Meadow.Foundation.Sensors.Environmental.Ysi.ParameterCode.pHmV);
                var sondeBatteryTuple = sondeData.FirstOrDefault(d => d.ParameterCode == Meadow.Foundation.Sensors.Environmental.Ysi.ParameterCode.BatteryVoltage);
                var sondeCablePowerTuple = sondeData.FirstOrDefault(d => d.ParameterCode == Meadow.Foundation.Sensors.Environmental.Ysi.ParameterCode.ExternalPower);

                if (sondeTempFTuple != default)
                {
                    sondeTemp = new Temperature(Convert.ToDouble(sondeTempFTuple.Value), Temperature.UnitType.Celsius);
                }
                if (sondeTemp == null)
                {
                    sondeTemp = new Temperature(Convert.ToDouble(-99));
                }

                if (sondeChlorTuple != default)
                {
                    sondeChlor = (Scalar)(double)sondeChlorTuple.Value;
                }

                if (sondeCondTuple != default)
                {
                    sondeCond = (Scalar)(float)sondeCondTuple.Value;
                }

                if (sondeLFCondTuple != default)
                {
                    sondeLFCond = (Scalar)(float)sondeLFCondTuple.Value;
                }

                if (sondeDOSatTuple != default)
                {
                    sondeDOSat = (Scalar)(float)sondeDOSatTuple.Value;
                }

                if (sondeDOmgLTuple != default)
                {
                    sondeDOmgL = (Scalar)(float)sondeDOmgLTuple.Value;
                }

                if (sondeSalTuple != default)
                {
                    sondeSal = (Scalar)(float)sondeSalTuple.Value;
                }

                if (sondeSpCondTuple != default)
                {
                    sondeSpCond = (Scalar)(float)sondeSpCondTuple.Value;
                }

                if (sondeBGARFUTuple != default)
                {
                    sondeBGARFU = (Scalar)(float)sondeBGARFUTuple.Value;
                }

                if (sondeTDSTuple != default)
                {
                    sondeTDS = (Scalar)(float)sondeTDSTuple.Value;
                }

                if (sondeTurbTuple != default)
                {
                    sondeTurb = (Scalar)(float)sondeTurbTuple.Value;
                }

                if (sondeTSSTuple != default)
                {
                    sondeTSS = (Scalar)(float)sondeTSSTuple.Value;
                }

                if (sondeWiperTuple != default)
                {
                    sondeWiper = (Scalar)(float)sondeWiperTuple.Value;
                }

                if (sondePHTuple != default)
                {
                    sondePH = (Scalar)(float)sondePHTuple.Value;
                }

                if (sondePHmVTuple != default)
                {
                    sondePHmV = (Scalar)(float)sondePHmVTuple.Value;
                }

                if (sondeBatteryTuple != default)
                {
                    sondeBattery = (Scalar)(float)sondeBatteryTuple.Value;
                }

                if (sondeCablePowerTuple != default)
                {
                    sondeCablePower = (Scalar)(float)sondeCablePowerTuple.Value;
                }
            }
            catch (Exception ex)
            {
                Resolver.Log.Warn($"Unable to read sonde information: {ex.Message}");
            }
        }
    }
}
