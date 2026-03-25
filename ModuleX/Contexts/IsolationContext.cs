using System.Reflection;
using System.Runtime.Loader;

namespace ModuleX.Contexts;

internal class IsolationContext(string path) : AssemblyLoadContext(isCollectible: true)
{
    private readonly AssemblyDependencyResolver _res = new(path);
    protected override Assembly? Load(AssemblyName name)
    {
        var p = _res.ResolveAssemblyToPath(name);
        return p != null ? LoadFromAssemblyPath(p) : null;
    }
}