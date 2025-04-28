using System;

namespace Thurston_Monitor.Core;

public class SensorRecord
{
    public int SensorID { get; set; }
    public DateTimeOffset? LastRecordTime { get; set; }
    public object? LastRecordValue { get; set; }
}

public class StorageController
{
    public StorageController(ConfigurationController configurationController)
    {
    }

    public void RecordSensorValue(int sensorID, object? value)
    {
    }
}
