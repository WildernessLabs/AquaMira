using System;

namespace AquaMira.Core
{
    public interface INetworkController
    {
        event EventHandler<bool>? NetworkConnectedChanged;
        event EventHandler<int>? SignalStrengthChanged;

        bool IsConnected { get; }
    }
}