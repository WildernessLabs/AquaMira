using Meadow.Foundation.Sensors.Power;
using Meadow.Units;
using System.Collections.Generic;
using Thurston_Monitor.Core.Contracts;

namespace Thurston_Monitor.Core;

public class Spm1xModbusDevice : ICompositeSensor
{
    private readonly ISpm1x sensor;
    private readonly ModbusDeviceConfig config;

    public Spm1xModbusDevice(ModbusDeviceConfig config, IThurston_MonitorHardware hardware)
    {
        this.config = config;

        if (config.IsSimulated)
        {
            var s = new SimulatedSpm1x(99);
            s.SetVoltage(120.Volts());
            s.SetCurrent(0.5.Amps());
            sensor = s;
        }
        else
        {
            var modbusClient = hardware.GetModbusSerialClient();
            sensor = new Spm1x(modbusClient, (byte)config.Address);
        }
    }

    public Dictionary<string, object> GetCurrentValues()
    {
        var values = new Dictionary<string, object>();

        var inputCurrent = sensor.ReadCurrent().GetAwaiter().GetResult();
        values.Add($"{config.Name}.Current", inputCurrent);
        var inputVoltage = sensor.ReadVoltage().GetAwaiter().GetResult();
        values.Add($"{config.Name}.Voltage", inputVoltage);

        return values;
    }
}
