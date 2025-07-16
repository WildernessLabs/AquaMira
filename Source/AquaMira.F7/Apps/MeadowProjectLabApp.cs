using AquaMira.Core;
using Meadow;
using Meadow.Devices;
using System;
using System.Threading.Tasks;

namespace AquaMira.F7
{
    public class MeadowProjectLabApp : App<F7CoreComputeV2>
    {
        private bool shutdownHappened = false;
        private const int WatchdogIntervalSeconds = 30;
        private MainController? mainController;

        public override Task Initialize()
        {
            new Task(async () =>
            {
                Device.WatchdogEnable(TimeSpan.FromSeconds(WatchdogIntervalSeconds));

                Resolver.Log.Info("Watchdog timer enabled");
                while (!shutdownHappened)
                {
                    await Task.Delay(TimeSpan.FromSeconds(3));
                    Device.WatchdogReset();
                    mainController?.WatchdogNotify();
                }
                Resolver.Log.Warn("Watchdog timer will trigger shortly");
            }, TaskCreationOptions.LongRunning)
                .Start();

            var hardware = new AquaMiraProjectLabHardware(Device);
            mainController = new MainController();
            return mainController.Initialize(hardware);
        }

        public override Task OnError(Exception e)
        {
            Resolver.Log.Info($"System error: {e.Message}");
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
            return mainController.Run();
        }
    }
}