using Meadow.Common;
using Meadow.Units;
using System;

namespace Thurston_Monitor.Core;

public static class InputToUnitConverter
{
    public static TUnit ConvertCurrentToUnit<TUnit>(Current current, double scale, double offset)
        where TUnit : struct, IUnit
    {
        var rawUnit = UnitFactory.CreateUnitFromCanonicalValue(current.Milliamps, typeof(TUnit).Name);

        return rawUnit switch
        {
            Temperature temperature => (TUnit)(object)new Temperature(
                temperature.Celsius * scale + offset,
                Temperature.UnitType.Celsius),
            _ => throw new NotSupportedException($"Conversion to {typeof(TUnit)} not supported")
        };
    }

    public static object ConvertCurrentToUnit(Current current, string unitType, double scale, double offset)
    {
        return unitType.ToLower() switch
        {
            "temperature" => ConvertCurrentToUnit<Temperature>(current, scale, offset),
            "pressure" => ConvertCurrentToUnit<Pressure>(current, scale, offset),
            _ => throw new NotSupportedException($"Conversion to {unitType} not supported")
        };
    }

    public static object ConvertCurrentToUnit(Current current, Type unitType, double scale, double offset)
    {
        var rawUnit = UnitFactory.CreateUnitFromCanonicalValue(current.Milliamps, unitType.Name);

        return rawUnit switch
        {
            Temperature temperature => new Temperature(
                temperature.Celsius * scale + offset,
                Temperature.UnitType.Celsius),
            _ => throw new NotSupportedException($"Conversion to {unitType.Name} not supported")
        };
    }

    //switch (channelType)
    //{
    //    case ConfigurableAnalogInputChannelType.Current_4_20:
    //        current = Read4_20mA(channelNumber);
    //        if (current.Milliamps < 3.9)
    //        {
    //            throw new CurrentOutOfRangeException("Undercurrent condition.  Check the sensor and wiring");
    //        }
    //        break;
    //    case ConfigurableAnalogInputChannelType.Current_0_20:
    //        current = Read0_20mA(channelNumber);
    //        break;
    //    default:
    //        throw new ArgumentException();
    //}

    //if (current.Milliamps > 20.1)
    //{
    //    throw new CurrentOutOfRangeException("Overcurrent condition.  Check the sensor and wiring");
    //}

    //var rawUnit = UnitFactory.CreateUnitFromCanonicalValue(current.Milliamps, channelConfigs[channelNumber].UnitType);

    //if (rawUnit is Temperature temperature)
    //{
    //    var c = temperature.Celsius * channelConfigs[channelNumber].Scale;
    //    c += channelConfigs[channelNumber].Offset;
    //    return new Temperature(c, Temperature.UnitType.Celsius);
    //}
    //if (rawUnit is Pressure pressure)
    //{
    //    var p = pressure.Bar * channelConfigs[channelNumber].Scale;
    //    p += channelConfigs[channelNumber].Offset;
    //    return new Pressure(p);
    //}

    //throw new NotSupportedException();
}

