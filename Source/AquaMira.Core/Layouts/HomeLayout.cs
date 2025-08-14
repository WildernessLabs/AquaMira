using Meadow;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.MicroLayout;
using System;
using System.Collections.Generic;

namespace AquaMira.Core;

internal class HomeLayout : StackLayout
{
    private readonly DataGrid grid;
    private readonly Dictionary<string, int> dataToRoLookup = new();
    private readonly List<string> rowMap = new();

    private readonly IFont smallFont;

    public HomeLayout(int left, int top, int width, int height)
        : base(left, top, width, height)
    {
        smallFont = new Font8x8();

        this.BackgroundColor = Color.FromRgb(50, 50, 50);

        grid = new DataGrid(0, 0, width, height,
            new[]
            {
                new DataGrid.ColumnDefinition(210, "Sensor"),
                new DataGrid.ColumnDefinition(60, "Time") { TextColor = Color.Blue, HorizontalAlignment = HorizontalAlignment.Center },
                new DataGrid.ColumnDefinition(50, "Value"),
            })
        {
            HeaderFont = new Font8x12(),
            RowFont = smallFont,
            RowHeight = 12,
            HeaderRowHeight = 20,
            BackgroundColor = Color.DarkOliveGreen
        };

        this.Controls.Add(grid);

    }

    internal void UpdateSensorValues(Dictionary<string, object> e)
    {
        foreach (var key in e.Keys)
        {
            var index = rowMap.IndexOf(key);
            if (index == -1)
            {
                rowMap.Add(key);
                index = rowMap.Count - 1;
            }

            if (index >= grid.RowCount)
            {
                grid.AddRow(key, DateTime.Now.ToString("HH:mm:ss"), e[key]);
            }

            grid.UpdateCell(index, 0, key);
            grid.UpdateCell(index, 1, DateTime.Now.ToString("HH:mm:ss"));
            grid.UpdateCell(index, 2, e[key]);
        }
    }
}