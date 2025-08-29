# Voltaic Battery Controller

## Registration

Inside your app's `IAquaMiraHardware` implementation:

```csharp
public IEnumerable<(Type ControllerType, string ConfigurationName)> GetAvailableSensingNodeControllers()
{
    return new[]
    {
        (typeof(VoltaicBatteryNodeController), "Voltaic"),
    };
}
```

## Sample Configuration

The root node name must match the name used (second parameter) in the registration above.

```json
"Voltaic": {
	"ModbusAddress": 1,
	"Name": "Battery",
	"IsSimulated": false,
	"SenseIntervalSeconds": 6
}
```