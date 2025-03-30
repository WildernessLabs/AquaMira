using System;
using System.Threading.Tasks;
using Meadow.Foundation.Graphics;
using Meadow.Peripherals.Displays;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Atmospheric;
using Meadow.Peripherals.Sensors.Buttons;

namespace Thurston_Monitor.Core.Contracts
{
    public interface IThurston_MonitorHardware
    {
        // basic hardware
        IButton? LeftButton { get; }
        IButton? RightButton { get; }

        // complex hardware
        ITemperatureSensor? TemperatureSensor { get; }
        IPixelDisplay? Display { get; }
        RotationType DisplayRotation { get; }

        // platform-dependent services
        IOutputController OutputController { get; }
        INetworkController NetworkController { get; }
    }
}