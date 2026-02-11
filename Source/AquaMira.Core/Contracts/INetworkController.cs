using System;
using System.Threading.Tasks;

namespace AquaMira.Core
{
    public interface INetworkController
    {
        event EventHandler<bool>? NetworkConnectedChanged;
        event EventHandler<int>? SignalStrengthChanged;

        bool IsConnected { get; }
        bool IsCellular { get; }

        Task ResetModem();
    }
}