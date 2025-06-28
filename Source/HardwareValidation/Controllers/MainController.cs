using Meadow;
using Meadow.Devices;
using Meadow.Foundation.IOExpanders;
using Meadow.Foundation.Sensors.Power;
using Meadow.Hardware;
using Meadow.Peripherals.Displays;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HardwareValidation;

public class MainController
{
    private IProjectLabHardware hardware;
    private DisplayController displayController;

    private Spm1x? powerMeter;
    private T322ai? t3;

    private readonly List<IDigitalInputPort> t3DigitalInputs = new();

    public Task Initialize(IProjectLabHardware hardware)
    {
        this.hardware = hardware;

        displayController = new DisplayController(
            this.hardware.Display,
            RotationType._270Degrees,
            this.hardware);

        var modbus = hardware.GetModbusRtuClient(9600);

        try
        {
            powerMeter = new Spm1x(modbus, 2);
            var sn = powerMeter.SerialNumber;

            displayController.SetPowerMeterInfo("Power Meter Found");
            Resolver.Log.Info($"found power meter SN {sn}");
        }
        catch (Exception ex)
        {
            displayController.SetPowerMeterInfo("POWER METER FAULT!");
            Resolver.Log.Error($"Unable to create power meter: {ex.Message}");
        }

        try
        {
            t3 = new T322ai(modbus, 254);
            var retries = 3;

            while (true)
            {
                try
                {
                    Resolver.Log.Info($"Checking fot the T3...");

                    var sn = t3.ReadSerialNumber().GetAwaiter().GetResult();

                    displayController.SetIOExpanderInfo("T3-22i Found");
                    displayController.ShowT3Inputs();
                    Resolver.Log.Info($"found T3 SN {sn}");

                    t3DigitalInputs.Add(t3.Pins.AI7.CreateDigitalInputPort());
                    t3DigitalInputs.Add(t3.Pins.AI8.CreateDigitalInputPort());
                    t3DigitalInputs.Add(t3.Pins.AI9.CreateDigitalInputPort());
                    t3DigitalInputs.Add(t3.Pins.AI10.CreateDigitalInputPort());
                    break;
                }
                catch (TimeoutException)
                {
                    retries--;
                    if (retries <= 0) throw;
                }
            }
        }
        catch (Exception ex)
        {
            displayController.SetIOExpanderInfo("T3-22i FAULT!");
            Resolver.Log.Error($"Unable to create T3: {ex.Message}");
        }

        return Task.CompletedTask;
    }

    public async Task Run()
    {
        // this delay is required for the desktop to be able to start the UI thread.  Do not remove.
        await Task.Delay(100);

        while (true)
        {
            // add any app logic here
            try
            {
                if (powerMeter != null)
                {
                    try
                    {
                        var volts = await powerMeter.ReadVoltage();
                        var amps = await powerMeter.ReadCurrent();

                        displayController.SetPowerMeterInfo($"Power: {amps.Amps:N2}A @ {volts.Volts:N1}V");
                    }
                    catch (Exception ex)
                    {
                        displayController.SetPowerMeterInfo("POWER METER FAULT!");
                        Resolver.Log.Error($"Unable to read power meter: {ex.Message}");
                    }
                }

                if (t3 != null && t3DigitalInputs.Count > 0)
                {
                    try
                    {
                        var discreteStates = new Dictionary<string, bool>();
                        foreach (var input in t3DigitalInputs)
                        {
                            Resolver.Log.Info($"T3 check {input.Pin.Name}");
                            discreteStates.Add(input.Pin.Name, input.State);
                        }

                        displayController.SetDiscreteInputStates(discreteStates);
                    }
                    catch (Exception ex)
                    {
                        displayController.SetIOExpanderInfo("T3-22i FAULT!");
                        Resolver.Log.Error($"Unable to read T3 discretes: {ex.Message}");
                    }
                }

                await Task.Delay(1000);
            }
            catch (AggregateException e)
            {
                Resolver.Log.Error(e.InnerException.ToString());
                throw e;
            }
        }
    }
}
