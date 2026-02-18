using Common.Models;

namespace Common.Events;

public readonly record struct ProgressEvent(ModuleIdentity Id, int Value, string Text);