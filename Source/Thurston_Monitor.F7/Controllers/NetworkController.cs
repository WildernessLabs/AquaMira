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
        private const string WIFI_NAME = "interwebs";
        private const string WIFI_PASSWORD = "1234567890";

        public event EventHandler? NetworkStatusChanged;

        private readonly IWiFiNetworkAdapter? wifi;

        public NetworkController(F7MicroBase device)
        {
            wifi = device.NetworkAdapters.Primary<IWiFiNetworkAdapter>();

            wifi.NetworkConnected += OnNetworkConnected;
            wifi.NetworkDisconnected += OnNetworkDisconnected;

            Resolver.Device.PlatformOS.NtpClient.TimeChanged += OnNtpTimeSync;
        }

        private void OnNtpTimeSync(DateTime utcTime)
        {
            Resolver.Log.Info($"NTP Time sync. Time is now: {utcTime:MM/dd/yyyy HH:mm:ss}");
        }

        private void OnNetworkDisconnected(INetworkAdapter sender, NetworkDisconnectionEventArgs args)
        {
            // Handle logic when disconnected.
        }

        private void OnNetworkConnected(INetworkAdapter sender, NetworkConnectionEventArgs args)
        {
            // Handle logic when connected.

            Resolver.Log.Info("Network connected");
            Resolver.Device.PlatformOS.NtpClient.Synchronize();
        }

        public bool IsConnected
        {
            get => wifi.IsConnected;
        }

        public async Task Connect()
        {
            await wifi.Connect(WIFI_NAME, WIFI_PASSWORD, TimeSpan.FromSeconds(45));
        }
    }
}