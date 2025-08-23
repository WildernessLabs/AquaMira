using System;

namespace AquaMira.Core;

public class SensorRecord
{
    public string? SensorName { get; set; }
    public DateTimeOffset? LastRecordTime { get; set; }
    public object? LastRecordValue { get; set; }
}
