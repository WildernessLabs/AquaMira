using Meadow;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.MicroLayout;
using Meadow.Peripherals.Displays;
using Meadow.Units;

namespace Thurston_Monitor.Core;

public class DisplayController
{
    private readonly DisplayScreen? screen;

    private MicroLayout _homelayout;

    public DisplayController(
        IPixelDisplay? display,
        RotationType displayRotation,
        Temperature.UnitType unit)
    {
        if (display != null)
        {
            var theme = new DisplayTheme
            {
                Font = new Font12x20(),
                BackgroundColor = Color.Black,
                TextColor = Color.White
            };

            screen = new DisplayScreen(
                display,
                rotation: displayRotation,
                theme: theme);

            GenerateLayout(screen);

            UpdateDisplay();
        }
        else
        {
            Resolver.Log.Warn("Display is null");
        }
    }

    private void GenerateLayout(DisplayScreen screen)
    {
        _homelayout = new HomeLayout(screen);

        screen.Controls.Add(_homelayout);
    }

    public void SetNetworkStatus(bool isConnected)
    {
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (screen == null) { return; }

        // TODO: do things
    }
}