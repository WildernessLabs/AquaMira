# AquaMira

## Configuration

Generally speaking, to configure your AquaMira application to read specific sensors, you will add sections to sensor-config.json.
Since each sensor is different, the configuration section for each is specific to that sensor.  See the readme for each module in the "Supprted Sensor Modules" section below for details.

Along with the configuration, you must register each node controller you want to use in your application.

This is done in your implementation of `IAquaMiraHardware` in the `GetAvailableSensingNodeControllers` method.  See the readme for each module in the "Supported Sensor Modules" section below for details.

## Supported Sensor Modules

- [Cerus XDrive VFD](./Source/NodeControllers/AquaMira.CerusNodeController/readme.md)
- [Keller Pressure Transducer](./Source/NodeControllers/AquaMira.KellerTransducerNodeController/readme.md)
- [Temco SPM-1x Power Meter](./Source/NodeControllers/AquaMira.SPM1xPowerNodeController/readme.md)
- [Temco T3-22i IO module](./Source/NodeControllers/AquaMira.T322InputNodeController/readme.md)
- [Voltaic Battery Controller](./Source/NodeControllers/AquaMira.VoltaicBatteryNodeController/readme.md)
- [YSI EXO Water Quality Sonde](./Source/NodeControllers/AquaMira.YsiSondeNodeController/readme.md)

## Bench Testing

See the [Bench Testing Guide](Documentation/BenchTesting.md) for setting up your device for bench testing.
 