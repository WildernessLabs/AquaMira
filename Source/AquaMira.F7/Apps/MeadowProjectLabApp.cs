using AquaMira.Core;
using AquaMira.Core.Contracts;
using Meadow;
using Meadow.Devices;
using System;
using System.Threading.Tasks;

namespace AquaMira.F7;

public class MeadowProjectLabApp : ProjectLabCoreComputeApp
{
    private bool shutdownHappened = false;
    private const int WatchdogIntervalSeconds = 30;
    private MainController? mainController;

    public override Task Initialize()
    {
        new Task(async () =>
        {
            Hardware.ComputeModule.WatchdogEnable(TimeSpan.FromSeconds(WatchdogIntervalSeconds));

            Resolver.Log.Info("Watchdog timer enabled");
            while (!shutdownHappened)
            {
                await Task.Delay(TimeSpan.FromSeconds(3));
                Hardware.ComputeModule.WatchdogReset();
                mainController?.WatchdogNotify();
            }
            Resolver.Log.Warn("Watchdog timer will trigger shortly");
        }, TaskCreationOptions.LongRunning)
            .Start();

        var svc = Resolver.UpdateService;

        svc.UpdateAvailable += async (updateService, info, token) =>
        {
            Resolver.Log.Info($"Update available!", "AquaMira");
        };

        svc.UpdateRetrieved += async (updateService, info, token) =>
        {
            Resolver.Log.Info($"Update retrieved!", "AquaMira");
        };

        svc.UpdateFailure += async (updateService, info, token) =>
        {
            Resolver.Log.Error($"Update failed: {info.Name}", "AquaMira");
        };

        svc.RetrieveProgress += async (updateService, info, token) =>
        {
            Resolver.Log.Info(
                $"Update progress: {info.DownloadProgress} bytes downloaded",
                "AquaMira"
            );
        };

        IAquaMiraHardware hardware;
        try
        {
            Resolver.Log.Info("Creating hardware");
            hardware = new AquaMiraProjectLabHardware(Hardware);
        }
        catch (Exception ex)
        {
            Resolver.Log.Error($"Error Creating hardware: {ex.Message}");
            throw;
        }

        try
        {
            mainController = new MainController();
            return mainController.Initialize(hardware);
        }
        catch (Exception ex)
        {
            Resolver.Log.Error($"Error Initializing MainController: {ex.Message}");
            throw;
        }
    }

    public override Task OnError(Exception e)
    {
        Resolver.Log.Info($"System error: {e.Message}");
        Resolver.Log.Info($" at {e.StackTrace}");
        shutdownHappened = true;

        return base.OnError(e);
    }

    public override Task OnShutdown()
    {
        Resolver.Log.Info($"Device is shutting down...");
        shutdownHappened = true;

        return base.OnShutdown();
    }

    public override Task Run()
    {
        return mainController!.Run();
    }
}