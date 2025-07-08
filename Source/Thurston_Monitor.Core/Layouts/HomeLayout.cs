using Meadow;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.MicroLayout;

namespace Thurston_Monitor.Core;

internal class HomeLayout : StackLayout
{
    protected IFont MediumFont { get; }
    protected IFont LargeFont { get; }

    private readonly GridLayout t3Grid;

    public HomeLayout(int left, int top, int width, int height)
        : base(left, top, width, height)
    {
        LargeFont = new Font12x20();
        MediumFont = new Font8x16();

        this.BackgroundColor = Color.FromRgb(50, 50, 50);

        this.Controls.Add(new Label(width, 30, "Hello!")
        {
            TextColor = Color.White,
        });
    }
}