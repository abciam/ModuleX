using Common.Enums;

namespace Common.Events;

public readonly record struct LogEvent(string Msg, ELog Level, DateTime Time)
{
    public LogEvent(string msg, ELog level)
        : this(msg, level, DateTime.Now)
    {
    }
}