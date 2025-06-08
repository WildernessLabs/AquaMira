using Meadow;
using Meadow.Devices;
using Meadow.Hardware;
using System;
using System.Threading.Tasks;
using Thurston_Monitor.Core;

namespace Thurston_Monitor.F7
{
    internal class NetworkController : INetworkController
    {
        public event EventHandler<bool>? NetworkConnectedChanged;
        public event EventHandler<int>? SignalStrengthChanged;

        private readonly IWiFiNetworkAdapter? wifi;
        private readonly ICellNetworkAdapter? cell;

        public NetworkController(F7MicroBase device)
        {
            // TODO: determine what adapter is in use (cell/wifi) and handle that properly

            wifi = device.NetworkAdapters.Primary<IWiFiNetworkAdapter>();
            cell = device.NetworkAdapters.Primary<ICellNetworkAdapter>();

            if (wifi != null)
            {
                Resolver.Log.Info("Using WiFi Network Adapter");
                wifi.NetworkConnected += OnNetworkConnected;
                wifi.NetworkDisconnected += OnNetworkDisconnected;
            }
            else if (cell != null)
            {
                Resolver.Log.Info("Using Cell Network Adapter", "Thurston");
                cell.NetworkConnected += OnNetworkConnected;
                cell.NetworkDisconnected += OnNetworkDisconnected;

                Resolver.Log.Info($"  IMEI: {cell.Imei}", "Thurston");
                Resolver.Log.Info($"  CSQ:  {cell.Csq}", "Thurston");
            }
            else
            {
                Resolver.Log.Error("No known Network Adapter");
            }

            _ = Task.Run(SignalMonitor);

            Resolver.Device.PlatformOS.NtpClient.TimeChanged += OnNtpTimeSync;
        }

        private async Task SignalMonitor()
        {
            while (true)
            {
                if (cell != null)
                {
                    var signal = cell.GetSignalQuality();
                    Resolver.Log.Debug($"Cell signal: {signal}", "Thurston");
                    SignalStrengthChanged?.Invoke(this, signal);
                }
                else if (wifi != null)
                {
                    // TBD
                }

                await Task.Delay(60000);
            }
        }

        private void OnNtpTimeSync(DateTime utcTime)
        {
            Resolver.Log.Info($"NTP Time sync. Time is now: {utcTime:MM/dd/yyyy HH:mm:ss}", "Thurston");
        }

        private void OnNetworkDisconnected(INetworkAdapter sender, NetworkDisconnectionEventArgs args)
        {
            // Handle logic when disconnected.
            Resolver.Log.Info("Network disconnected", "Thurston");
            NetworkConnectedChanged?.Invoke(this, false);
        }

        private void OnNetworkConnected(INetworkAdapter sender, NetworkConnectionEventArgs args)
        {
            // Handle logic when connected.

            Resolver.Log.Info("Network connected", "Thurston");
            NetworkConnectedChanged?.Invoke(this, true);
            Resolver.Device.PlatformOS.NtpClient.Synchronize();
        }

        public bool IsConnected
        {
            get => (wifi?.IsConnected ?? false) || (cell?.IsConnected ?? false);
        }
    }
}