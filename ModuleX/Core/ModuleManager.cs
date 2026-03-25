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
    private ConcurrentDictionary<string, AssemblyModuleContainer> _loaded = new();

    public async Task LoadAsync()
    {
        if (!Directory.Exists(Program.ModulesPath)) return;

        var dlls = Directory.GetFiles(Program.ModulesPath, "*.dll", SearchOption.AllDirectories);

        foreach (var path in dlls)
        {
            var module = await LoadSingleModuleAsync(path);

            Program.Instance?._mainForm?.RegisterModuleButton(path, Path.GetFileNameWithoutExtension(path), module!);
        }

        bus.Publish(new LogEvent($"{_loaded.Count} modules active", ELog.Basic));
    }

    public async Task UnloadSingleModuleAsync(string path)
    {
        if (_loaded.TryRemove(path, out var container))
        {
            foreach (var mod in container.Modules.Values)
            {
                try { mod.Dispose(); } catch { }
            }
            container.Context.Unload();

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }

    public async Task<AsyncModule?> LoadSingleModuleAsync(string path)
    {
        if (_loaded.ContainsKey(path)) return _loaded[path].Modules.Values.FirstOrDefault();

        if (!await VerifyModuleAsync(path)) return null;

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
            {
                var container = new AssemblyModuleContainer(loader, modulesInDll);
                _loaded[path] = container;
                return modulesInDll.Values.First();
            }

            loader.Unload();
            return null;
        }
        catch (Exception ex)
        {
            loader.Unload();
            bus.Publish(new LogEvent($"{ExceptionParser.Parse(ex)}", ELog.Error));
            return null;
        }
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

            bus.Publish(new LogEvent($"Verificate {fileName}", ELog.Basic));
            bus.Publish(new LogEvent($"Verificate successful {hashString}", ELog.Basic));

            return true;
        }
        catch (Exception ex)
        {
            bus.Publish(new LogEvent($"Verificate error: {ex.Message}", ELog.Error));
            return false;
        }
    }
    public async Task ReloadAllAsync()
    {
        await UnloadAllAsync();

        Program.Instance!._mainForm!.Invoke(() => 
        {
            Program.Instance!._mainForm!.ButtonsPanel.Controls.Clear();
        });

        await LoadAsync();
    }
    public async Task UnloadAllAsync()
    {
        var paths = _loaded.Keys.ToList();

        foreach (var path in paths)
        {
            await UnloadSingleModuleAsync(path);
        }

        _loaded.Clear();

        GC.Collect();
        GC.WaitForPendingFinalizers();
    }
    public IEnumerable<AsyncModule> GetAll() => _loaded.Values.SelectMany(c => c.Modules.Values);
    public void Dispose()
    {
        foreach (var module in GetAll())
        {
            try
            {
                module.Dispose();
            }
            catch (Exception ex)
            {
                bus.Publish(new LogEvent($"Error disposing module {module.Identity}: {ex.Message}", ELog.Error));
            }
        }

        foreach (var container in _loaded.Values)
        {
            try
            {
                container.Context.Unload();
            }
            catch { }
        }

        _loaded.Clear();
    }

    public AsyncModule? GetModuleByPath(string path)
    {
        if (_loaded.TryGetValue(path, out var container))
        {
            return container.Modules.Values.FirstOrDefault();
        }
        return null;
    }

    private record AssemblyModuleContainer(IsolationContext Context, ConcurrentDictionary<ModuleIdentity, AsyncModule> Modules);
}