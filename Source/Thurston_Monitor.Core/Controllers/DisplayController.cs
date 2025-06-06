using Meadow;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.MicroLayout;
using Meadow.Peripherals.Displays;

namespace Thurston_Monitor.Core;

public class DisplayController
{
    private readonly DisplayScreen? screen;

    private HomeLayout homelayout;
    private Picture heartbeatPicture;

    public DisplayController(
        IPixelDisplay? display,
        RotationType displayRotation)
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

    internal void WatchdogNotify()
    {
        heartbeatPicture.IsVisible = !heartbeatPicture.IsVisible;
    }

    private void GenerateLayout(DisplayScreen screen)
    {
        homelayout = new HomeLayout(screen);

        heartbeatPicture = new Picture(
            screen.Width - Resources.Heart.Width - 5,
            screen.Height - Resources.Heart.Height - 5,
            Resources.Heart.Width,
            Resources.Heart.Height,
            Resources.Heart);

        screen.Controls.Add(homelayout, heartbeatPicture);
    }

    public void SetNetworkStatus(bool isConnected)
    {
        homelayout.SetConnectedState(isConnected);

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (screen == null) { return; }

        // TODO: do things
    }
}