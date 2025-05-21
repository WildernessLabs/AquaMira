using System;

namespace Thurston_Monitor.Core
{
    public interface INetworkController
    {
        event EventHandler NetworkStatusChanged;

        bool IsConnected { get; }
    }
}