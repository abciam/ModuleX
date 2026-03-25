using Common.Enums;
using Common.Interfaces;

namespace ModuleX.Contexts;

internal class ModuleContext(Bus bus) : IModuleContext, IDisposable
{
    private readonly SemaphoreSlim _heavyLock = new(1, 1);
    private readonly SemaphoreSlim _mediumLock = new(2, 2);
    private readonly SemaphoreSlim _lightLock = new(4, 4);

    public async Task<(bool success, T? data)> Execute<T>(ETaskWeight weight, Func<Task<T?>> func)
    {
        var sem = weight switch
        {
            ETaskWeight.Heavy => _heavyLock,
            ETaskWeight.Medium => _mediumLock,
            ETaskWeight.Light => _lightLock,
            _ => _lightLock
        };
        await sem.WaitAsync();
        try { return (true, await func()); }
        catch { return (false, default); }
        finally { sem.Release(); }
    }

    public async Task<bool> Execute(ETaskWeight weight, Func<Task> func)
    {
        var res = await Execute<object?>(weight, async () => { await func(); return null; });
        return res.success;
    }
    public void Publish(string topic, object data) => bus.PublishTopic(topic, data);
    public void Subscribe(string topic, Action<object> handler) => bus.SubscribeTopic(topic, handler);
    public void Dispose() { _heavyLock.Dispose(); _mediumLock.Dispose(); _lightLock.Dispose(); }
}