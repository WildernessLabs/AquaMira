using Meadow.Hardware;

namespace AquaMira.Core.Contracts;

public interface IInputController
{
    IDigitalInputPort? GetInputForChannel(int channelNumber);
}
