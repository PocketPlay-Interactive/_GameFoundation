using System.Collections.Generic;
using System.Linq;

public static class GameFoundationModuleRegistry
{
    private static readonly List<IGameFoundationModule> Modules = new List<IGameFoundationModule>();

    public static IReadOnlyList<IGameFoundationModule> RegisteredModules => Modules;

    public static void Register(IGameFoundationModule module)
    {
        if (module == null || Modules.Contains(module))
            return;

        Modules.Add(module);
        Modules.Sort((a, b) => a.Order.CompareTo(b.Order));
    }

    public static void Unregister(IGameFoundationModule module)
    {
        if (module == null)
            return;

        Modules.Remove(module);
    }

    public static IEnumerable<IGameFoundationModule> MergeWith(IEnumerable<IGameFoundationModule> sceneModules)
    {
        if (sceneModules == null)
            return Modules;

        return Modules
            .Concat(sceneModules.Where(module => module != null))
            .Distinct()
            .OrderBy(module => module.Order);
    }
}
