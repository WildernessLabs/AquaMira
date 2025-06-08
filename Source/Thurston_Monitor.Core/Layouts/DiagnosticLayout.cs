using Meadow;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.MicroLayout;

namespace Thurston_Monitor.Core;

internal class DiagnosticLayout : StackLayout
{
    private readonly ScrollingTextArea textConsole;

    public DiagnosticLayout(int left, int top, int width, int height)
        : base(left, top, width, height)
    {
        var label = new Label(this.Width, 30, "Diagnostics")
        {
            TextColor = Color.White,
        };

        textConsole = new ScrollingTextArea(
            2, 2, this.Width - 4, this.Height - 34,
            new Font8x8());

        textConsole.Add("1");
        textConsole.Add("2");

        this.Add(label, textConsole);
    }

    public void AddText(string item)
    {
        textConsole.Add(item);
    }
}
