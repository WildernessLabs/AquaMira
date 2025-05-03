using Meadow.Hardware;
using System;
using Thurston_Monitor.Core.Contracts;

namespace Thurston_Monitor.DT;

internal class InputController : IInputController
{
    public IDigitalInputPort GetInputForChannel(int channelNumber)
    {
        throw new NotImplementedException();
    }
}