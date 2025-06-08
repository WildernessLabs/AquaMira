using Meadow;
using System.Threading.Tasks;
using Thurston_Monitor.Core.Contracts;

namespace Thurston_Monitor.Core;

public class MainController
{
    private IThurston_MonitorHardware hardware;

    private CloudController cloudController;
    private ConfigurationController configurationController;
    private DisplayController? displayController;
    private SensorController sensorController;
    private StorageController storageController;

    private INetworkController NetworkController => hardware.NetworkController;

    public MainController()
    {
    }

    public async Task Initialize(IThurston_MonitorHardware hardware)
    {
        this.hardware = hardware;

        var a = Resolver.Device.NetworkAdapters;

        // create generic services
        configurationController = new ConfigurationController();
        storageController = new StorageController(configurationController);

        cloudController = new CloudController(
            Resolver.MeadowCloudService,
            Resolver.CommandService,
            storageController,
            NetworkController);

        // Register the CloudController so we can avoid passing it around to everyone that needs to log errors
        Resolver.Services.Add(cloudController);

        sensorController = new SensorController(hardware, storageController);

        displayController = new DisplayController(
            this.hardware.Display,
            this.hardware.DisplayRotation,
            this.hardware);

        // connect events
        NetworkController.NetworkConnectedChanged += OnNetworkConnectedChanged;
        NetworkController.SignalStrengthChanged += OnNetworkSignalStrengthChanged;

        await sensorController.ApplySensorConfig(
            configurationController.SensorConfiguration);

        _ = Task.Run(async () =>
        {
            await cloudController.ReportDeviceStartup();
            await cloudController.ReportSensorConfiguration(configurationController.SensorConfiguration);
        });
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
        Resolver.Log.Info($"Network connection state changed to {e}");
        displayController?.SetNetworkStatus(e);
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