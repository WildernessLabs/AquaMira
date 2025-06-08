using Meadow.Hardware;

namespace Thurston_Monitor.Core.Contracts;

public interface IInputController
{
    IDigitalInputPort? GetInputForChannel(int channelNumber);
}
