using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace Celeste.Mod.TracyHelper;

public class TracyHelperModule : EverestModule {
    public static TracyHelperModule Instance { get; private set; } = null!;

    public TracyHelperModule() {
        Instance = this;
#if DEBUG
        // debug builds use verbose logging
        Logger.SetLogLevel(nameof(TracyHelperModule), LogLevel.Verbose);
#else
        // release builds use info logging to reduce spam in log files
        Logger.SetLogLevel(nameof(TracyHelperModule), LogLevel.Info);
#endif
    }

    private readonly List<Hook> onHooks = [];
    private readonly List<ILHook> ilHooks = [];

    public override void Load() {
        onHooks.Add(new Hook(typeof(Player).GetMethod("Update")!, On_Player_Update));

        ilHooks.Add(new ILHook(typeof(Game).GetMethod(nameof(Game.Tick))!, IL_Game_Tick));
        //
        //ProfileMethod(typeof(Player).GetMethod("Update")!);

        // Profiler.EmitFrameMark();
        using var z = Profiler.BeginZone();

        Console.WriteLine("hi");
    }

    public override void Unload() {
        foreach (var hook in ilHooks) {
            hook.Dispose();
        }
        ilHooks.Clear();
    }

    private static void IL_Game_Tick(ILContext il) {
        var cur = new ILCursor(il) {
            Index = il.Instrs.Count - 1
        };

        cur.EmitDelegate(Profiler.EmitFrameMark);
    }

    private static void On_Player_Update(On.Celeste.Player.orig_Update orig, Player self)
    {
        using var zone = Profiler.BeginZone(null, true, ColorType.Black, null, 0, "DMD<Celeste.Player::Update>?12149405.cs", "Celeste.Player::Update");
        orig(self);
    }

    /// Inserts a profiler zone for the specified method
    private void ProfileMethod(MethodBase method) {
        ilHooks.Add(new ILHook(method, il => {
            var cur = new ILCursor(il);

            // Store zone in a local variable
            var zoneVar = new VariableDefinition(il.Import(typeof(Profiler.Zone)));
            il.Body.Variables.Add(zoneVar);

            // Begin profiler zone
            cur.EmitLdnull(); // zoneName
            cur.EmitLdcI4(1/*true*/); // active
            cur.EmitLdcI4((uint)ColorType.Black); // color
            cur.EmitLdnull(); // text
            cur.EmitLdcI4(0); // lineNumber
            cur.EmitLdstr($"{il.Method.DeclaringType}.cs"); // filePath
            cur.EmitLdstr(il.Method.Name); // memberName
            cur.EmitDelegate(Profiler.BeginZone);
            cur.EmitStloc(zoneVar);

            // End profiler zones at every return
            while (cur.TryGotoNext(MoveType.Before, instr => instr.MatchRet())) {
                cur.MoveAfterLabels();

                cur.EmitLdloc(zoneVar);
                cur.EmitCall(typeof(Profiler.Zone).GetMethod(nameof(Profiler.Zone.Dispose))!);

                cur.Index++; // Go over the Ret instruction
            }

            Console.WriteLine(il.ToString());
        }));
    }
}
