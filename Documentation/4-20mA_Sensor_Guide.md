# Guide: Using 4-20mA and 0-20mA Sensors in AquaMira

This document describes how to configure analog current sensors (such as 4-20mA and 0-20mA loop sensors) for use with the AquaMira platform, including mathematical scaling, configuration file syntax, and Grafana visualization setup.

---

## 1. Mathematical Scaling: Scale and Offset

Analog current sensors output a current ($x$ in milliamperes, mA) that has a linear relationship with the physical engineering value ($y$). This is represented by the slope-intercept equation:

$$y = \text{Scale} \cdot x + \text{Offset}$$

To calculate the `Scale` and `Offset` for any sensor, follow these steps.

### General Heuristic

1. **Identify Sensor Ranges**:
   - $I_{\min}, I_{\max}$: The sensor's output current range in mA (e.g., $4\text{ mA}$ and $20\text{ mA}$).
   - $V_{\min}, V_{\max}$: The sensor's physical measuring range in native units (e.g., $0^\circ\text{F}$ to $100^\circ\text{F}$, or $0\text{ psi}$ to $150\text{ psi}$).

2. **Convert to Meadow Canonical Units**:
   Meadow processes sensor data in SI units internally. You **must** convert your physical range values ($V_{\min}, V_{\max}$) to Meadow's canonical units for that sensor type:
   - **Temperature**: Celsius ($^\circ\text{C}$)
     - Formula: $T(^\circ\text{C}) = (T(^\circ\text{F}) - 32) \cdot \frac{5}{9}$
   - **Pressure**: Bar ($\text{bar}$)
     - Formula: $P(\text{bar}) = P(\text{psi}) \cdot 0.0689476$
   - **Relative Humidity**: Percent ($\%$) (e.g., $0$ to $100$)
   - **Volumetric Flow**: Cubic Meters Per Second ($\text{m}^3/\text{s}$)
     - Formula: $\text{GPM} \cdot 6.30902 \cdot 10^{-5}$

3. **Compute Scale and Offset**:
   - $\text{Scale} = \frac{V_{\max\_canonical} - V_{\min\_canonical}}{I_{\max} - I_{\min}}$
   - $\text{Offset} = V_{\min\_canonical} - (\text{Scale} \cdot I_{\min})$

---

### Step-by-Step Examples

#### Example A: 0 to 200°C Temperature Sensor (4-20mA mode)
1. **Inputs**:
   - Current: $I_{\min} = 4\text{ mA}$, $I_{\max} = 20\text{ mA}$
   - Range: $V_{\min} = 0^\circ\text{C}$, $V_{\max} = 200^\circ\text{C}$
2. **Canonical Conversion**: Already in Celsius ($0^\circ\text{C}$ and $200^\circ\text{C}$).
3. **Calculations**:
   - $\text{Scale} = \frac{200 - 0}{20 - 4} = \frac{200}{16} = 12.5$
   - $\text{Offset} = 0 - (12.5 \cdot 4) = -50.0$
4. **Formula**: $y = 12.5 \cdot x - 50$

#### Example B: 0 to 150 psi Pressure Sensor (0-20mA mode)
1. **Inputs**:
   - Current: $I_{\min} = 0\text{ mA}$, $I_{\max} = 20\text{ mA}$
   - Range: $V_{\min} = 0\text{ psi}$, $V_{\max} = 150\text{ psi}$
2. **Canonical Conversion** (psi to Bar):
   - $V_{\min\_canonical} = 0 \cdot 0.0689476 = 0\text{ bar}$
   - $V_{\max\_canonical} = 150 \cdot 0.0689476 = 10.34214\text{ bar}$
3. **Calculations**:
   - $\text{Scale} = \frac{10.34214 - 0}{20 - 0} = 0.517107$
   - $\text{Offset} = 0 - (0.517107 \cdot 0) = 0.0$
4. **Formula**: $y = 0.517107 \cdot x$

---

## 2. Using Scale and Offset in the AquaMira Sensor Config

In the AquaMira `sensor-config.json` file, add channels to the `ConfigurableAnalogs` module using the calculated parameters:

```json
{
  "ConfigurableAnalogs": {
    "IsSimulated": false,
    "Channels": [
      {
        "ChannelNumber": 0,
        "ChannelType": "Current_4_20",
        "Scale": 12.5,
        "Offset": -50.0,
        "UnitType": "Temperature",
        "Name": "0-200C Temperature Sensor",
        "SenseIntervalSeconds": 60
      },
      {
        "ChannelNumber": 1,
        "ChannelType": "Current_0_20",
        "Scale": 0.517107,
        "Offset": 0.0,
        "UnitType": "Pressure",
        "Name": "0-150psi Pressure Sensor",
        "SenseIntervalSeconds": 60
      }
    ]
  }
}
```

- **ChannelType**: Use `Current_4_20` for 4-20mA loops or `Current_0_20` for 0-20mA loops.
- **UnitType**: Matches the physical class name in Meadow (e.g., `Temperature` or `Pressure`).

---

## 3. Grafana Scaling (For Raw current sent to cloud)

If the device is configured to send raw milliamperes ($4\text{ mA}$ to $20\text{ mA}$) to the database, you must perform the scaling computation within Grafana using a query or expression. 

Unlike the device configuration, **Grafana does not require conversion to canonical SI units**. You can compute the scale and offset directly for your desired display unit (e.g., Fahrenheit or psi).

### Examples:

#### A. Graphing Temperature in Fahrenheit (0-200°C Sensor in 4-20mA mode)
1. Convert the range to Fahrenheit:
   - $V_{\min} = 0^\circ\text{C} = 32^\circ\text{F}$
   - $V_{\max} = 200^\circ\text{C} = 392^\circ\text{F}$
2. Compute Scale and Offset:
   - $\text{Scale} = \frac{392 - 32}{20 - 4} = 22.5$
   - $\text{Offset} = 32 - (22.5 \cdot 4) = -58.0$
3. **Grafana Math / Queries**:
   - **PromQL (Prometheus)**:
     `($A * 22.5) - 58`
   - **InfluxQL (InfluxDB)**:
     `SELECT (value * 22.5) - 58 FROM "raw_current" WHERE ...`
   - **Flux (InfluxDB v2)**:
     `|> map(fn: (r) => ({ r with _value: r._value * 22.5 - 58.0 }))`
   - **SQL (PostgreSQL / TimescaleDB)**:
     `SELECT (value * 22.5) - 58.0 AS temp_f FROM ...`

#### B. Graphing Pressure in psi (0-150 psi Sensor in 0-20mA mode)
1. Use native units ($0\text{ psi}$ and $150\text{ psi}$):
   - $\text{Scale} = \frac{150 - 0}{20 - 0} = 7.5$
   - $\text{Offset} = 0 - (7.5 \cdot 0) = 0.0$
2. **Grafana Math / Queries**:
   - **PromQL**:
     `$A * 7.5`
   - **InfluxQL**:
     `SELECT value * 7.5 FROM "raw_current" WHERE ...`
   - **SQL**:
     `SELECT value * 7.5 AS pressure_psi FROM ...`

---

## 4. ADC Quantization and Unit Resolution

The analog inputs on Meadow hardware use a **12-bit Analog-to-Digital Converter (ADC)**. This means the analog input voltage is digitized into $2^{12} = 4096$ discrete quantized states (from `0` to `4095`). 

Because of this quantization, sensor readings do not change smoothly/continuously. Instead, they jump in stepped increments. It is helpful to calculate these step sizes (resolution per bit) to understand the quantization of the data.

### Example: 0-100°C Temperature Sensor (4-20mA mode)

Assume the Meadow board's hardware maps the current range of $0$ to $20\text{ mA}$ to the full $0$ to $3.3\text{ V}$ range of the 12-bit ADC.

1. **Current Resolution per ADC step (bit)**:
   $$\text{Resolution}_{\text{current}} = \frac{20\text{ mA}}{4096\text{ steps}} \approx 0.00488\text{ mA/bit} \text{ (or } 4.88\text{ }\mu\text{A/bit)}$$

2. **Temperature Span per Milliamp**:
   For a $0$ to $100^\circ\text{C}$ sensor spanning $4$ to $20\text{ mA}$ ($16\text{ mA}$ span):
   $$\text{Scale} = \frac{100^\circ\text{C} - 0^\circ\text{C}}{20\text{ mA} - 4\text{ mA}} = 6.25^\circ\text{C/mA}$$

3. **Temperature Resolution per ADC step (bit)**:
   Combine the current resolution and temperature scale to find the minimum physical increment the hardware can detect:
   $$\text{Resolution}_{\text{temperature}} = \text{Scale} \cdot \text{Resolution}_{\text{current}} = 6.25^\circ\text{C/mA} \cdot 0.00488\text{ mA/bit} \approx 0.0305^\circ\text{C/bit}$$

### Impact on Visualization
- The system cannot detect a temperature change smaller than **$0.0305^\circ\text{C}$**.
- When graphing this sensor at high zoom levels in Grafana, the line will look "stepped" (e.g., jumping from $25.00^\circ\text{C}$ to $25.03^\circ\text{C}$ to $25.06^\circ\text{C}$) rather than showing a perfectly smooth curve. This is normal behavior representing the hardware's physical resolution limit.
