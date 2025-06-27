using Meadow;
using Meadow.Foundation.Graphics.MicroLayout;

namespace HardwareValidation;

internal class HomeLayout : StackLayout
{
    private readonly Label powerMeterLabel;

    public HomeLayout(int left, int top, int width, int height)
        : base(left, top, width, height)
    {
        this.BackgroundColor = Color.FromRgb(50, 50, 50);

        this.Add(new Label(width, 30, "Thurston Hardware Validator")
        {
            TextColor = Color.White,
        });

        powerMeterLabel = new Label(width, 30, "PowerMeter")
        {
            TextColor = Color.White,
        };

        this.Add(powerMeterLabel);
    }

    public void SetPowerMeterInfo(string text)
    {
        powerMeterLabel.Text = text;
    }
}
