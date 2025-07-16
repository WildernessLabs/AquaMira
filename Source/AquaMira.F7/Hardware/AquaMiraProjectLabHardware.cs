using AquaMira.Core;
using AquaMira.Core.Contracts;
using Meadow;
using Meadow.Devices;
using Meadow.Modbus;
using Meadow.Peripherals.Displays;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Buttons;

namespace AquaMira.F7;

internal class AquaMiraProjectLabHardware : IAquaMiraHardware
{
    private readonly IProjectLabHardware projLab;

    public RotationType DisplayRotation => RotationType._270Degrees;
    public IButton? LeftButton => projLab.LeftButton;
    public IButton? RightButton => projLab.RightButton;
    public IButton? UpButton => projLab.UpButton;
    public ITemperatureSensor? TemperatureSensor => projLab.TemperatureSensor;
    public IPixelDisplay? Display => projLab.Display;
    public INetworkController NetworkController { get; }

    public IInputController InputController { get; }

    public AquaMiraProjectLabHardware(F7CoreComputeV2 device)
    {
        Resolver.Log.Info("Creating ProjectLab...");

        projLab = ProjectLab.Create();

        InputController = new InputController(projLab);

        NetworkController = new NetworkController(device);
    }

    public ModbusRtuClient GetModbusSerialClient()
    {
        var baud = ConfigurationController.AppSettings.ModbusBaudRate ?? 9600;
        Resolver.Log.Info($"Modbus RTU is running at {baud} baud");

        return projLab.GetModbusRtuClient(baud);
    }
}