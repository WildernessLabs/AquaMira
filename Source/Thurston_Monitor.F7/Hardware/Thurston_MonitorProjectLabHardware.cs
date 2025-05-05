using Meadow.Devices;
using Meadow.Modbus;
using Meadow.Peripherals.Displays;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Buttons;
using Thurston_Monitor.Core;
using Thurston_Monitor.Core.Contracts;

namespace Thurston_Monitor.F7
{
    internal class Thurston_MonitorProjectLabHardware : IThurston_MonitorHardware
    {
        private readonly IProjectLabHardware projLab;

        public RotationType DisplayRotation => RotationType._270Degrees;
        public IButton? LeftButton => projLab.LeftButton;
        public IButton? RightButton => projLab.RightButton;
        public ITemperatureSensor? TemperatureSensor => projLab.TemperatureSensor;
        public IPixelDisplay? Display => projLab.Display;
        public INetworkController NetworkController { get; }

        public IInputController InputController { get; }

        public Thurston_MonitorProjectLabHardware(F7CoreComputeV2 device)
        {
            projLab = ProjectLab.Create();

            InputController = new InputController(projLab);

            NetworkController = new NetworkController(device);
        }

        public ModbusRtuClient GetModbusSerialClient()
        {
            return projLab.GetModbusRtuClient(ConfigurationController.AppSettings.ModbusBaudRate ?? 19200);
        }
    }
}