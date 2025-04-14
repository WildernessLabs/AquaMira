using Meadow;
using Meadow.Hardware;
using System.Collections.Generic;
using System.Threading;
using Thurston_Monitor.Core;

namespace Thurston_Monitor.F7;

public class InputController : Core.IInputController
{
    private Timer? analyzerTimer;

    public List<IDigitalSignalAnalyzer> Analyzers { get; } = new();

    public InputController(IPin[] pins)
    {
        Resolver.Log.Info("Digital inputs:");
        var index = 0;
        foreach (var pin in pins)
        {
            var a = pin.CreateDigitalSignalAnalyzer(false);
            Resolver.Log.Info($"  {index}: {a.GetType().Name}");
            Analyzers.Add(a);
            index++;
        }
    }

    public void Configure(ConfigurationController configuration)
    {
        // TODO: only configure desired inputs
        analyzerTimer = new Timer(AnalyzerTimerProc, null, 2000, 2000);
    }

    private void AnalyzerTimerProc(object _)
    {
        var index = 0;

        foreach (var a in Analyzers)
        {
            var f = a.GetMeanFrequency();

            Resolver.Log.Info($"Freq{index}: {f.Hertz:N0}");
            index++;
        }
    }
}
