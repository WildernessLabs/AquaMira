using Meadow;
using System;
using System.Collections.Generic;

namespace Thurston_Monitor.Core;

public class SensorRecord
{
    public int SensorID { get; set; }
    public DateTimeOffset? LastRecordTime { get; set; }
    public object? LastRecordValue { get; set; }
}

public class StorageController
{
    private readonly Dictionary<int, SensorRecord> _lastValues = new();
    private CircularBuffer<
    public StorageController(ConfigurationController configurationController)
    {
    }

    public void RecordSensorValues(Dictionary<int, object> values)
    {
        // only record values that have changed
        foreach (var v in values)
        {
            var changed = false;

            if (_lastValues.ContainsKey(v.Key))
            {
                _lastValues.Add(v.Key, new SensorRecord
                {
                    SensorID = v.Key,
                    LastRecordTime = DateTimeOffset.UtcNow,
                    LastRecordValue = v.Value
                });
                changed = true;
            }
            else
            {
                if (_lastValues[v.Key].LastRecordValue != v.Value)
                {
                    _lastValues[v.Key].LastRecordValue = v.Value;
                    changed = true;
                }
            }

            if (changed)
            {
                Resolver.Log.Info($"Store: {v.Key}:{v.Value}");
            }
        }
    }
}
