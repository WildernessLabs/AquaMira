using System;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Thurston_Monitor.Core;

namespace Thurston_Monitor.F7
{
    public class MeadowProjectLabApp : App<F7CoreComputeV2>
    {
        private MainController mainController;

        public override Task Initialize()
        {
            var hardware = new Thurston_MonitorProjectLabHardware(Device);
            mainController = new MainController();
            return mainController.Initialize(hardware);
        }

        public override Task Run()
        {
            return mainController.Run();
        }
    }
}