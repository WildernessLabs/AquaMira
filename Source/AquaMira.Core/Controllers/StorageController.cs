using Meadow;
using System;
using System.Collections.Generic;

namespace AquaMira.Core;

public class StorageController
{
    private readonly Dictionary<string, SensorRecord> _lastValues = new();

    public CircularBuffer<RecordBatch> Records { get; } = new CircularBuffer<RecordBatch>(50);

    public StorageController(ConfigurationController configurationController)
    {
        Records.Overrun += OnRecordBufferOverrun;
    }

    private void OnRecordBufferOverrun(object sender, EventArgs e)
    {
        Resolver.Log.Info("Telemetry storage overrrun");
    }

    public void RecordSensorValues(Dictionary<string, object> values)
    {
        var batch = new RecordBatch();

        // only record values that have changed
        foreach (var v in values)
        {
            var changed = false;

            if (_lastValues.ContainsKey(v.Key))
            {
                if (_lastValues[v.Key].LastRecordValue != v.Value)
                {
                    _lastValues[v.Key].LastRecordValue = v.Value;
                    changed = true;
                }
            }
            else
            {
                _lastValues.Add(v.Key, new SensorRecord
                {
                    SensorName = v.Key,
                    LastRecordTime = DateTimeOffset.UtcNow,
                    LastRecordValue = v.Value
                });
                changed = true;
            }

            if (changed)
            {
                batch.Values.Add(v.Key, v.Value);
            }
        }

        if (batch.Values.Count > 0)
        {
            Resolver.Log.Info($"batched {batch.Values.Count} records");
            Records.Append(batch);
        }
    }
}
