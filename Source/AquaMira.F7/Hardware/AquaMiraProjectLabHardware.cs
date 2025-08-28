using AquaMira.Core;
using AquaMira.Core.Contracts;
using Meadow;
using Meadow.Devices;
using Meadow.Modbus;
using Meadow.Peripherals.Displays;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Buttons;
using System;
using System.Collections.Generic;

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

    public AquaMiraProjectLabHardware(IMeadowDevice device)
        : this(ProjectLab.Create())
    {
    }

    public AquaMiraProjectLabHardware(IProjectLabHardware projLab)
    {
        this.projLab = projLab;

        InputController = new InputController(projLab);

        NetworkController = new NetworkController(projLab.ComputeModule);
    }

    public ModbusRtuClient GetModbusSerialClient()
    {
        var baud = ConfigurationController.AppSettings.ModbusBaudRate ?? 9600;
        Resolver.Log.Info($"Modbus RTU is running at {baud} baud");

        return projLab.GetModbusRtuClient(baud);
    }

    public IEnumerable<(Type ControllerType, string ConfigurationName)> GetAvailableSensingNodeControllers()
    {
        // Return the sensing node controllers available on this hardware platform
        return new[]
        {
            (typeof(T322InputNodeController), "T322iInputs"),
            (typeof(SPM1xPowerNodeController), "Spm1x"),
            (typeof(CerusNodeController), "CerusXDrive"),
        };
    }
}