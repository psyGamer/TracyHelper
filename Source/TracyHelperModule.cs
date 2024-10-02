using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
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

    internal readonly List<Hook> OnHooks = [];
    internal readonly List<ILHook> ILHooks = [];

    public override void Initialize() {
        using (new DetourConfigContext(new DetourConfig("TracyHelper", before: ["*"])).Use()) {
            InstrumentationInjector.Load();
        }
    }

    public override void Load()
    {

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
    }


}
