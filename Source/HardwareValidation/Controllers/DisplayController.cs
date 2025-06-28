using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.MicroLayout;
using Meadow.Peripherals.Displays;
using System.Collections.Generic;

namespace HardwareValidation;

public class DisplayController
{
    private readonly DisplayScreen? screen;
    private readonly List<MicroLayout> navigationStack = new();
    private int currentPage = 0;
    private DisplayTheme? theme;
    private AbsoluteLayout mainLayout;
    private HomeLayout homeLayout;

    public DisplayController(
        IPixelDisplay? display,
        RotationType displayRotation,
        IProjectLabHardware hardware)
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

        hardware.UpButton.Clicked += OnHomeRequested;
        hardware.LeftButton.Clicked += OnPreviousRequested;
        hardware.RightButton.Clicked += OnNextRequested;
    }

    private void GenerateLayout(DisplayScreen screen)
    {
        theme = new DisplayTheme
        {
            BackgroundColor = Color.FromRgb(50, 50, 50)
        };

        mainLayout = new AbsoluteLayout(0, 0, screen.Width, screen.Height);

        homeLayout = new HomeLayout(0, 0, screen.Width, screen.Height);
        navigationStack.Add(homeLayout);

        mainLayout.Add(homeLayout);

        screen.Controls.Add(mainLayout);
    }

    private void OnNextRequested(object sender, System.EventArgs e)
    {
        if (screen == null) return;

        if (currentPage >= navigationStack.Count - 1) return;

        screen.BeginUpdate();

        navigationStack[currentPage].IsVisible = false;
        currentPage++;
        navigationStack[currentPage].IsVisible = true;
        screen.EndUpdate();
    }

    private void OnPreviousRequested(object sender, System.EventArgs e)
    {
        if (screen == null) return;

        if (currentPage <= 0) return;

        screen.BeginUpdate();

        navigationStack[currentPage].IsVisible = false;
        currentPage--;
        navigationStack[currentPage].IsVisible = true;
        screen.EndUpdate();
    }

    private void OnHomeRequested(object sender, System.EventArgs e)
    {
        if (screen == null) return;

        screen.BeginUpdate();

        if (currentPage > 0)
        {
            navigationStack[currentPage].IsVisible = false;
        }
        currentPage = 0;
        navigationStack[currentPage].IsVisible = true;
        screen.EndUpdate();
    }

    private void UpdateDisplay()
    {
        if (screen == null) { return; }

        // TODO: do things
    }

    public void SetPowerMeterInfo(string text)
    {
        homeLayout.SetPowerMeterInfo(text);
    }

    public void SetIOExpanderInfo(string text)
    {
        homeLayout.SetIOExpanderInfo(text);
    }

    public void ShowT3Inputs()
    {
        homeLayout.ShowT3Inputs(true);
    }

    internal void SetDiscreteInputStates(Dictionary<string, bool> discreteStates)
    {
        screen?.BeginUpdate();
        homeLayout.SetDiscreteInputStates(discreteStates);
        screen?.EndUpdate();
    }
}