using Meadow.Hardware;
using Thurston_Monitor.Core.Contracts;

namespace Thurston_Monitor.DT;

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
