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

        private IOutputController OutputController => hardware.OutputController;
        private INetworkController NetworkController => hardware.NetworkController;
        private IInputController InputController => hardware.InputController;

        private readonly Temperature.UnitType units;
        private Temperature currentTemperature;
        private Temperature thresholdTemperature;

        public MainController()
        {
        }

        public Task Initialize(IThurston_MonitorHardware hardware)
        {
            this.hardware = hardware;

            this.thresholdTemperature = 68.Fahrenheit();

            // create generic services
            configurationController = new ConfigurationController();
            cloudController = new CloudController(Resolver.CommandService);
            sensorController = new SensorController(hardware);

            displayController = new DisplayController(
                this.hardware.Display,
                this.hardware.DisplayRotation,
                units);

            // connect events
            NetworkController.NetworkStatusChanged += OnNetworkStatusChanged;

            NetworkController.Connect();

            InputController?.Configure(configurationController);

            sensorController.ApplySensorConfig(configurationController.SensorConfiguration);

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

        private void CheckTemperaturesAndSetOutput()
        {
            OutputController?.SetState(currentTemperature < thresholdTemperature);
        }

        private void OnCurrentTemperatureChanged(object sender, Temperature temperature)
        {
            currentTemperature = temperature;

            CheckTemperaturesAndSetOutput();

            // update the UI
            displayController.UpdateCurrentTemperature(currentTemperature);
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