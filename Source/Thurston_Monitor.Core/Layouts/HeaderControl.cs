using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.MicroLayout;

namespace Thurston_Monitor.Core;

internal class HeaderControl : AbsoluteLayout
{
    private readonly Picture disconnected;
    private readonly Picture connected;
    private readonly Picture heart;
    private readonly Label signal;

    public HeaderControl(DisplayScreen screen)
        : base(0, 0, screen.Width, 34)
    {
        var logo = new Picture(
                 0, 0,
                 Resources.LogoSmall.Width,
                 Resources.LogoSmall.Height,
                 Resources.LogoSmall);

        heart = new Picture(
                screen.Width - Resources.Heart.Width,
                3,
                Resources.Heart.Width,
                Resources.Heart.Height,
                Resources.Heart);

        connected = new Picture(
            heart.Left - Resources.NetConnected.Width,
            1,
            Resources.NetConnected.Width, Resources.NetConnected.Height, Resources.NetConnected);
        disconnected = new Picture(
            heart.Left - Resources.NetConnected.Width,
            1,
            Resources.NetDisconnected.Width, Resources.NetDisconnected.Height, Resources.NetDisconnected);

        signal = new Label(
            connected.Left - 40, 0, 40, logo.Height)
        {
            VerticalAlignment = VerticalAlignment.Center,
            Text = "--"
        };

        this.Controls.Add(logo, disconnected, connected, heart, signal);

        connected.IsVisible = false;
    }

    public void SetSignal(int? signal)
    {
        this.signal.Text = signal?.ToString() ?? "--";
    }

    public void SetConnectedState(bool isConnected)
    {
        connected.IsVisible = isConnected;
        disconnected.IsVisible = !isConnected;
    }

    public void ToggleHeartbeat()
    {
        heart.IsVisible = !heart.IsVisible;
    }
}
