# YsiSondeNodeController

This is an AquaMira node controller specifically for the YSI EXO Water Quality Sonde. It is designed to interface with the YSI EXO Sonde and collect data from its various sensors.

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
| Parameters | Array of Strings | Yes | N/A | A list of parameters to read from the YSI Sonde. Valid parameters include: Temperature, Chloriform, Conductivity, LFChloriform, DOSat, DOmgL, Salinity, SpCond, BGARFU, TDS, Turbidity, TSS, WiperPos, pH, pHmv, Battery, CablePower. A parameter of 'All' will include all supported parameters|

### Sample Configuration Section

```yaml
  "YSISonde": {
    "ModbusAddress": 5,
    "Name": "Sonde",
    "IsSimulated": false,
    "SenseIntervalSeconds": 60,
    "Parameters": [
      "Temperature",
      "Chloriform",
      "Conductivity",
      "LFChloriform",
      "DOSat",
      "DOmgL",
      "Salinity",
      "SpCond",
      "BGARFU",
      "TDS",
      "Turbidity",
      "TSS",
      "WiperPos",
      "pH",
      "pHmv",
      "Battery",
      "CablePower"
    ]
  },

```