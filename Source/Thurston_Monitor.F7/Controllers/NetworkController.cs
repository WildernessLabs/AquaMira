using Meadow;
using Meadow.Devices;
using Meadow.Hardware;
using System;
using Thurston_Monitor.Core;

namespace Thurston_Monitor.F7
{
    internal class NetworkController : INetworkController
    {
        public event EventHandler? NetworkStatusChanged;

        private readonly IWiFiNetworkAdapter? wifi;
        private readonly ICellNetworkAdapter? cell;

        public NetworkController(F7MicroBase device)
        {
            // TODO: determine what adapter is in use (cell/wifi) and handle that properly

            wifi = device.NetworkAdapters.Primary<IWiFiNetworkAdapter>();
            cell = device.NetworkAdapters.Primary<ICellNetworkAdapter>();

            if (wifi != null)
            {
                wifi.NetworkConnected += OnNetworkConnected;
                wifi.NetworkDisconnected += OnNetworkDisconnected;
            }
            else if (cell != null)
            {
                Resolver.Log.Info("Using Cell Network Adapter");
            }
            else
            {
                Resolver.Log.Error("No known Network Adapter");
            }

            Resolver.Device.PlatformOS.NtpClient.TimeChanged += OnNtpTimeSync;
        }

        private void OnNtpTimeSync(DateTime utcTime)
        {
            Resolver.Log.Info($"NTP Time sync. Time is now: {utcTime:MM/dd/yyyy HH:mm:ss}");
        }

        private void OnNetworkDisconnected(INetworkAdapter sender, NetworkDisconnectionEventArgs args)
        {
            // Handle logic when disconnected.
            Resolver.Log.Info("Network disconnected");
        }

        private void OnNetworkConnected(INetworkAdapter sender, NetworkConnectionEventArgs args)
        {
            // Handle logic when connected.

            Resolver.Log.Info("Network connected");
            Resolver.Device.PlatformOS.NtpClient.Synchronize();
        }

        public bool IsConnected
        {
            get => (wifi?.IsConnected ?? false) || (cell?.IsConnected ?? false);
        }
    }
}