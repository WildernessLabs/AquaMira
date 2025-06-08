using Meadow;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.MicroLayout;
using Meadow.Peripherals.Displays;
using System.Collections.Generic;
using Thurston_Monitor.Core.Contracts;

namespace Thurston_Monitor.Core;

public class DisplayController
{
    private readonly DisplayScreen? screen;

    public DisplayController(
        IPixelDisplay? display,
        RotationType displayRotation,
        IThurston_MonitorHardware hardware)
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

    private readonly List<MicroLayout> navigationStack = new();
    private int _currentPage = 0;
    private DisplayTheme? theme;
    private AbsoluteLayout mainLayout;
    private StackLayout diagnosticLayout;
    private HomeLayout homeLayout;
    private HeaderControl headerControl;

    private void GenerateLayout(DisplayScreen screen)
    {
        theme = new DisplayTheme
        {
            BackgroundColor = Color.FromRgb(50, 50, 50)
        };

        mainLayout = new AbsoluteLayout(0, 0, screen.Width, screen.Height);

        headerControl = new HeaderControl(screen);

        homeLayout = new HomeLayout(0, headerControl.Bottom, screen.Width, screen.Height - headerControl.Height);
        navigationStack.Add(homeLayout);

        diagnosticLayout = new DiagnosticLayout(0, headerControl.Bottom, screen.Width, screen.Height - headerControl.Height);
        navigationStack.Add(diagnosticLayout);

        mainLayout.Add(headerControl, homeLayout, diagnosticLayout);
        diagnosticLayout.IsVisible = false;

        screen.Controls.Add(mainLayout);

        headerControl.ApplyTheme(theme);
    }

    private void OnNextRequested(object sender, System.EventArgs e)
    {
        if (screen == null) return;

        if (_currentPage >= navigationStack.Count - 1) return;

        screen.BeginUpdate();

        navigationStack[_currentPage].IsVisible = false;
        _currentPage++;
        navigationStack[_currentPage].IsVisible = true;
        screen.EndUpdate();
    }

    private void OnPreviousRequested(object sender, System.EventArgs e)
    {
        if (screen == null) return;

        if (_currentPage <= 0) return;

        screen.BeginUpdate();

        navigationStack[_currentPage].IsVisible = false;
        _currentPage--;
        navigationStack[_currentPage].IsVisible = true;
        screen.EndUpdate();
    }

    private void OnHomeRequested(object sender, System.EventArgs e)
    {
        if (screen == null) return;

        screen.BeginUpdate();

        if (_currentPage > 0)
        {
            navigationStack[_currentPage].IsVisible = false;
        }
        _currentPage = 0;
        navigationStack[_currentPage].IsVisible = true;
        screen.EndUpdate();
    }

    internal void WatchdogNotify()
    {
        headerControl.ToggleHeartbeat();
    }

    public void SetNetworkStatus(bool isConnected)
    {
        headerControl.SetConnectedState(isConnected);

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (screen == null) { return; }

        // TODO: do things
    }
}