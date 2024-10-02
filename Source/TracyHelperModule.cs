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

    private readonly List<Hook> onHooks = [];
    private readonly List<ILHook> ilHooks = [];

    public override void Load() {
        using (new DetourConfigContext(new DetourConfig("TracyHelper", before: ["*"])).Use())
        {
            // General
            ilHooks.Add(new ILHook(typeof(Game).GetMethod(nameof(Game.Tick))!, il => {
                var cur = new ILCursor(il) {
                    Index = il.Instrs.Count - 1
                };

                // Mark frames
                cur.EmitDelegate(Profiler.EmitFrameMark);
            }));

            ProfileMethod(typeof(Game).GetMethod(nameof(Game.Tick), BindingFlags.Public | BindingFlags.Instance)!, "Tick");
            ProfileMethod(typeof(Celeste).GetMethod(nameof(Celeste.Update), BindingFlags.NonPublic | BindingFlags.Instance)!, "Update");
            ProfileMethod(typeof(Celeste).GetMethod(nameof(Celeste.Draw), BindingFlags.NonPublic | BindingFlags.Instance)!, "Draw");

            // ProfileMethod(typeof(Player).GetMethod("Update")!);
        }
    }

    public override void Unload() {
        foreach (var hook in onHooks)
        {
            hook.Dispose();
        }
        onHooks.Clear();

        foreach (var hook in ilHooks) {
            hook.Dispose();
        }
        ilHooks.Clear();
    }

    /// Inserts a profiler zone for the specified method
    private void ProfileMethod(MethodInfo method, string? zoneName = null) {
        ilHooks.Add(new ILHook(method, il => {
            var cur = new ILCursor(il);

            // Create a try-finally block to properly dispose the zone
            var exceptionHandler = new ExceptionHandler(ExceptionHandlerType.Finally);
            il.Body.ExceptionHandlers.Add(exceptionHandler);

            // Store zone in a local variable
            var zoneVar = new VariableDefinition(il.Import(typeof(Profiler.Zone)));
            il.Body.Variables.Add(zoneVar);
            // Store return value in a local variable (if needed)
            var returnVar = new VariableDefinition(il.Method.ReturnType);
            if (method.ReturnType != typeof(void)) {
                il.Body.Variables.Add(returnVar);
            }

            // Begin profiler zone
            if (zoneName == null) {
                cur.EmitLdnull(); // zoneName
            } else {
                cur.EmitLdstr(zoneName);
            }
            cur.EmitLdcI4(1/*true*/); // active
            cur.EmitLdcI4((uint)ColorType.Black); // color
            cur.EmitLdnull(); // text
            cur.EmitLdcI4(0); // lineNumber
            cur.EmitLdstr($"{method.DeclaringType!.Name}.cs"); // filePath
            cur.EmitLdstr(il.Method.Name); // memberName
            cur.EmitDelegate(Profiler.BeginZone);
            cur.EmitStloc(zoneVar);

            // Begin try-block
            exceptionHandler.TryStart = cur.Next;

            // Convert all "ret" into "leave" instructions
            var returnLabel = cur.DefineLabel();
            for (; cur.Index < il.Instrs.Count; cur.Index++) {
                if (cur.Next?.OpCode == OpCodes.Ret) {
                    if (method.ReturnType != typeof(void)) {
                        // Store return result
                        cur.EmitStloc(returnVar);
                    }

                    cur.Next.OpCode = OpCodes.Leave;
                    cur.Next.Operand = returnLabel;
                }
            }

            // End try-block
            cur.Index = il.Instrs.Count - 1;
            if (method.ReturnType == typeof(void)) {
                // Avoid dealing with retargeting labels
                cur.Next!.OpCode = OpCodes.Nop;
            } else {
                // Store return result
                cur.Next!.OpCode = OpCodes.Stloc;
                cur.Next!.Operand = returnVar;
            }

            cur.Index++;

            cur.EmitLeave(returnLabel);

            // End profiler zone
            cur.EmitLdloca(zoneVar);
            exceptionHandler.TryEnd = cur.Prev;
            exceptionHandler.HandlerStart = cur.Prev; // Begin finally-block
            cur.EmitCall(typeof(Profiler.Zone).GetMethod(nameof(Profiler.Zone.Dispose))!);

            // End finally-block
            cur.EmitEndfinally();

            if (method.ReturnType != typeof(void)) {
                // Retrieve return result
                cur.EmitLdloc(returnVar);
                exceptionHandler.HandlerEnd = cur.Prev;
                cur.EmitRet();
            } else {
                cur.EmitRet();
                exceptionHandler.HandlerEnd = cur.Prev;
            }

            returnLabel.Target = cur.Prev;

            Logger.Debug(Tag, $"Applied profiling to {method} in {method.DeclaringType}");
        }));
    }
}
