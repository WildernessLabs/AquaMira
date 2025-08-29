# Cerus XDrive

## Registration

Inside your app's `IAquaMiraHardware` implementation:

```csharp
public IEnumerable<(Type ControllerType, string ConfigurationName)> GetAvailableSensingNodeControllers()
{
    return new[]
    {
        (typeof(CerusNodeController), "CerusXDrive"),
    };
}
```

## Sample Configuration

The root node name must match the name used (second parameter) in the registration above.

```json
"CerusXDrive": {
	"ModbusAddress": 1,
	"Name": "Pump",
	"IsSimulated": false,
	"SenseIntervalSeconds": 6
}
```