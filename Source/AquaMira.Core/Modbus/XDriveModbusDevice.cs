using AquaMira.Core.Contracts;
using Meadow.Foundation.VFDs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AquaMira.Core;

public class XDriveModbusDevice : ICompositeSensor
{
    private readonly IXDrive drive;
    private readonly ModbusDeviceConfig config;

    public XDriveModbusDevice(ModbusDeviceConfig config, IAquaMiraHardware hardware)
    {
        this.config = config;

        if (config.IsSimulated)
        {
            drive = new SimulatedXDrive();
        }
        else
        {
            var modbusClient = hardware.GetModbusSerialClient();
            drive = new CerusXDrive(modbusClient, (byte)config.Address);
        }


        drive.Connect();
    }

    public Dictionary<string, object> GetCurrentValues()
    {
        var values = new Dictionary<string, object>();

        // the caller needs this to be synchronous
        Task.Run(async () =>
        {
            // TODO: need to add exception handling here!
            var outputCurrent = await drive.ReadOutputCurrent();
            values.Add($"{config.Name}.OutputCurrent", outputCurrent);
            var ambientTemp = await drive.ReadAmbientTemperature();
            values.Add($"{config.Name}.AmbientTemp", ambientTemp);
            var dcVoltage = await drive.ReadDCBusVoltage();
            values.Add($"{config.Name}.DCBusVoltage", dcVoltage);
            var outputFrequency = await drive.ReadOutputFrequency();
            values.Add($"{config.Name}.OutputFrequency", outputFrequency);
            var outputVoltage = await drive.ReadOutputVoltage();
            values.Add($"{config.Name}.OutputVoltage", outputVoltage);
        }).GetAwaiter().GetResult();

        return values;
    }
}
