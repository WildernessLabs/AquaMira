using Meadow.Units;
using System;
using Thurston_Monitor.Core.Contracts;

namespace Thurston_Monitor.Core
{
    public class SensorController
    {
        private Temperature temperature;

        public event EventHandler<Temperature> CurrentTemperatureChanged = default!;

        public SensorController(IThurston_MonitorHardware platform)
        {
        }

        public Temperature CurrentTemperature
        {
            get => temperature;
            private set
            {
                if (value == CurrentTemperature) return;
                temperature = value;
                CurrentTemperatureChanged?.Invoke(this, CurrentTemperature);
            }
        }

        private void OnTemperatureUpdated(object sender, Meadow.IChangeResult<Meadow.Units.Temperature> e)
        {
            CurrentTemperature = e.New;
        }
    }
}