using System;

namespace Thurston_Monitor.Core
{
    public interface INetworkController
    {
        event EventHandler<bool>? NetworkConnectedChanged;
        event EventHandler<int>? SignalStrengthChanged;

        bool IsConnected { get; }
    }
}