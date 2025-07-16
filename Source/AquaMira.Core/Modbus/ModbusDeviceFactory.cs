using AquaMira.Core.Contracts;
using System;

namespace AquaMira.Core;

public static class ModbusDeviceFactory
{
    public static ICompositeSensor CreateSensor(ModbusDeviceConfig config, IAquaMiraHardware hardware)
    {
        switch (config.Driver.ToLower())
        {
            case "cerusxdrive":
                return new XDriveModbusDevice(config, hardware);
            case "spm1x":
                return new Spm1xModbusDevice(config, hardware);
            default:
                throw new NotSupportedException($"Modbus device {config.Driver} not supported");
        }

        throw new NotImplementedException();
    }
}
