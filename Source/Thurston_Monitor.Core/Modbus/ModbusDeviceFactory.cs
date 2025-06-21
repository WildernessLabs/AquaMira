using System;
using Thurston_Monitor.Core.Contracts;

namespace Thurston_Monitor.Core;

public static class ModbusDeviceFactory
{
    public static ICompositeSensor CreateSensor(ModbusDeviceConfig config, IThurston_MonitorHardware hardware)
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
