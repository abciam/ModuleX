using Common.Interfaces;

namespace Common.Models;

public abstract class AsyncModule : IDisposable
{
    protected IModuleContext Context { get; private set; } = null!;
    protected IEventBus Bus { get; private set; } = null!;

    public abstract ModuleIdentity Identity { get; }
    public string Name => Identity.Name;

    public async Task<bool> Init(IModuleContext context, IEventBus bus)
    {
        Context = context;
        Bus = bus;
        return await OnInitAsync();
    }

    protected abstract Task<bool> OnInitAsync();

    public abstract void Render(IUiBuilder ui);

    public virtual void Dispose() { }
}