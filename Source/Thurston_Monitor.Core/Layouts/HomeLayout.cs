using Meadow;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.MicroLayout;

namespace Thurston_Monitor.Core;

internal class HomeLayout : StackLayout
{
    protected IFont MediumFont { get; }
    protected IFont LargeFont { get; }

    private readonly Picture disconnected;
    private readonly Picture connected;

    public HomeLayout(DisplayScreen screen)
        : base(0, 0, screen.Width, screen.Height)
    {
        LargeFont = new Font12x20();
        MediumFont = new Font8x16();

        this.BackgroundColor = Color.FromRgb(50, 50, 50);

        this.Add(new Picture(Resources.Logo.Width, Resources.Logo.Height, Resources.Logo));
        this.Add(new Label(screen.Width, 30, "Hello!")
        {
            TextColor = Color.White,
        });

        connected = new Picture(Resources.NetConnected.Width, Resources.NetConnected.Height, Resources.NetConnected);
        disconnected = new Picture(Resources.NetDisconnected.Width, Resources.NetDisconnected.Height, Resources.NetDisconnected);

        this.Add(disconnected);
        this.Add(connected);

        connected.IsVisible = false;
    }

    public void SetConnectedState(bool isConnected)
    {
        connected.IsVisible = isConnected;
        disconnected.IsVisible = !isConnected;
    }
}