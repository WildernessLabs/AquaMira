using AquaMira.Core;

namespace AquaMira.Tests;

public class ConfigurationControllerTests
{
    [Fact]
    public void Test1()
    {
        var controller = new ConfigurationController("inputs/sample-config.json");
    }

    [Fact]
    public void GetConfigurationNode_ReturnsCorrectJsonForModbusDevices()
    {
        var controller = new ConfigurationController("inputs/sample-config.json");

        var modbusJson = controller.GetConfigurationNode("ModbusDevices");

        Assert.NotNull(modbusJson);
        Assert.Contains("FranklinElectric.XDrive", modbusJson);
        Assert.Contains("Pump1_Motor", modbusJson);
    }

    [Fact]
    public void GetConfigurationNode_ReturnsCorrectJsonForFrequencyInputs()
    {
        var controller = new ConfigurationController("inputs/sample-config.json");

        var frequencyJson = controller.GetConfigurationNode("FrequencyInputs");

        Assert.NotNull(frequencyJson);
        Assert.Contains("VolumetricFlow", frequencyJson);
        Assert.Contains("Pump1 Output", frequencyJson);
    }

    [Fact]
    public void GetConfigurationNode_ReturnsCorrectJsonForConfigurableAnalogs()
    {
        var controller = new ConfigurationController("inputs/sample-config.json");

        var analogsJson = controller.GetConfigurationNode("ConfigurableAnalogs");

        Assert.NotNull(analogsJson);
        Assert.Contains("IsSimulated", analogsJson);
        Assert.Contains("Current_4_20", analogsJson);
        Assert.Contains("Temperature", analogsJson);
    }

    [Fact]
    public void GetConfigurationNode_ReturnsNullForNonExistentNode()
    {
        var controller = new ConfigurationController("inputs/sample-config.json");

        var result = controller.GetConfigurationNode("NonExistentNode");

        Assert.Null(result);
    }

    [Fact]
    public void GetConfigurationNode_ReturnsNullForT322iInputs_WhenNotInConfig()
    {
        var controller = new ConfigurationController("inputs/sample-config.json");

        var result = controller.GetConfigurationNode("T322iInputs");

        Assert.Null(result);
    }

    [Fact]
    public void GetRegisteredNodeNames_ReturnsAllAvailableNodes()
    {
        var controller = new ConfigurationController("inputs/sample-config.json");

        var nodeNames = controller.GetRegisteredNodeNames().ToList();

        Assert.Contains("ModbusDevices", nodeNames);
        Assert.Contains("FrequencyInputs", nodeNames);
        Assert.Contains("ConfigurableAnalogs", nodeNames);
        Assert.Contains("MyCusomSensorConfig", nodeNames); // Custom node in sample config
        Assert.Contains("AnotherSensorConfig", nodeNames); // Another custom node in sample config
        Assert.Contains("Sensors", nodeNames); // Empty array in sample config
        Assert.DoesNotContain("T322iInputs", nodeNames); // Not in the sample config
        Assert.DoesNotContain("DigitalInputs", nodeNames); // Not in the sample config
    }

    [Fact]
    public void GetRegisteredNodeNames_ReturnsCorrectCount()
    {
        var controller = new ConfigurationController("inputs/sample-config.json");

        var nodeNames = controller.GetRegisteredNodeNames().ToList();

        Assert.Equal(6, nodeNames.Count); // ModbusDevices, FrequencyInputs, ConfigurableAnalogs, Sensors, MyCusomSensorConfig, AnotherSensorConfig
    }

    [Fact]
    public void GetConfigurationNode_HandlesComplexNestedStructures()
    {
        var controller = new ConfigurationController("inputs/sample-config.json");

        var analogsJson = controller.GetConfigurationNode("ConfigurableAnalogs");

        Assert.NotNull(analogsJson);
        // Verify it includes the nested Channels array
        Assert.Contains("Channels", analogsJson);
        Assert.Contains("0-100F Temperature Sensor", analogsJson);
        Assert.Contains("-40-140F Temperature Sensor", analogsJson);
        Assert.Contains("0-30psi Pressure Sensor", analogsJson);
    }

    [Fact]
    public void GetCustomConfigurationNode_ReturnsAnotherSensorConfig()
    {
        var controller = new ConfigurationController("inputs/sample-config.json");

        var sensorConfigJson = controller.GetConfigurationNode("AnotherSensorConfig");

        Assert.NotNull(sensorConfigJson);
        Assert.Contains("Channels", sensorConfigJson);
        Assert.Contains("ModbusAddress", sensorConfigJson);
        Assert.Contains("0-30psi Pressure Sensor", sensorConfigJson);
        Assert.Contains("CustomProperty", sensorConfigJson);
    }

    [Fact]
    public void GetCustomConfigurationNode_ReturnsMyCustomSensorConfig()
    {
        var controller = new ConfigurationController("inputs/sample-config.json");

        var customConfigJson = controller.GetConfigurationNode("MyCustomSensorConfig");
        Assert.NotNull(customConfigJson);
    }
}