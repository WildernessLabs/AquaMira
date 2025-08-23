using System;
using System.Collections.Generic;

namespace AquaMira.Core;

public class RecordBatch
{
    public DateTimeOffset BatchTime { get; set; } = DateTimeOffset.UtcNow;
    public Dictionary<string, object> Values { get; set; } = new();
}
