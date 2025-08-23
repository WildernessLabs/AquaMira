using Meadow;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.MicroLayout;
using System;
using System.Collections.Generic;

namespace AquaMira.Core;

internal class HomeLayout : StackLayout
{
    private readonly GridLayout dataGrid;
    private readonly Label[][] dataGridLabels;
    private readonly IFont smallFont;

    public HomeLayout(int left, int top, int width, int height)
        : base(left, top, width, height)
    {
        smallFont = new Font8x8();

        this.BackgroundColor = Color.FromRgb(50, 50, 50);

        var rowCount = 15;
        dataGridLabels = new Label[rowCount][];
        for (int i = 0; i < rowCount; i++)
        {
            dataGridLabels[i] = GetRow();
        }
        dataGrid = new GridLayout(0, 0, width, height, rowCount, 3);

        for (int r = 0; r < dataGridLabels.Length; r++)
        {
            for (int c = 0; c < dataGridLabels[r].Length; c++)
            {
                dataGrid.Add(dataGridLabels[r][c], r, c, 1, 1, GridLayout.Alignment.Left);
            }
        }
        this.Controls.Add(dataGrid);

    }

    private Label[] GetRow()
    {
        return new Label[3]
        {
                new Label(100, 20, "-") { Font = smallFont, TextColor = Color.White },
                new Label(100, 20, "-") { Font = smallFont, TextColor = Color.Blue, HorizontalAlignment = HorizontalAlignment.Right },
                new Label(100, 20, "-") { Font = smallFont, TextColor = Color.White },
        };
    }

    private readonly List<string> rowMap = new();

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

            if (index >= dataGridLabels.Length)
            {
                // If we run out of rows, just skip this key
                continue;
            }

            Resolver.Log.Info($"Updating row {index}", "DISPLAY");

            dataGridLabels[index][0].Text = key;
            dataGridLabels[index][1].Text = DateTime.Now.ToString("HH:mm:ss");
            dataGridLabels[index][2].Text = e[key].ToString();
        }
    }
}