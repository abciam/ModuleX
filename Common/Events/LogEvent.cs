using Common.Enums;

namespace Common.Events;

public readonly record struct LogEvent(DateTime Time, string Msg, ELog Level);