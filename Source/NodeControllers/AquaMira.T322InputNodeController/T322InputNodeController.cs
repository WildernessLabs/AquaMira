using AquaMira.Core;
using AquaMira.Core.Contracts;
using Meadow;
using Meadow.Foundation.IOExpanders;
using Meadow.Foundation.Serialization;
using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AquaMira;

public class T322InputNodeController : ISensingNodeController
{
    public IT322ai T3Module { get; private set; }

    public T322InputNodeController()
    {
    }

    public Task<IEnumerable<ISensingNode>> ConfigureFromJson(string configJson, IAquaMiraHardware hardware)
    {
        Resolver.Log?.Info("Configuring T3-22i input node controller from JSON");

        T322iConfiguration config;
        try
        {
            config = MicroJson.Deserialize<T322iConfiguration>(configJson);
            if (config == null)
            {
                Resolver.Log?.Error("Failed to deserialize T3-22i configuration from JSON");
                return Task.FromResult(Enumerable.Empty<ISensingNode>());
            }
        }
        catch (Exception ex)
        {
            Resolver.Log?.Error($"Failed to deserialize T3-22i configuration from JSON: {ex.Message}");
            return Task.FromResult(Enumerable.Empty<ISensingNode>());
        }

        if (config.IsSimulated)
        {
            T3Module = new SimulatedT322ai();
        }
        else
        {
            T3Module = new T322ai(hardware.GetModbusSerialClient(), (byte)config.ModbusAddress);
        }

        return ConfigureInputs(config.Channels);
    }

    private async Task<IEnumerable<ISensingNode>> ConfigureInputs(IEnumerable<ExtendedChannelConfig> channels)
    {
        // bus contention or processor starvation can make port initializations fail,
        // so we need to track what succeeds and keep retrying

        List<(ExtendedChannelConfig ChannelConfig, bool Success)> channelResults = new List<(ExtendedChannelConfig, bool)>();
        channelResults.AddRange(channels.Select(c => (c, false)));

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

        var nodelist = new List<ISensingNode>();

        do
        {
            for (var r = 0; r < channelResults.Count; r++)
            {
                var result = channelResults[r];
                Resolver.Log.Info($"Configuring T3-22i input channel {result.ChannelConfig.ChannelNumber} ({result.ChannelConfig.Name})...");

                try
                {
                    var capture = result;

                    ISensingNode? node = null;

                    switch (result.ChannelConfig.ChannelType)
                    {
                        case ChannelInputType.Current_4_20:
                        case ChannelInputType.Current_0_20:
                            // verify the pin is valid
                            var pin = T3Module.Pins.FirstOrDefault(p => (int)p.Key == result.ChannelConfig.ChannelNumber);
                            if (pin == null)
                            {
                                Resolver.Log.Error($"No T3 Pin for requested channel {result.ChannelConfig.ChannelNumber}");
                                break;
                            }
                            // create an input
                            ICurrentInputPort? cinput = null;

                            do
                            {
                                try
                                {
                                    cinput = await T3Module.CreateCurrentInputPort(pin);
                                }
                                catch (Exception iex)
                                {
                                    Resolver.Log.Info($"Failed to create current input port for channel {result.ChannelConfig.ChannelNumber}: {iex.Message}.  Will retry.");
                                    await Task.Delay(1000);
                                }
                            } while (cinput == null);

                            switch (result.ChannelConfig.UnitType)
                            {
                                case nameof(Temperature):
                                    node = new UnitizedSensingNode<Temperature>(result.ChannelConfig.Name, cinput, () => ReadUnitizedChannel(cinput, result.ChannelConfig),
                                    TimeSpan.FromSeconds(result.ChannelConfig.SenseIntervalSeconds));
                                    break;
                                default:
                                    node = new SensingNode(result.ChannelConfig.Name, cinput, () => ReadUnitizedChannel(cinput, result.ChannelConfig),
                                    TimeSpan.FromSeconds(result.ChannelConfig.SenseIntervalSeconds));
                                    break;
                            }
                            break;
                        case ChannelInputType.Voltage_0_10:
                            Resolver.Log.Warn($"Voltage inputs are not supported on the T3-22i module, skipping channel {result.ChannelConfig.ChannelNumber}");
                            break;
                        case ChannelInputType.Count:
                            Resolver.Log.Warn($"Count inputs are not supported on the T3-22i module, skipping channel {result.ChannelConfig.ChannelNumber}");
                            // verify the pin is valid
                            var countpin = T3Module.Pins.FirstOrDefault(p => (int)p.Key == result.ChannelConfig.ChannelNumber);
                            if (countpin == null)
                            {
                                Resolver.Log.Error($"No T3 Pin for requested channel {result.ChannelConfig.ChannelNumber}");
                                break;
                            }
                            // create an input
                            // TODO: need retry logic here
                            var countinput = T3Module.CreateCounter(countpin, Meadow.Hardware.InterruptMode.EdgeRising);
                            // register the input for reading
                            node = new SensingNode(result.ChannelConfig.Name, countinput, () =>
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
                            TimeSpan.FromSeconds(result.ChannelConfig.SenseIntervalSeconds)
                            );
                            break;
                        case ChannelInputType.DiscreteInput:
                            Resolver.Log.Warn($"Discrete inputs are not supported on the T3-22i module, skipping channel {result.ChannelConfig.ChannelNumber}");
                            break;
                    }

                    if (node != null)
                    {
                        nodelist.Add(node);
                    }

                    channelResults[r] = (channelResults[r].ChannelConfig, true);
                }
                catch (Exception ex)
                {
                    Resolver.Log.Error($"Failed to configure T322i input channel {result.ChannelConfig.ChannelNumber}: {ex.Message}");
                }
            }
        } while (channelResults.Any(c => !c.Success));

        return nodelist;
    }

    private IUnit? ReadUnitizedChannel(ICurrentInputPort cinput, ChannelConfig analog)
    {
        try
        {
            Resolver.Log.Info($"Reading T322i input channel {cinput.Pin.Name} ({analog.ChannelNumber})...");

            var rawCurrent = cinput.Read().GetAwaiter().GetResult();

            Resolver.Log.Info($"T322i input channel {cinput.Pin.Name} ({analog.ChannelNumber}) read successfully: {rawCurrent.Milliamps} mA");

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
