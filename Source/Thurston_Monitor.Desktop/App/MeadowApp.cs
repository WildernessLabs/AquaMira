using Meadow;
using Meadow.Foundation.Displays;
using Meadow.Logging;
using System;
using System.Threading.Tasks;
using Thurston_Monitor.Core;

namespace Thurston_Monitor.DT
{
    internal class MeadowApp : App<Desktop>
    {
        private readonly bool shutdownHappened = false;
        private const int WatchdogIntervalSeconds = 30;
        private MainController? mainController;

        public override Task Initialize()
        {
            new Task(async () =>
            {
                while (!shutdownHappened)
                {
                    await Task.Delay(TimeSpan.FromSeconds(3));
                    mainController?.WatchdogNotify();
                }
            }, TaskCreationOptions.LongRunning)
                .Start();

            // output log messages to the VS debug window
            Resolver.Log.AddProvider(new DebugLogProvider());

            var hardware = new Thurston_MonitorHardware(Device);
            mainController = new MainController();
            return mainController.Initialize(hardware);
        }

        public override Task Run()
        {
            // this must be spawned in a worker because the UI needs the main thread
            _ = mainController.Run();

            ExecutePlatformDisplayRunner();

            return base.Run();
        }

        private void ExecutePlatformDisplayRunner()
        {
            if (Device.Display is SilkDisplay silkDisplay)
            {
                Program.InvokeOnMainThread(silkDisplay.Run);
            }
        }
    }
}