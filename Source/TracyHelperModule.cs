using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using MonoMod;
using MonoMod.InlineRT;
using MonoMod.RuntimeDetour;

namespace Celeste.Mod.TracyHelper;

public class TracyHelperModule : EverestModule {
    public const string Tag = "TracyHelper";

    public static TracyHelperModule Instance { get; private set; } = null!;

    public TracyHelperModule() {
        Instance = this;
#if DEBUG
        // debug builds use verbose logging
        Logger.SetLogLevel(Tag, LogLevel.Verbose);
#else
        // release builds use info logging to reduce spam in log files
        Logger.SetLogLevel(Tag, LogLevel.Info);
#endif
    }

    /// API for other mods to register their MonoModRules for Tracy integration
    public void MonoModRules_API()
    {
        var asmName = GetType().Assembly.GetName();
        var asmDef = Instance.Metadata.AssemblyContext.Resolve(new AssemblyNameReference(asmName.Name, asmName.Version));

        // Register dependency
        if (!MonoModRule.Modder.DependencyMap.TryGetValue(MonoModRule.Modder.Module, out var dependencies)) {
            MonoModRule.Modder.DependencyMap[MonoModRule.Modder.Module] = dependencies = new List<ModuleDefinition>();
        }
        dependencies.Add(asmDef.MainModule);

        // Register a handler for MonoMod.ProfileMethod
        var ruleMethod = typeof(ModInterop).GetMethod(nameof(ModInterop.ProfileMethod), BindingFlags.Public | BindingFlags.Static)!;
        var modder = MonoModRule.Modder; // Needs to be captured, since it'll no longer be available when the patch runs
        foreach (var type in MonoModRule.Modder.Module.Types.Where(type => type.FullName == "MonoMod.ProfileMethod")) {
            MonoModRule.Modder.CustomMethodAttributeHandlers[type.FullName] = (self, args) => ruleMethod.Invoke(self, [..args, modder]);
        }
    }

    internal readonly List<Hook> OnHooks = [];
    internal readonly List<ILHook> ILHooks = [];

    public override void Initialize() {
        using (new DetourConfigContext(new DetourConfig("TracyHelper", before: ["*"])).Use()) {
            InstrumentationInjector.Load();
        }
    }

    public override void Load() {
        Profiler.SetProgramName("Celeste");
    }

    public override void Unload() {
        foreach (var hook in OnHooks)
        {
            hook.Dispose();
        }
        OnHooks.Clear();

        foreach (var hook in ILHooks) {
            hook.Dispose();
        }
        ILHooks.Clear();

        InstrumentationInjector.Unload();
    }
}
