using AquaMira.Core;
using Meadow;
using Meadow.Hardware;
using System;
using System.Threading.Tasks;

namespace AquaMira.F7
{
    internal class NetworkController : INetworkController
    {
        public event EventHandler<bool>? NetworkConnectedChanged;
        public event EventHandler<int>? SignalStrengthChanged;

        private readonly IWiFiNetworkAdapter? wifi;
        private readonly ICellNetworkAdapter? cell;
        private readonly AquaMiraAppSettings settings;

        public bool IsCellular { get; } = false;

        public NetworkController(IMeadowDevice device)
        {
            device.PlatformOS.TimeChanged += (s) =>
            {
                Resolver.Log.Info($"System time changed to {DateTime.UtcNow:MM/dd/yyyy HH:mm:ss}", "AquaMira");
            };

            this.settings = ConfigurationController.AppSettings;

            wifi = device.NetworkAdapters.Primary<IWiFiNetworkAdapter>();
            cell = device.NetworkAdapters.Primary<ICellNetworkAdapter>();

            if (wifi != null)
            {
                Resolver.Log.Info("Using WiFi Network Adapter");
                wifi.NetworkConnected += OnNetworkConnected;
                wifi.NetworkDisconnected += OnNetworkDisconnected;
                wifi.NetworkConnectFailed += OnNetworkConnectFailed;
                wifi.NetworkError += OnNetworkError;
            }
            else if (cell != null)
            {
                Resolver.Log.Info("Using Cell Network Adapter", "AquaMira");
                cell.NetworkConnected += OnNetworkConnected;
                cell.NetworkDisconnected += OnNetworkDisconnected;

                IsCellular = true;

                Resolver.Log.Info($"  IMEI: {cell.Imei}", "AquaMira");
                Resolver.Log.Info($"  CSQ:  {cell.Csq}", "AquaMira");
            }
            else
            {
                Resolver.Log.Error("No known Network Adapter");
            }

            if (!_resetDeviceOnCellError)
            {
                Resolver.Log.Info("+++ PWRKEY HIGH", "AquaMira");
                PWRKEY.State = true; // de-assert PWRKEY - this is a bug in the native driver
            }

            Resolver.Device.PlatformOS.NtpClient.TimeChanged += OnNtpTimeSync;
        }

        private void PlatformOS_TimeChanged(DateTime utcTime)
        {
            throw new NotImplementedException();
        }

        private void OnNetworkError(INetworkAdapter sender, NetworkErrorEventArgs args)
        {
            Resolver.Log.Info($"{sender.GetType().Name} Network Error: {args.ErrorCode}", Constants.LoggingSource);
        }

        private void OnNetworkConnectFailed(INetworkAdapter sender)
        {
            Resolver.Log.Info($"{sender.GetType().Name} Network connect failed", Constants.LoggingSource);
        }

        private IDigitalOutputPort? _h10Output;

        private IDigitalOutputPort PWRKEY
        {
            get
            {
                if (_h10Output == null)
                {
                    var ctrl = Resolver.Services.Get<ModemControl>();
                    var pin = ctrl.ResetPin;
                    Resolver.Log.Info($"+++ Creating PWRKEY output on pin {pin.Name}", "AquaMira");
                    _h10Output = Resolver.Device.CreateDigitalOutputPort(pin, true);
                }
                return _h10Output;
            }
        }

        private readonly bool _resetDeviceOnCellError = false;

        public Task ResetModem()
        {
            if (cell != null)
            {
                if (_resetDeviceOnCellError)
                {
                    Resolver.Device.PlatformOS.Reset();
                }
                else
                {
                    return Task.Run(async () =>
                    {
                        Resolver.Log.Info("+++ Resetting cellular modem", "AquaMira");

                        // HACK HACK HACK!
                        // this is just a POC to see if this fixes it on ProjLab 3.e

                        await Task.Delay(5000);// just a test wait

                        // according to 3.7.2.1 of the EG21-G manual
                        // hold low for > 650ms, then 29.5s later it will power off
                        Resolver.Log.Info("+++ PWRKEY LOW", "AquaMira");
                        PWRKEY.State = false; // assert low to power off
                        await Task.Delay(1000); // hold for 1 second
                        Resolver.Log.Info("+++ PWRKEY HIGH", "AquaMira");
                        PWRKEY.State = true; // de-assert
                        await Task.Delay(35000); // wait 35 seconds for it to power down
                        Resolver.Log.Info("+++ Cellular modem should be OFF", "AquaMira");
                        await Task.Delay(1000); // wait for 5 seconds just so you can visually verify after the above message
                        PWRKEY.State = false; // pulse it low again, for > 500ms
                        await Task.Delay(1000); // hold for 1 second
                        PWRKEY.State = true; // de-assert
                        Resolver.Log.Info("+++ Cellular modem should be ON", "AquaMira");

                        Resolver.Log.Info("+++ Cellular modem reset complete, now resetting Meadow", "AquaMira");
                        Resolver.Log.Info("+++ Resetting cellular modem", "AquaMira");
                    });
                }
            }

            return Task.CompletedTask;
        }

        private async Task SignalMonitor()
        {
            while (true)
            {
                if (cell != null)
                {
                    var signal = cell.GetSignalQuality();
                    Resolver.Log.Debug($"Cell signal: {signal}", "AquaMira");
                    SignalStrengthChanged?.Invoke(this, signal);
                }
                else if (wifi != null)
                {
                    // TBD
                }

                await Task.Delay(settings.NetworkSignalRefreshSeconds);
            }
        }

        private void OnNtpTimeSync(DateTime utcTime)
        {
            Resolver.Log.Info($"NTP Time sync. Time is now: {utcTime:MM/dd/yyyy HH:mm:ss}", "AquaMira");
        }

        private void OnNetworkDisconnected(INetworkAdapter sender, NetworkDisconnectionEventArgs args)
        {
            // Handle logic when disconnected.
            Resolver.Log.Info("Network disconnected", "AquaMira");
            NetworkConnectedChanged?.Invoke(this, false);
        }

        private void OnNetworkConnected(INetworkAdapter sender, NetworkConnectionEventArgs args)
        {
            // Handle logic when connected.

            Resolver.Log.Info("Network connected", "AquaMira");
            NetworkConnectedChanged?.Invoke(this, true);
            Resolver.Device.PlatformOS.NtpClient.Synchronize();
        }

        public bool IsConnected
        {
            get => (wifi?.IsConnected ?? false) || (cell?.IsConnected ?? false);
        }
    }
}