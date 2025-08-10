using Meadow;
using Meadow.Foundation.IOExpanders;
using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AquaMira.Core;

public interface ISensingNodeController
{
    Task ConfigureInputs(IEnumerable<ExtendedChannelConfig> channels);
}

public class T322InputController : ISensingNodeController
{
    public IT322ai T3Module { get; private set; }

    public T322InputController(IT322ai t3Module)
    {
        T3Module = t3Module;
    }

    public async Task ConfigureInputs(IEnumerable<ExtendedChannelConfig> channels)
    {
        try
        {
            // read the serial number to verify comms
            Resolver.Log.Info($"Connecting to a T3-22i...");
            var sn = await T3Module.ReadSerialNumber();
            Resolver.Log.Info($"T3-22i SN: {sn}");
        }
        catch (Exception ex)
        {
            Resolver.Log.Error($"Unable to connect to T3-22i: {ex.Message}");
        }

        foreach (var analog in channels)
        {
            try
            {
                var capture = analog;

                switch (analog.ChannelType)
                {
                    case ChannelInputType.Current_4_20:
                    case ChannelInputType.Current_0_20:
                        // verify the pin is valid
                        var pin = T3Module.Pins.FirstOrDefault(p => (int)p.Key == analog.ChannelNumber);
                        if (pin == null)
                        {
                            Resolver.Log.Error($"No T3 Pin for requested channel {analog.ChannelNumber}");
                            break;
                        }
                        // create an input
                        var cinput = await T3Module.CreateCurrentInputPort(pin);

                        ISensingNode node;

                        switch (analog.UnitType)
                        {
                            case nameof(Temperature):
                                node = new UnitizedSensingNode<Temperature>(analog.Name, cinput, () => ReadUnitizedChannel(cinput, analog),
                                TimeSpan.FromSeconds(analog.SenseIntervalSeconds));
                                break;
                            default:
                                node = new SensingNode(analog.Name, cinput, () => ReadUnitizedChannel(cinput, analog),
                                TimeSpan.FromSeconds(analog.SenseIntervalSeconds));
                                break;
                        }
                        break;
                    case ChannelInputType.Voltage_0_10:
                        Resolver.Log.Warn($"Voltage inputs are not supported on the T3-22i module, skipping channel {analog.ChannelNumber}");
                        break;
                    case ChannelInputType.Count:
                        Resolver.Log.Warn($"Count inputs are not supported on the T3-22i module, skipping channel {analog.ChannelNumber}");
                        // verify the pin is valid
                        var countpin = T3Module.Pins.FirstOrDefault(p => (int)p.Key == analog.ChannelNumber);
                        if (countpin == null)
                        {
                            Resolver.Log.Error($"No T3 Pin for requested channel {analog.ChannelNumber}");
                            break;
                        }
                        // create an input
                        var countinput = T3Module.CreateCounter(countpin, Meadow.Hardware.InterruptMode.EdgeRising);
                        // register the input for reading
                        var countNode = new SensingNode(analog.Name, countinput, () =>
                        {
                            try
                            {
                                var count = countinput.Count;
                                // TODO: need to figure out how to convert that to the units requested
                                return VolumetricFlow.Zero;
                            }
                            catch (Exception rex)
                            {
                                Resolver.Log.Error($"Failed to read counter input channel: {rex.Message}");
                                return null;
                            }
                        },
                        TimeSpan.FromSeconds(analog.SenseIntervalSeconds)
                        );
                        break;
                    case ChannelInputType.DiscreteInput:
                        Resolver.Log.Warn($"Discrete inputs are not supported on the T3-22i module, skipping channel {analog.ChannelNumber}");
                        break;
                }

            }
            catch (Exception ex)
            {
                Resolver.Log.Error($"Failed to configure T322i input channel {analog.ChannelNumber}");
            }
        }
    }

    private IUnit? ReadUnitizedChannel(ICurrentInputPort cinput, ChannelConfig analog)
    {
        try
        {
            Resolver.Log.Info($"Reading T322i input channel {cinput.Pin.Name} ({analog.ChannelNumber})...");

            var rawCurrent = cinput.Read().GetAwaiter().GetResult();
            return InputToUnitConverter.ConvertCurrentToUnit(
                rawCurrent,
                analog.UnitType,
                analog.Scale,
                analog.Offset);
        }
        catch (Exception rex)
        {
            Resolver.Log.Error($"Failed to read T322i input channel {cinput.Pin.Name}: {rex.Message}");
            return null;
        }
    }
}
