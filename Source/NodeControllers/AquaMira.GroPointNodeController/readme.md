# GroPoint Node Controller

## Registration

Inside your app's `IAquaMiraHardware` implementation:

```csharp
public IEnumerable<(Type ControllerType, string ConfigurationName)> GetAvailableSensingNodeControllers()
{
    return new[]
    {
        (typeof(GroPointNodeController), "GroPoint"),
    };
}
```

## Sample Configuration

The root node name must match the name used (second parameter) in the registration above.

```json
"GroPoint": {
	"ModbusAddress": 1,
	"Name": "Pump",
	"IsSimulated": false,
	"SensorType": "Gplp2625_S_T_8",
	"SenseIntervalSeconds": 6
}
```