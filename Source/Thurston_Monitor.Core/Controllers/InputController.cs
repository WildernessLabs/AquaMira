using System;
using System.Threading.Tasks;
using Thurston_Monitor.Core.Contracts;

namespace Thurston_Monitor.Core
{
    public class InputController
    {
        public event EventHandler? UnitDownRequested;
        public event EventHandler? UnitUpRequested;

        public InputController(IThurston_MonitorHardware platform)
        {
            if (platform.LeftButton is { } ub)
            {
                ub.PressStarted += (s, e) => UnitDownRequested?.Invoke(this, EventArgs.Empty);
            }
            if (platform.RightButton is { } db)
            {
                db.PressStarted += (s, e) => UnitDownRequested?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
