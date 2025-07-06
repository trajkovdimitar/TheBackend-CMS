using System.Reflection;

namespace TheBackendCmsSolution.Modules.Abstractions;

public static class ModuleLoader
{
    public static IEnumerable<ICmsModule> DiscoverModules()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();

        // Ensure module assemblies are loaded so they can be discovered even if
        // no types have been referenced yet.
        foreach (var path in Directory.EnumerateFiles(AppContext.BaseDirectory, "TheBackendCmsSolution.Modules.*.dll"))
        {
            try
            {
                var name = AssemblyName.GetAssemblyName(path);
                if (!assemblies.Any(a => a.GetName().Name == name.Name))
                {
                    assemblies.Add(Assembly.Load(name));
                }
            }
            catch
            {
                // Ignore load failures so one bad assembly does not prevent startup
            }
        }

        var moduleTypes = assemblies
            .SelectMany(a =>
            {
                try
                {
                    return a.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    return ex.Types.Where(t => t != null).Cast<Type>();
                }
                catch
                {
                    return Array.Empty<Type>();
                }
            })
            .Where(t => typeof(ICmsModule).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        return moduleTypes.Select(t => (ICmsModule)Activator.CreateInstance(t)!);
    }
}
