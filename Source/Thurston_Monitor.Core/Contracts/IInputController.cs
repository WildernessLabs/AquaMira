using Meadow.Hardware;
using System.Collections.Generic;

namespace Thurston_Monitor.Core;

public interface IInputController
{
    List<IDigitalSignalAnalyzer> Analyzers { get; }
    void Configure(ConfigurationController configuration);

}
