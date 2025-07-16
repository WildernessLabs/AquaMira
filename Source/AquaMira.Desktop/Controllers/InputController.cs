using Meadow.Hardware;
using AquaMira.Core.Contracts;

namespace AquaMira.DT;

public class InputController : IInputController
{
    public InputController()
    {
    }

    public IDigitalInputPort? GetInputForChannel(int channelNumber)
    {
        return null;
    }
}
