using Meadow.Units;
using Sensors.Flow.HallEffect.Simulation;

namespace AquaMira.Tests;

public class SimulatedFlowSensorTests
{
    [Fact]
    public async Task BasicSawtoothTest()
    {
        var sensor = new SimulatedHallEffectFlowSensor();
        sensor.MinimumSimulatedValue = VolumetricFlow.Zero;
        sensor.MaximumSimulatedValue = new VolumetricFlow(100, VolumetricFlow.UnitType.LitersPerMinute);
        sensor.StartSimulation(Meadow.Peripherals.Sensors.SimulationBehavior.Sawtooth);

        var list = new List<VolumetricFlow>();

        for (var i = 0; i < 10; i++)
        {
            list.Add(sensor.Flow);
            await Task.Delay(1001);
        }
    }
}
