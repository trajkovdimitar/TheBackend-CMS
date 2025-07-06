using System.Reflection;

namespace TheBackendCmsSolution.Modules.Abstractions;

public static class ModuleLoader
{
    public static IEnumerable<ICmsModule> DiscoverModules()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var moduleTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(ICmsModule).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
        return moduleTypes.Select(t => (ICmsModule)Activator.CreateInstance(t)!);
    }
}
