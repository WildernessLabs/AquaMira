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
| Parameters | Array of Strings | Yes | N/A | A list of parameters to read from the YSI Sonde. See "Valid Parameters" section below. A parameter of 'All' will include all supported parameters. Unknown parameters will be logged and ignored.|

### Valid Parameters

The following parameters are supported (case-insensitive):

**Basic Water Quality:**
- `All` - Includes all supported parameters
- `Temperature` - Water temperature
- `Conductivity` - Electrical conductivity
- `SpecificConductivity` - Specific conductance
- `nLFConductivity` - Non-linear frequency conductivity
- `Salinity` - Salinity
- `pH` - pH value
- `pHmV` - pH in millivolts
- `ORP` - Oxidation-reduction potential

**Dissolved Oxygen:**
- `DOSat` - Dissolved oxygen percent saturation
- `DOmgL` - Dissolved oxygen in mg/L
- `ODOPercentLocal` - Optical dissolved oxygen percent local

**Total Dissolved/Suspended Solids:**
- `TDSmgL` - Total dissolved solids in mg/L
- `TDSgL` - Total dissolved solids in g/L
- `TDSkgL` - Total dissolved solids in kg/L
- `TSSmgL` - Total suspended solids in mg/L
- `TSSgL` - Total suspended solids in g/L

**Pressure and Depth:**
- `PressurePsia` - Pressure in psia
- `PressurePsig` - Pressure in psig
- `DepthMeters` - Depth in meters
- `DepthFeet` - Depth in feet
- `VerticalPositionm` - Vertical position in meters
- `VerticalPositionft` - Vertical position in feet

**Turbidity:**
- `TurbidityNTU` - Turbidity in NTU
- `TurbidityFNU` - Turbidity in FNU

**Nutrients:**
- `NH3` - Ammonia
- `NH4` - Ammonium
- `NO3` - Nitrate
- `Chloride` - Chloride
- `Potassium` - Potassium

**Chlorophyll and Algae:**
- `ChlorophyllugL` - Chlorophyll in µg/L
- `ChlorophyllRFU` - Chlorophyll in RFU
- `BGAPCugL` - Blue-green algae phycocyanin in µg/L
- `BGAPEugL` - Blue-green algae phycoerythrin in µg/L

**Fluorescence and Organic Matter:**
- `fDOMrfu` - Fluorescent dissolved organic matter in RFU
- `fDOMqsu` - Fluorescent dissolved organic matter in QSU
- `RhodamineugL` - Rhodamine in µg/L

**PAR (Photosynthetically Active Radiation):**
- `PARChannel1` - PAR channel 1
- `PARChannel2` - PAR channel 2

**System Monitoring:**
- `WiperPosition` - Wiper position voltage
- `WiperPeakCurrent` - Wiper peak current

### Sample Configuration Section

The root node name must match the name used (second parameter) in the registration above.

```json
  "YSISonde": {
    "ModbusAddress": 5,
    "Name": "Sonde",
    "IsSimulated": false,
    "SenseIntervalSeconds": 60,
    "Parameters": [
      "Temperature",
      "Conductivity",
      "DOSat",
      "DOmgL",
      "Salinity",
      "pH",
      "TurbidityNTU",
      "ChlorophyllugL"
    ]
  }
```