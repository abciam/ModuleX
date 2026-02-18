using Common.Enums;
using Common.Events;
using Common.Helpers;
using Common.Interfaces;
using Common.Models;
using ModuleX.Contexts;
using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace ModuleX.Core;

internal class ModuleManager(IModuleContext context, Bus bus) : IDisposable
{
    private ConcurrentBag<AssemblyModuleContainer> _loaded = [];

    public async Task LoadAsync()
    {
        if (!Directory.Exists(Program.ModulesPath)) return;
        var dlls = Directory.GetFiles(Program.ModulesPath, "*.dll", SearchOption.AllDirectories);

        foreach (var path in dlls)
        {
            if (!await VerifyModuleAsync(path))
            {
                bus.Publish(new LogEvent(DateTime.Now, $"Verificate fail: {Path.GetFileName(path)}", ELog.Warning));
                continue;
            }

            var loader = new IsolationContext(path);
            try
            {
                var assembly = loader.LoadFromAssemblyPath(path);
                var types = assembly.GetTypes().Where(t => typeof(AsyncModule).IsAssignableFrom(t) && !t.IsAbstract);

                var modulesInDll = new ConcurrentDictionary<ModuleIdentity, AsyncModule>();
                foreach (var type in types)
                {
                    if (Activator.CreateInstance(type) is AsyncModule mod)
                    {
                        if (await mod.Init(context, bus))
                        {
                            modulesInDll.TryAdd(mod.Identity, mod);
                        }
                    }
                }

                if (!modulesInDll.IsEmpty)
                    _loaded.Add(new AssemblyModuleContainer(loader, modulesInDll));
                else
                    loader.Unload();
            }
            catch (Exception ex)
            {
                loader.Unload();
                bus.Publish(new LogEvent(DateTime.Now, $"{ExceptionParser.Parse(ex)}", ELog.Error));
            }
        }
        bus.Publish(new LogEvent(DateTime.Now, $"{GetAll().Count()} modules Loaded", ELog.Basic));
    }
    private async Task<bool> VerifyModuleAsync(string filePath)
    {
        try
        {
            var fileName = Path.GetFileName(filePath);

            using var hasher = SHA256.Create();
            using var stream = File.OpenRead(filePath);
            var hashBytes = await hasher.ComputeHashAsync(stream);
            var hashString = Convert.ToHexStringLower(hashBytes);

            var payload = new
            {
                Module = fileName,
                Hash = hashString,
                Timestamp = DateTime.UtcNow
            };

            await Task.Delay(200);

            bus.Publish(new LogEvent(DateTime.Now, $"Verificate {fileName} [Hash: {hashString[..6]}...]", ELog.Basic));

            return true;
        }
        catch (Exception ex)
        {
            bus.Publish(new LogEvent(DateTime.Now, $"Verificate error: {ex.Message}", ELog.Error));
            return false;
        }
    }
    public async Task ReloadAllAsync()
    {
        foreach (var container in _loaded)
        {
            foreach (var mod in container.Modules.Values)
            {
                try { mod.Dispose(); } catch { }
            }
            container.Context.Unload();
        }

        _loaded = [];

        GC.Collect();
        GC.WaitForPendingFinalizers();

        await LoadAsync();
    }
    public IEnumerable<AsyncModule> GetAll() => _loaded.SelectMany(c => c.Modules.Values);
    public void Dispose()
    {
        foreach (var m in GetAll()) 
            m.Dispose();
        foreach (var c in _loaded) 
            c.Context.Unload();
    }

    private record AssemblyModuleContainer(IsolationContext Context, ConcurrentDictionary<ModuleIdentity, AsyncModule> Modules);
}