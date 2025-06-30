using Meadow;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.MicroLayout;
using Meadow.Units;
using System.Collections.Generic;

namespace HardwareValidation;

internal class T3InputGrid : GridLayout
{
    private readonly IFont font;
    private readonly Label in1Label;
    private readonly Label in1ValueLabel;
    private readonly Label in2Label;
    private readonly Label in2ValueLabel;
    private readonly Label in3Label;
    private readonly Label in3ValueLabel;
    private readonly Label in4Label;
    private readonly Label in4ValueLabel;
    private readonly Label in5Label;
    private readonly Label in5ValueLabel;
    private readonly Label in6Label;
    private readonly Label in6ValueLabel;
    private readonly Label in7Label;
    private readonly Label in7ValueLabel;
    private readonly Label in8Label;
    private readonly Label in8ValueLabel;
    private readonly Label in9Label;
    private readonly Label in9ValueLabel;
    private readonly Label in10Label;
    private readonly Label in10ValueLabel;

    public T3InputGrid()
        : base(0, 0, 320, 90, 6, 4)
    {
        font = new Font8x12();

        in1Label = new Label(20, 20, "IN1") { Font = font };
        this.Add(in1Label, 0, 0, alignment: Alignment.Center);
        in1ValueLabel = new Label(20, 20, "0mA") { Font = font };
        this.Add(in1ValueLabel, 0, 1, alignment: Alignment.Left);
        in2Label = new Label(20, 20, "IN2") { Font = font };
        this.Add(in2Label, 1, 0, alignment: Alignment.Center);
        in2ValueLabel = new Label(20, 20, "0mA") { Font = font };
        this.Add(in2ValueLabel, 1, 1, alignment: Alignment.Left);
        in3Label = new Label(20, 20, "IN3") { Font = font };
        this.Add(in3Label, 2, 0, alignment: Alignment.Center);
        in3ValueLabel = new Label(20, 20, "0mA") { Font = font };
        this.Add(in3ValueLabel, 2, 1, alignment: Alignment.Left);
        in4Label = new Label(20, 20, "IN4") { Font = font };
        this.Add(in4Label, 3, 0, alignment: Alignment.Center);
        in4ValueLabel = new Label(20, 20, "0mA") { Font = font };
        this.Add(in4ValueLabel, 3, 1, alignment: Alignment.Left);
        in5Label = new Label(20, 20, "IN5") { Font = font };
        this.Add(in5Label, 4, 0, alignment: Alignment.Center);
        in5ValueLabel = new Label(20, 20, "0mA") { Font = font };
        this.Add(in5ValueLabel, 4, 1, alignment: Alignment.Left);
        in6Label = new Label(20, 15, "IN6") { Font = font };
        this.Add(in6Label, 5, 0, alignment: Alignment.Center);
        in6ValueLabel = new Label(20, 20, "0mA") { Font = font };
        this.Add(in6ValueLabel, 5, 1, alignment: Alignment.Left);

        in7Label = new Label(20, 20, "IN7") { Font = font };
        this.Add(in7Label, 0, 2, alignment: Alignment.Center);
        in7ValueLabel = new Label(20, 20, "OPEN") { Font = font };
        this.Add(in7ValueLabel, 0, 3, alignment: Alignment.Left);
        in8Label = new Label(20, 20, "IN8") { Font = font };
        this.Add(in8Label, 1, 2, alignment: Alignment.Center);
        in8ValueLabel = new Label(20, 20, "OPEN") { Font = font };
        this.Add(in8ValueLabel, 1, 3, alignment: Alignment.Left);
        in9Label = new Label(20, 20, "IN9") { Font = font };
        this.Add(in9Label, 2, 2, alignment: Alignment.Center);
        in9ValueLabel = new Label(20, 20, "OPEN") { Font = font };
        this.Add(in9ValueLabel, 2, 3, alignment: Alignment.Left);
        in10Label = new Label(20, 20, "IN10") { Font = font };
        this.Add(in10Label, 3, 2, alignment: Alignment.Center);
        in10ValueLabel = new Label(20, 20, "OPEN") { Font = font };
        this.Add(in10ValueLabel, 3, 3, alignment: Alignment.Left);
    }

    internal void ShowCurrentInputs(Dictionary<string, Current> currentInputs)
    {
        foreach (var input in currentInputs)
        {
            Resolver.Log.Info($"{input.Key}: {input.Value.Amps:N2}A");
            var label = input.Key switch
            {
                "AI1" => in1ValueLabel,
                "AI2" => in2ValueLabel,
                "AI3" => in3ValueLabel,
                "AI4" => in4ValueLabel,
                "AI5" => in5ValueLabel,
                "AI6" => in6ValueLabel,
                _ => null
            };
            if (label != null)
            {
                label.Text = $"{input.Value.Milliamps:N1}mA";
            }
        }
    }

    internal void SetDiscreteInputStates(Dictionary<string, bool> discreteStates)
    {
        foreach (var state in discreteStates)
        {
            Resolver.Log.Info($"{state.Key}: {state.Value}");
            var label = state.Key switch
            {
                "AI7" => in7ValueLabel,
                "AI8" => in8ValueLabel,
                "AI9" => in9ValueLabel,
                "AI10" => in10ValueLabel,
                _ => null
            };

            if (label != null)
            {
                label.Text = state.Value ? "CLOSED" : "OPEN";
            }
        }
    }
}
