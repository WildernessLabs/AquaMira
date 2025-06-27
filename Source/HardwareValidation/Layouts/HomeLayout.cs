using Meadow;
using Meadow.Foundation.Graphics.MicroLayout;

namespace HardwareValidation;

internal class HomeLayout : StackLayout
{
    private readonly Label powerMeterLabel;
    private readonly Label ioExpanderLabel;

    public HomeLayout(int left, int top, int width, int height)
        : base(left, top, width, height)
    {
        this.BackgroundColor = Color.FromRgb(50, 50, 50);

        this.Add(new Label(width, 30, "Thurston Hardware Validator")
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

        this.Add(powerMeterLabel, ioExpanderLabel);
    }

    public void SetPowerMeterInfo(string text)
    {
        powerMeterLabel.Text = text;
    }

    public void SetIOExpanderInfo(string text)
    {
        ioExpanderLabel.Text = text;
    }
}
