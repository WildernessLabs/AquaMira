using Meadow;
using Meadow.Foundation.Graphics.MicroLayout;
using Meadow.Units;
using System.Collections.Generic;

namespace HardwareValidation;

internal class HomeLayout : StackLayout
{
    private readonly Label powerMeterLabel;
    private readonly Label ioExpanderLabel;
    private readonly Label vfdLabel;
    private readonly T3InputGrid t3InputGrid;

    public HomeLayout(int left, int top, int width, int height)
        : base(left, top, width, height)
    {
        this.BackgroundColor = Color.FromRgb(50, 50, 50);

        this.Controls.Add(new Label(width, 30, "AquaMira Hardware Validator")
        {
            TextColor = Color.White,
        });

        powerMeterLabel = new Label(width, 30, "Testing Power Meter...")
        {
            TextColor = Color.White,
        };

        ioExpanderLabel = new Label(width, 30, "Testing T3-22i...")
        {
            TextColor = Color.White,
        };

        t3InputGrid = new T3InputGrid();

        vfdLabel = new Label(width, 30, "Testing VFD...")
        {
            TextColor = Color.White,
        };

        this.Controls.Add(powerMeterLabel, ioExpanderLabel, t3InputGrid, vfdLabel);

        ShowT3Inputs(false);
    }

    public void SetPowerMeterInfo(string text)
    {
        powerMeterLabel.Text = text;
    }

    public void SetIOExpanderInfo(string text)
    {
        ioExpanderLabel.Text = text;
    }

    public void SetVFDInfo(string text)
    {
        vfdLabel.Text = text;
    }

    public void ShowT3Inputs(bool show)
    {
        t3InputGrid.IsVisible = show;
    }

    internal void SetDiscreteInputStates(Dictionary<string, bool> discreteStates)
    {
        t3InputGrid.SetDiscreteInputStates(discreteStates);
    }

    internal void ShowCurrentInputs(Dictionary<string, Current> currentInputs)
    {
        t3InputGrid.ShowCurrentInputs(currentInputs);
    }
}
