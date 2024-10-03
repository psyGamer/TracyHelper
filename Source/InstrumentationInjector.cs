using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace Celeste.Mod.TracyHelper;

/// Injects performance instrumentation into various parts of the game
public static class InstrumentationInjector {
    public static void Load() {
        // General
        TracyHelperModule.Instance.ILHooks.Add(new ILHook(typeof(Game).GetMethod(nameof(Game.Tick))!, IL_Game_Tick));

        ProfileMethod(typeof(Celeste).GetMethod(nameof(Celeste.Update), BindingFlags.NonPublic | BindingFlags.Instance)!, "Update", 0x2B7FDB);
        ProfileMethod(typeof(Celeste).GetMethod(nameof(Celeste.Draw), BindingFlags.NonPublic | BindingFlags.Instance)!, "Draw", 0x21CC58);

        On.Monocle.EntityList.Add_Entity += On_EntityList_Add;
        On.Monocle.ComponentList.Add_Component += On_ComponentList_Add;

        ProfileMethod(typeof(EntityList).GetMethod(nameof(EntityList.Update), BindingFlags.NonPublic | BindingFlags.Instance)!, "Entity Update", 0x1064C3);
        ProfileMethod(typeof(EntityList).GetMethod(nameof(EntityList.Render))!, "Entity Render", 0x1B9D45);
        ProfileMethod(typeof(EntityList).GetMethod(nameof(EntityList.RenderOnly))!, null, 0x1B9D45, null, null, NameEntityRenderOnly);
        ProfileMethod(typeof(EntityList).GetMethod(nameof(EntityList.RenderOnlyFullMatch))!, null, 0x1B9D45, null, null, NameEntityRenderOnlyFullMatch);
        ProfileMethod(typeof(EntityList).GetMethod(nameof(EntityList.RenderExcept))!, null, 0x1B9D45, null, null, NameEntityRenderExcept);

        return;

        static string NameEntityRenderOnly(EntityList _, int tags) => $"Entity Render Only ({TagsToString(tags)})";
        static string NameEntityRenderOnlyFullMatch(EntityList _, int tags) => $"Entity Render OnlyFullMatch ({TagsToString(tags)})";
        static string NameEntityRenderExcept(EntityList _, int tags) => $"Entity Render Except ({TagsToString(tags)})";

        static string TagsToString(int tag) {
            StringBuilder builder = new();
            for (int id = 0; id < BitTag.TotalTags; id++) {
                int mask = 1 << id;
                if ((tag & mask) != 0) {
                    if (builder.Length != 0) {
                        builder.Append(" | ");
                    }

                    builder.Append(BitTag.byID[id].Name);
                }
            }
            return builder.ToString();
        }
    }
    public static void Unload() {
        On.Monocle.EntityList.Add_Entity -= On_EntityList_Add;
        On.Monocle.ComponentList.Add_Component -= On_ComponentList_Add;

        profiledTypes.Clear();
    }

    private static void IL_Game_Tick(ILContext il)
    {
        var method = typeof(Game).GetMethod(nameof(Game.Tick))!;

        var zoneVar = new VariableDefinition(il.Import(typeof(Profiler.Zone)));
        il.Body.Variables.Add(zoneVar);

        var cur = new ILCursor(il);

        cur.GotoNext(instr => instr.MatchLdsfld("Microsoft.Xna.Framework.FNAPlatform", "PollEvents"));

        cur.EmitDelegate(Profiler.EmitFrameMarkStart);

        cur.EmitZoneStart(zoneVar, method, "Poll Events", 0xD24E4E);
        cur.GotoNext(MoveType.After, instr => instr.MatchCallvirt("Microsoft.Xna.Framework.FNAPlatform/PollEventsFunc", "Invoke"));
        cur.EmitZoneEnd(zoneVar);

        // End frame
        cur.Index = il.Instrs.Count - 1;

        cur.EmitDelegate(Profiler.EmitFrameMarkEnd);
    }

    private static readonly HashSet<string> profiledTypes = [];

    // Dynamically apply profiling to entities
    private static void On_EntityList_Add(On.Monocle.EntityList.orig_Add_Entity orig, EntityList self, Entity entity) {
        var entityType = entity.GetType();
        if (profiledTypes.Add(entityType.FullName!)) {
            ProfileMethod(entityType.GetMethod(nameof(Entity.Update), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, []), null, 0x659CD9, $"{entityType.FullName}::{nameof(Entity.Update)}", $"{entityType.Name}.cs");
            ProfileMethod(entityType.GetMethod(nameof(Entity.Render), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, []), null, 0x6ECC8D, $"{entityType.FullName}::{nameof(Entity.Render)}", $"{entityType.Name}.cs");
        }

        orig(self, entity);
    }

    // Dynamically apply profiling to components
    private static void On_ComponentList_Add(On.Monocle.ComponentList.orig_Add_Component orig, ComponentList self, Component component) {
        var componentType = component.GetType();
        if (profiledTypes.Add(componentType.FullName!)) {
            ProfileMethod(componentType.GetMethod(nameof(Component.Update), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, []), null, 0x90ACCC, $"{componentType.FullName}::{nameof(Component.Update)}", $"{componentType.Name}.cs");
            ProfileMethod(componentType.GetMethod(nameof(Component.Render), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, []), null, 0xA2ECBA, $"{componentType.FullName}::{nameof(Component.Render)}", $"{componentType.Name}.cs");
        }

        orig(self, component);
    }

    /// Inserts a profiler zone for the specified method
    private static void ProfileMethod(MethodInfo? method, string? zoneName = null, uint color = 0, string? memberName = null, string? fileName = null, Delegate? customZoneName = null) {
        if (method == null) {
            return;
        }

        TracyHelperModule.Instance.ILHooks.Add(new ILHook(method, il => {
            var cur = new ILCursor(il);

            // Abort if this method already is being profiled
            var zoneType = il.Import(typeof(Profiler.Zone));
            if (il.Body.Variables.Any(var => var.VariableType.FullName == zoneType.FullName)) {
                Logger.Verbose(TracyHelperModule.Tag, $"Skipping duplicate for {method} in {method.DeclaringType}");
                return;
            }

            // Create a try-finally block to properly dispose the zone
            var exceptionHandler = new ExceptionHandler(ExceptionHandlerType.Finally);
            il.Body.ExceptionHandlers.Add(exceptionHandler);

            // Store zone in a local variable
            var zoneVar = new VariableDefinition(zoneType);
            il.Body.Variables.Add(zoneVar);
            // Store return value in a local variable (if needed)
            var returnVar = new VariableDefinition(il.Method.ReturnType);
            if (method.ReturnType != typeof(void)) {
                il.Body.Variables.Add(returnVar);
            }

            // Begin profiler zone
            cur.EmitZoneStart(zoneVar, method, zoneName, color, memberName, fileName, customZoneName);

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

            Logger.Debug(TracyHelperModule.Tag, $"Applied profiling to {method} in {method.DeclaringType}");
        }));
    }

    private static void EmitZoneStart(this ILCursor cur, VariableReference zoneVariable, MethodInfo method, string? zoneName = null, uint color = 0, string? memberName = null, string? fileName = null, Delegate? customZoneName = null)
    {
        if (customZoneName != null) {
            int paramCount = method.GetParameters().Length + (method.IsStatic ? 0 : 1);
            for (int i = 0; i < paramCount; i++) {
                cur.EmitLdarg(i);
            }
            cur.EmitDelegate(customZoneName);
        } else if (zoneName == null) {
            cur.EmitLdnull(); // zoneName
        } else {
            cur.EmitLdstr(zoneName);
        }
        cur.EmitLdcI4(1/*true*/); // active
        cur.EmitLdcI4(color);
        cur.EmitLdnull(); // text
        cur.EmitLdcI4(0); // lineNumber
        cur.EmitLdstr(fileName ?? $"{method.DeclaringType!.Name}.cs"); // filePath
        cur.EmitLdstr(memberName ?? $"{method.DeclaringType!.FullName}::{method.Name}"); // memberName
        cur.EmitDelegate(Profiler.BeginZone);
        cur.EmitStloc(zoneVariable);
    }
    private static void EmitZoneEnd(this ILCursor cur, VariableReference zoneVariable)
    {
        cur.EmitLdloca(zoneVariable);
        cur.EmitCall(typeof(Profiler.Zone).GetMethod(nameof(Profiler.Zone.Dispose))!);
    }
}
