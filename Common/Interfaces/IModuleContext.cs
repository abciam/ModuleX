using Common.Enums;

namespace Common.Interfaces;

public interface IModuleContext
{
    Task<(bool success, T? data)> Execute<T>(ETaskWeight weight, Func<Task<T?>> func);
    Task<bool> Execute(ETaskWeight weight, Func<Task> func);
    void Publish(string topic, object data);
    void Subscribe(string topic, Action<object> handler);
}