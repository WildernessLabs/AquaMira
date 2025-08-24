using AquaMira.Core;
using AquaMira.Core.Contracts;
using Meadow;
using Meadow.Foundation.Sensors;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Foundation.Sensors.Hid;
using Meadow.Hardware;
using Meadow.Modbus;
using Meadow.Peripherals.Displays;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Buttons;
using Meadow.Units;
using System;
using System.Collections.Generic;

namespace AquaMira.DT;

internal class AquaMiraHardware : IAquaMiraHardware
{
    private readonly Desktop device;
    private readonly Keyboard keyboard;

    public RotationType DisplayRotation => RotationType.Default;
    public INetworkController? NetworkController { get; }
    public IPixelDisplay? Display => device.Display;
    public ITemperatureSensor? TemperatureSensor { get; }
    public IButton? UpButton { get; }
    public IButton? RightButton { get; }
    public IButton? LeftButton { get; }
    public IInputController InputController { get; }

    private ModbusRtuClient? modbusClient;

    public AquaMiraHardware(Desktop device)
    {
        this.device = device;

        keyboard = new Keyboard();
        NetworkController = new NetworkController(keyboard);

        TemperatureSensor = new SimulatedTemperatureSensor(
            new Temperature(70, Temperature.UnitType.Fahrenheit),
            keyboard.Pins.Q.CreateDigitalInterruptPort(InterruptMode.EdgeRising),
            keyboard.Pins.W.CreateDigitalInterruptPort(InterruptMode.EdgeRising));

        UpButton = new PushButton(keyboard.Pins.Up);
        LeftButton = new PushButton(keyboard.Pins.Left);
        RightButton = new PushButton(keyboard.Pins.Right);
        InputController = new InputController();
    }

    public ModbusRtuClient GetModbusSerialClient()
    {
        if (modbusClient == null)
        {
            var port = new WindowsSerialPort(
                ConfigurationController.AppSettings.ModbusSerialPort,
                ConfigurationController.AppSettings.ModbusBaudRate.Value);

            modbusClient = new ModbusRtuClient(port);
        }

        return modbusClient;
    }

    public IEnumerable<(Type ControllerType, string ConfigurationName)> GetAvailableSensingNodeControllers()
    {
        // Return the sensing node controllers available on the Desktop platform
        return new[]
        {
            (typeof(T322InputNodeController), "T322iInputs"),
            (typeof(SPM1xPowerNodeController), "Spm1x"),
        };
    }
}