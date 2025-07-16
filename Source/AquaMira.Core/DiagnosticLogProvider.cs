using Meadow.Logging;
using System;

namespace AquaMira.Core;

public class DiagnosticLogProvider : ILogProvider
{
    public event EventHandler<string>? DiagnosticMessageReceived;

    public void Log(LogLevel level, string message, string? messageGroup)
    {
        message = message.Substring(message.LastIndexOf(']') + 1);

        switch (level)
        {
            case LogLevel.Error:
            case LogLevel.Warning:
                DiagnosticMessageReceived?.Invoke(this, message);
                break;
            default:
                if (messageGroup != null && messageGroup.Contains("AquaMira"))
                {
                    DiagnosticMessageReceived?.Invoke(this, message);
                }
                break;
        }
    }
}
