using AquaMira.Core.Contracts;
using Meadow;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AquaMira.Core;

public class MainController
{
    private IAquaMiraHardware hardware;

    private CloudController cloudController;
    private ConfigurationController configurationController;
    private DisplayController? displayController;
    private SensorController sensorController;
    private StorageController storageController;

    private INetworkController NetworkController => hardware.NetworkController;

    public MainController()
    {
    }

    public async Task Initialize(IAquaMiraHardware hardware)
    {
        Resolver.Log.Debug("MainController Initialize", Constants.LoggingSource);

        this.hardware = hardware;

        // create generic services
        configurationController = new ConfigurationController();
        storageController = new StorageController(configurationController);

        Resolver.Log.Debug("Creating NetworkController", Constants.LoggingSource);
        cloudController = new CloudController(
            Resolver.MeadowCloudService,
            Resolver.CommandService,
            storageController,
            NetworkController);

        // Register the CloudController so we can avoid passing it around to everyone that needs to log errors
        Resolver.Services.Add(cloudController);

        Resolver.Log.Debug("Creating SensorController", Constants.LoggingSource);
        sensorController = new SensorController(hardware, configurationController);

        // Dynamically register all available sensing node controllers for this hardware
        Resolver.Log.Debug("Registering available sensing node controllers", Constants.LoggingSource);
        var availableControllers = hardware.GetAvailableSensingNodeControllers();
        Resolver.Log.Info($"Found {availableControllers?.Count()} registered sensing node controllers", Constants.LoggingSource);
        if (availableControllers != null)
        {
            foreach (var controllerDescriptor in availableControllers)
            {
                Resolver.Log.Info($"Registering sensing node controller {controllerDescriptor.ControllerType.Name} with configuration name '{controllerDescriptor.ConfigurationName}'", Constants.LoggingSource);
                sensorController.RegisterSensingNodeController(controllerDescriptor);
            }
        }
        sensorController.SensorValuesUpdated += OnSensorValuesUpdated;

        Resolver.Log.Debug("Loading sensing node controllers", Constants.LoggingSource);
        await sensorController.LoadSensingNodeControllers(hardware);
        sensorController.Start();

        Resolver.Log.Debug("Creating DisplayController", Constants.LoggingSource);
        displayController = new DisplayController(
            this.hardware.Display,
            this.hardware.DisplayRotation,
            this.hardware);

        // connect events
        Resolver.Log.Debug("Connecting events", Constants.LoggingSource);
        NetworkController.NetworkConnectedChanged += OnNetworkConnectedChanged;
        NetworkController.SignalStrengthChanged += OnNetworkSignalStrengthChanged;

        _ = Task.Run(async () =>
        {
            Resolver.Log.Debug("Reporting device startup to cloud", Constants.LoggingSource);
            try
            {
                await cloudController.ReportDeviceStartup(Resolver.Device);
                await cloudController.ReportSensorConfiguration(sensorController);
            }
            catch (Exception ex)
            {
                Resolver.Log.Info($"Failed to send to cloud: {ex.Message}", Constants.LoggingSource);
            }
        });
    }

    private void OnSensorValuesUpdated(object sender, System.Collections.Generic.Dictionary<string, object> e)
    {
        storageController.RecordSensorValues(e);
        displayController?.UpdateSensorValues(e);
    }

    private void OnNetworkSignalStrengthChanged(object sender, int e)
    {
        displayController?.SetNetworkSignal(e);
    }

    public void WatchdogNotify()
    {
        displayController?.WatchdogNotify();
    }

    private void OnNetworkConnectedChanged(object sender, bool e)
    {
        Resolver.Log.Info($"Network connection state changed to {e}", Constants.LoggingSource);
        displayController?.SetNetworkStatus(e);
    }

    public async Task Run()
    {
        // this delay is required for the desktop to be able to start the UI thread.  Do not remove.
        await Task.Delay(100);

        while (true)
        {
            // add any app logic here
            try
            {
                sensorController.SensorProc.Wait(1000);
            }
            catch (AggregateException e)
            {
                Resolver.Log.Error(e.InnerException.ToString());
                throw e;
            }
        }
    }
}