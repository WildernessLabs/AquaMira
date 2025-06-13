using Meadow;
using Meadow.Foundation.Sensors.Hid;
using Meadow.Hardware;
using System;
using System.Threading.Tasks;
using Thurston_Monitor.Core;

namespace Thurston_Monitor.DT;

internal class NetworkController : INetworkController
{
    private bool isConnected;

    public event EventHandler<bool>? NetworkConnectedChanged;
    public event EventHandler<int>? SignalStrengthChanged;

    public NetworkController(Keyboard? keyboard)
    {
        if (keyboard != null)
        {
            // allow the app to simulate network up/down with the keyboard
            keyboard.Pins.Plus.CreateDigitalInterruptPort(InterruptMode.EdgeRising).Changed += (s, e) => { _ = Connect(); };
            keyboard.Pins.Minus.CreateDigitalInterruptPort(InterruptMode.EdgeRising).Changed += (s, e) => { IsConnected = false; };
        }
    }

    public bool IsConnected
    {
        get => isConnected;
        private set
        {
            if (value == IsConnected) { return; }
            isConnected = value;
            NetworkConnectedChanged?.Invoke(this, value);
        }
    }

    public async Task Connect()
    {
        // simulate connection delay
        await Task.Delay(1000);

        SignalStrengthChanged?.Invoke(this, -99);
        IsConnected = true;
    }
}