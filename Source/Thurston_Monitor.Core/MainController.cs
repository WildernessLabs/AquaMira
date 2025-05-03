using Meadow;
using Meadow.Units;
using System;
using System.Threading.Tasks;
using Thurston_Monitor.Core.Contracts;

namespace Thurston_Monitor.Core
{
    public class MainController
    {
        private IThurston_MonitorHardware hardware;

        private CloudController cloudController;
        private ConfigurationController configurationController;
        private DisplayController displayController;
        private SensorController sensorController;
        private StorageController storageController;

        private INetworkController NetworkController => hardware.NetworkController;
        private IInputController InputController => hardware.InputController;

        private readonly Temperature.UnitType units;
        private readonly Temperature currentTemperature;
        private Temperature thresholdTemperature;

        public MainController()
        {
        }

        public Task Initialize(IThurston_MonitorHardware hardware)
        {
            this.hardware = hardware;

            this.thresholdTemperature = 68.Fahrenheit();

            var a = Resolver.Device.NetworkAdapters;

            // create generic services
            configurationController = new ConfigurationController();
            storageController = new StorageController(configurationController);

            cloudController = new CloudController(
                Resolver.MeadowCloudService,
                Resolver.CommandService,
                storageController,
                NetworkController);

            sensorController = new SensorController(hardware, storageController);

            displayController = new DisplayController(
                this.hardware.Display,
                this.hardware.DisplayRotation,
                units);

            // connect events
            NetworkController.NetworkStatusChanged += OnNetworkStatusChanged;

            NetworkController.Connect();

            sensorController.ApplySensorConfig(
                configurationController.SensorConfiguration);

            _ = cloudController.ReportDeviceStartup();

            return Task.CompletedTask;
        }

        private void ConfigureAnalyzers(ConfigurationController configuration)
        {
        }

        private void OnNetworkStatusChanged(object sender, EventArgs e)
        {
            Resolver.Log.Info($"Network state changed to {NetworkController.IsConnected}");
            displayController.SetNetworkStatus(NetworkController.IsConnected);
        }

        public async Task Run()
        {
            while (true)
            {
                // add any app logic here

                await Task.Delay(5000);
            }
        }
    }
}