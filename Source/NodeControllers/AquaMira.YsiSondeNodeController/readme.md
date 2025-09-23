# YsiSondeNodeController

This is an AquaMira node controller specifically for the YSI EXO Water Quality Sonde. It is designed to interface with the YSI EXO Sonde and collect data from its various sensors.

## Registration

Inside your app's `IAquaMiraHardware` implementation:

```csharp
public IEnumerable<(Type ControllerType, string ConfigurationName)> GetAvailableSensingNodeControllers()
{
    return new[]
    {
        (typeof(YsiSondeNodeController), "YSISonde")
    };
}
```

## Behavior

The YsiSondeNodeController will connect to the sonde at startup and make requests to the sode for data at a period that is 1/2 of the configured `SenseIntervalSeconds` to ensure that it can publish the most recent data.  It will publish data on a period of `SenseIntervalSeconds` using the last-read values for each sensor configured.

## Configuration

Configuration Properties:

| Property Name| Type| Required|Default | Description|
|--------------|-----|---------|--------|------------|
| ModbusAddress | Integer | Yes | N/A | The Modbus address of the YSI Sonde. |
| Name | String | Yes | N/A | The name of the sensor instance. This name will be the prefix of all data items. |
| IsSimulated | Boolean | No | false | If true, the node controller will simulate data instead of reading from the actual device. |
| SenseIntervalSeconds | Integer | No | 60 | The interval in seconds at which the node controller will read data from the YSI Sonde. |
| Parameters | Array of Strings | Yes | N/A | A list of parameters to read from the YSI Sonde. See the Complete Parameter List section below for all valid parameters. A parameter of 'All' will include all supported parameters|

## Complete Parameter List

The following parameters are supported by the YSI Sonde Node Controller:

### Basic Water Quality
- `Temperature` - Water temperature
- `Conductivity` - Electrical conductivity
- `SpecificConductivity` - Temperature-compensated conductivity
- `nLFConductivity` - Non-linear function conductivity
- `Salinity` - Salinity in practical salinity units
- `pH` - pH in standard units
- `pHmV` - pH in millivolts
- `ORP` - Oxidation-reduction potential

### Dissolved Oxygen
- `DOSat` - Dissolved oxygen percent saturation
- `DOmgL` - Dissolved oxygen in mg/L
- `ODOPercentLocal` - Dissolved oxygen percent local saturation

### Total Dissolved Solids (TDS)
- `TDSmgL` - TDS in mg/L
- `TDSgL` - TDS in g/L
- `TDSkgL` - TDS in kg/L

### Total Suspended Solids (TSS)
- `TSSmgL` - TSS in mg/L
- `TSSgL` - TSS in g/L

### Pressure and Depth
- `PressurePsia` - Pressure in psia
- `PressurePsig` - Pressure in psig
- `DepthMeters` - Depth in meters
- `DepthFeet` - Depth in feet
- `VerticalPositionm` - Vertical position in meters
- `VerticalPositionft` - Vertical position in feet

### Turbidity
- `TurbidityNTU` - Turbidity in NTU
- `TurbidityFNU` - Turbidity in FNU

### Nutrients
- `NH3` - Ammonia in mg/L
- `NH4` - Ammonium in mg/L
- `NO3` - Nitrate in mg/L
- `Chloride` - Chloride in mg/L
- `Potassium` - Potassium in mg/L

### Chlorophyll and Algae
- `ChlorophyllugL` - Chlorophyll in μg/L
- `ChlorophyllRFU` - Chlorophyll in RFU
- `BGAPCugL` - Blue-green algae phycocyanin in μg/L
- `BGAPEugL` - Blue-green algae phycoerythrin in μg/L

### Fluorescence and Organic Matter
- `fDOMrfu` - Fluorescent dissolved organic matter in RFU
- `fDOMqsu` - Fluorescent dissolved organic matter in QSU
- `RhodamineugL` - Rhodamine in μg/L

### Light and Radiation
- `PARChannel1` - PAR Channel 1
- `PARChannel2` - PAR Channel 2

### System Monitoring
- `BatteryVoltage` - Battery voltage
- `ExternalPower` - External power voltage
- `WiperPosition` - Wiper position
- `WiperPeakCurrent` - Wiper peak current

### Sample Configuration Section

The root node name must match the name used (second parameter) in the registration above.

```yaml
  "YSISonde": {
    "ModbusAddress": 5,
    "Name": "Sonde",
    "IsSimulated": false,
    "SenseIntervalSeconds": 60,
    "Parameters": [
      "Temperature",
      "Conductivity",
      "Salinity",
      "pH",
      "DOSat",
      "DOmgL",
      "TurbidityNTU",
      "ChlorophyllugL",
      "BatteryVoltage"
    ]
  },

```

### Using All Parameters

To enable all supported parameters, use the special "All" parameter:

```yaml
  "YSISonde": {
    "ModbusAddress": 5,
    "Name": "Sonde",
    "IsSimulated": false,
    "SenseIntervalSeconds": 60,
    "Parameters": ["All"]
  },

```