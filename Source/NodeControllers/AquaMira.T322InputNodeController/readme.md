# Temco T3-22i IO module

## Registration

Inside your app's `IAquaMiraHardware` implementation:

```csharp
public IEnumerable<(Type ControllerType, string ConfigurationName)> GetAvailableSensingNodeControllers()
{
    return new[]
    {
        (typeof(T322InputNodeController), "T322iInputs"),
    };
}
```

## Sample Configuration

The root node name must match the name used (second parameter) in the registration above.

```json
  "T322iInputs": {
    "ModbusAddress": 254,
    "IsSimulated": false,
    "Channels": [
      {
        "ChannelNumber": 10,
        "ChannelType": "Current_4_20",
        "Scale": 1,
        "Offset": 0,
        "UnitType": "Temperature",
        "Name": "Housing Temp",
        "SenseIntervalSeconds": 30
      },
      {
        "ChannelNumber": 0,
        "ChannelType": "Current_4_20",
        "Scale": 1,
        "Offset": 0,
        "UnitType": "Pressure",
        "Name": "Pressure 1",
        "SenseIntervalSeconds": 30
      }
    ]
  }
```