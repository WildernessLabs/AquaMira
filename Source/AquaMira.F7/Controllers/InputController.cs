using Meadow;
using Meadow.Devices;
using Meadow.Hardware;
using AquaMira.Core.Contracts;

namespace AquaMira.F7;

public class InputController : IInputController
{
    private readonly IProjectLabHardware projLab;

    public InputController(IProjectLabHardware projLab)
    {
        this.projLab = projLab;
    }

    public IDigitalInputPort? GetInputForChannel(int channelNumber)
    {
        switch (channelNumber)
        {
            //case 0:
            //    // TODO: for testing this is I3
            //    return projLab.IOTerminal.Pins.D3.CreateDigitalInputPort(ResistorMode.ExternalPullDown);
            default:
                // TODO: log the attempt to configure an invalid channel
                Resolver.Log.Info($"Invalid digital input channel requested {channelNumber}");
                return null;
        }
    }
}
