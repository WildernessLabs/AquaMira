using System;

namespace Thurston_Monitor.Core;

public static class ModbusDeviceFactory
{
    public static ICompositeSensor CreateSensor(ModbusDeviceConfig config)
    {
        switch (config.Driver.ToLower())
        {
            case "franklinelectric.xdrive":
                return new XDriveModbusDevice(config);
            default:
                throw new NotSupportedException($"Modbus device {config.Driver} not supported");
        }

        throw new NotImplementedException();
    }
}
