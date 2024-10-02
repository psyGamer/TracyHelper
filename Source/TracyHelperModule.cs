using System;

namespace Celeste.Mod.TracyHelper;

public class TracyHelperModule : EverestModule
{
    public static TracyHelperModule Instance { get; private set; }

    public TracyHelperModule()
    {
        Instance = this;
#if DEBUG
        // debug builds use verbose logging
        Logger.SetLogLevel(nameof(TracyHelperModule), LogLevel.Verbose);
#else
        // release builds use info logging to reduce spam in log files
        Logger.SetLogLevel(nameof(TracyHelperModule), LogLevel.Info);
#endif
    }

    public override void Load()
    {
        // TODO: apply any hooks that should always be active
    }

    public override void Unload()
    {
        // TODO: unapply any hooks applied in Load()
    }
}