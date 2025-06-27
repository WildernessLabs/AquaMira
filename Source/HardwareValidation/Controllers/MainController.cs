using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Power;
using Meadow.Peripherals.Displays;
using System;
using System.Threading.Tasks;

namespace HardwareValidation;

public class MainController
{
    private IProjectLabHardware hardware;
    private DisplayController displayController;
    private Spm1x? powerMeter;

    public Task Initialize(IProjectLabHardware hardware)
    {
        this.hardware = hardware;

        displayController = new DisplayController(
            this.hardware.Display,
            RotationType._270Degrees,
            this.hardware);

        try
        {
            powerMeter = new Spm1x(hardware.GetModbusRtuClient(), 2);
            var sn = powerMeter.SerialNumber;

            displayController.SetPowerMeterInfo("Power Meter Found");
            Resolver.Log.Info($"found power meter SN {sn}");
        }
        catch (Exception ex)
        {
            displayController.SetPowerMeterInfo("POWER METER FAULT!");
            Resolver.Log.Error($"Unable to create power meter: {ex.Message}");
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
