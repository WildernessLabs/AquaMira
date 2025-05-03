using Meadow.Peripherals.Sensors;
using System.Collections.Generic;

namespace Thurston_Monitor.Core;

public interface ICompositeSensor : ISensor
{
    Dictionary<string, object> GetCurrentValues();
}
