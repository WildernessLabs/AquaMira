using Meadow.Modbus;
using Meadow.Peripherals.Displays;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Buttons;

namespace AquaMira.Core.Contracts;

public interface IAquaMiraHardware
{
    // basic hardware
    IButton? UpButton { get; }
    IButton? LeftButton { get; }
    IButton? RightButton { get; }

    // complex hardware
    ITemperatureSensor? TemperatureSensor { get; }
    IPixelDisplay? Display { get; }
    RotationType DisplayRotation { get; }

    // platform-dependent services
    IInputController InputController { get; }
    INetworkController NetworkController { get; }

    ModbusRtuClient GetModbusSerialClient();
}