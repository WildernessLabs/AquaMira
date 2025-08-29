# Temco SPM-1x Power Meter

## Registration

Inside your app's `IAquaMiraHardware` implementation:

```csharp
public IEnumerable<(Type ControllerType, string ConfigurationName)> GetAvailableSensingNodeControllers()
{
    return new[]
    {
        (typeof(SPM1xPowerNodeController), "Spm1x"),
    };
}
```

## Sample Configuration

The root node name must match the name used (second parameter) in the registration above.

```json
"Spm1x": {
    "ModbusAddress": 2,
    "Name": "InputPower",
    "IsSimulated": false,
    "SenseIntervalSeconds": 5
},
```