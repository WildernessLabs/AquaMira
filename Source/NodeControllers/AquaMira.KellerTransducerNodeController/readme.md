# Keller Transducer

## Registration

Inside your app's `IAquaMiraHardware` implementation:

```csharp
public IEnumerable<(Type ControllerType, string ConfigurationName)> GetAvailableSensingNodeControllers()
{
    return new[]
    {
        (typeof(KellerTransducerNodeController), "Keller"),
    };
}
```

## Sample Configuration

The root node name must match the name used (second parameter) in the registration above.

```json
"Keller": {
	"ModbusAddress": 1,
	"Name": "Transducer",
	"IsSimulated": false,
	"SenseIntervalSeconds": 6
}
```