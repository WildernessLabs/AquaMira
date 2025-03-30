using System;
using System.Threading.Tasks;

namespace Thurston_Monitor.Core
{
    public interface INetworkController
    {
        event EventHandler NetworkStatusChanged;

        Task Connect();
        bool IsConnected { get; }
    }
}