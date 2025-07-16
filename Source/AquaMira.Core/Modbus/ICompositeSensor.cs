using Meadow.Foundation.Sensors;
using Meadow.Peripherals.Sensors;
using System;
using System.Collections.Generic;

namespace AquaMira.Core;

public class RandomizedSimulatedDigitalInputPort : SimulatedDigitalInputPort
{
    private readonly Random _random = new();

    public RandomizedSimulatedDigitalInputPort(string? name = null, bool initialState = false)
        : base(name)
    {
    }

    public override bool State
    {
        get => _random.Next(2) % 2 == 0;
        set => base.State = value;
    }
}

public interface ICompositeSensor : ISensor
{
    Dictionary<string, object> GetCurrentValues();
}
