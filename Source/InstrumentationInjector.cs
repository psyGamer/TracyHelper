using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading;
using Celeste.Mod.Helpers;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace Celeste.Mod.TracyHelper;

/// Injects performance instrumentation into various parts of the game
public static class InstrumentationInjector {
    public static void Load() {
        // General
        TracyHelperModule.Instance.ILHooks.Add(new ILHook(typeof(Game).GetMethod(nameof(Game.Tick))!, IL_Game_Tick));

        ProfileMethod(typeof(Celeste).GetMethod(nameof(Celeste.Update), BindingFlags.NonPublic | BindingFlags.Instance)!, new ProfileConfig { ZoneName = "Update", Color = 0x2B7FDB });
        ProfileMethod(typeof(Celeste).GetMethod(nameof(Celeste.Draw), BindingFlags.NonPublic | BindingFlags.Instance)!, new ProfileConfig { ZoneName = "Draw", Color = 0x21CC58 });

        // Scenes
        foreach (var sceneType in FakeAssembly.GetFakeEntryAssembly()
                     .GetTypes()
                     .Where(type => typeof(Scene).IsAssignableFrom(type)))
        {
            ProfileMethod(sceneType.GetMethod(nameof(Scene.BeforeUpdate), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)!, new ProfileConfig { Color = 0x2B7FDB });
            ProfileMethod(sceneType.GetMethod(nameof(Scene.Update), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)!, new ProfileConfig { Color = 0x2B7FDB });
            ProfileMethod(sceneType.GetMethod(nameof(Scene.AfterUpdate), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)!, new ProfileConfig { Color = 0x2B7FDB });

            ProfileMethod(sceneType.GetMethod(nameof(Scene.BeforeRender), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)!, new ProfileConfig { Color = 0x21CC58 });
            ProfileMethod(sceneType.GetMethod(nameof(Scene.Render), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)!, new ProfileConfig { Color = 0x21CC58 });
            ProfileMethod(sceneType.GetMethod(nameof(Scene.AfterRender), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)!, new ProfileConfig { Color = 0x21CC58 });

            ProfileMethod(sceneType.GetMethod(nameof(Scene.End), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)!, new ProfileConfig { Color = 0x21CC58 });
            ProfileMethod(sceneType.GetMethod(nameof(Scene.Begin), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)!, new ProfileConfig { Color = 0x21CC58 });
        }

        // Misc
        ProfileMethod(typeof(MInput).GetMethod(nameof(MInput.Update), BindingFlags.NonPublic | BindingFlags.Static)!, new ProfileConfig { Color = 0xC741D3 });
        ProfileMethod(typeof(Audio).GetMethod(nameof(Audio.Update), BindingFlags.Public | BindingFlags.Static)!, new ProfileConfig { Color = 0xC741D3 });
        ProfileMethod(typeof(AutoSplitterInfo).GetMethod(nameof(AutoSplitterInfo.Update), BindingFlags.Public | BindingFlags.Instance)!, new ProfileConfig { Color = 0xC741D3 });
        ProfileMethod(typeof(Monocle.Commands).GetMethod(nameof(Monocle.Commands.UpdateOpen), BindingFlags.NonPublic | BindingFlags.Instance)!, new ProfileConfig { Color = 0xC741D3 });
        ProfileMethod(typeof(Monocle.Commands).GetMethod(nameof(Monocle.Commands.UpdateClosed), BindingFlags.NonPublic | BindingFlags.Instance)!, new ProfileConfig { Color = 0xC741D3 });
        ProfileMethod(typeof(Monocle.Commands).GetMethod(nameof(Monocle.Commands.Render), BindingFlags.NonPublic | BindingFlags.Instance)!, new ProfileConfig { Color = 0xC741D3 });

        // Threading
        IL.Celeste.RunThread.RunThreadWithLogging += IL_RunThreadWithLogging;

        // Gameplay
        On.Monocle.EntityList.Add_Entity += On_EntityList_Add;
        On.Monocle.ComponentList.Add_Component += On_ComponentList_Add;
        On.Monocle.RendererList.Add += On_RendererList_Add;

        ProfileMethod(typeof(EntityList).GetMethod(nameof(EntityList.Update), BindingFlags.NonPublic | BindingFlags.Instance)!, new ProfileConfig { ZoneName = "Entities Update", Color = 0x1064C3 });
        ProfileMethod(typeof(EntityList).GetMethod(nameof(EntityList.Render))!, new ProfileConfig { ZoneName = "Entities Render", Color = 0x1B9D45 });
        ProfileMethod(typeof(EntityList).GetMethod(nameof(EntityList.RenderOnly))!, new ProfileConfig { CustomZoneNameDelegate = NameEntityRenderOnly, Color = 0x1B9D45 });
        ProfileMethod(typeof(EntityList).GetMethod(nameof(EntityList.RenderOnlyFullMatch))!, new ProfileConfig { CustomZoneNameDelegate = NameEntityRenderOnlyFullMatch, Color = 0x1B9D45 });
        ProfileMethod(typeof(EntityList).GetMethod(nameof(EntityList.RenderExcept))!, new ProfileConfig { CustomZoneNameDelegate = NameEntityRenderExcept, Color = 0x1B9D45 });

        ProfileMethod(typeof(Renderer).GetMethod(nameof(Renderer.Update))!, new ProfileConfig { ZoneName = "Renderers Update", Color = 0x1064C3 });
        ProfileMethod(typeof(Renderer).GetMethod(nameof(Renderer.BeforeRender))!, new ProfileConfig { ZoneName = "Renderers BeforeRender", Color = 0x1B9D45 });
        ProfileMethod(typeof(Renderer).GetMethod(nameof(Renderer.Render))!, new ProfileConfig { ZoneName = "Renderers Render", Color = 0x1B9D45 });
        ProfileMethod(typeof(Renderer).GetMethod(nameof(Renderer.AfterRender))!, new ProfileConfig { ZoneName = "Renderers AfterRender", Color = 0x1B9D45 });

        // Loading
        IL.Celeste.LevelLoader.LoadingThread += IL_LoadingThread;
        ProfileMethod(typeof(SaveData).GetMethod(nameof(SaveData.StartSession))!, new ProfileConfig { Color = 0x8856D2 });
        ProfileMethod(typeof(ParticleSystem).GetConstructor([typeof(int), typeof(int)])!, new ProfileConfig { Color = 0x8856D2 });

        ProfileMethod(typeof(Engine).GetMethod(nameof(Engine.OnSceneTransition), BindingFlags.NonPublic | BindingFlags.Instance)!, new ProfileConfig { ZoneName = "Scene Transition GC", Color = 0xD68003 });

        return;

        static string NameEntityRenderOnly(EntityList _, int tags) => $"Entities RenderOnly ({TagsToString(tags)})";
        static string NameEntityRenderOnlyFullMatch(EntityList _, int tags) => $"Entities RenderOnlyFullMatch ({TagsToString(tags)})";
        static string NameEntityRenderExcept(EntityList _, int tags) => $"Entities RenderExcept ({TagsToString(tags)})";

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
        On.Monocle.RendererList.Add -= On_RendererList_Add;

        IL.Celeste.RunThread.RunThreadWithLogging -= IL_RunThreadWithLogging;

        IL.Celeste.LevelLoader.LoadingThread -= IL_LoadingThread;

        profiledTypes.Clear();
    }

    private static void IL_Game_Tick(ILContext il) {
        var zoneVar = new VariableDefinition(il.Import(typeof(Profiler.Zone)));
        il.Body.Variables.Add(zoneVar);

        var cur = new ILCursor(il);

        cur.GotoNext(instr => instr.MatchLdsfld("Microsoft.Xna.Framework.FNAPlatform", "PollEvents"));

        cur.EmitDelegate(Profiler.EmitFrameMarkStart);

        cur.EmitZoneStart(zoneVar, isStatic: true, isVirtual: false, new ProfileConfig { ZoneName = "Poll Events", Color = 0xD24E4E });
        cur.GotoNext(MoveType.After, instr => instr.MatchCallvirt("Microsoft.Xna.Framework.FNAPlatform/PollEventsFunc", "Invoke"));
        cur.EmitZoneEnd(zoneVar);

        // End frame
        cur.Index = il.Instrs.Count - 1;

        cur.EmitDelegate(Profiler.EmitFrameMarkEnd);
    }

    private static void IL_RunThreadWithLogging(ILContext il)
    {
        var zoneVar = new VariableDefinition(il.Import(typeof(Profiler.Zone)));
        il.Body.Variables.Add(zoneVar);

        var cur = new ILCursor(il);

        // Go to start of try-block
        cur.GotoNext(instr => instr.MatchLdarg0());
        cur.EmitZoneStart(zoneVar, isStatic: true, isVirtual: false,  new ProfileConfig { Color = 0xC741D3, CustomZoneNameIL = nameCur => {
            nameCur.EmitLdstr("Thread ");
            nameCur.EmitCall(typeof(Thread).GetProperty(nameof(Thread.CurrentThread))!.GetGetMethod()!);
            nameCur.EmitCall(typeof(Thread).GetProperty(nameof(Thread.Name))!.GetGetMethod()!);
            nameCur.EmitCall(typeof(string).GetMethod(nameof(string.Concat), [typeof(string), typeof(string)])!);
        }});

        // Go the end of finally-block
        cur.Index = il.Instrs.Count - 1;
        cur.GotoPrev(instr => instr.MatchEndfinally());
        cur.MoveBeforeLabels();
        cur.EmitZoneEnd(zoneVar);
    }

    // Dynamically apply profiling to certain types
    private static readonly HashSet<string> profiledTypes = [];

    private static void On_EntityList_Add(On.Monocle.EntityList.orig_Add_Entity orig, EntityList self, Entity entity) {
        using (var zone = Profiler.BeginZone(color: 0x64E8D2)) {
            var entityType = entity.GetType();
            zone.Name = $"Apply Entity profiling: {entityType}";
            if (profiledTypes.Add(entityType.FullName!)) {
                ProfileMethod(entityType.GetMethod(nameof(Entity.Update), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, []), new ProfileConfig { Color = 0x659CD9 });
                ProfileMethod(entityType.GetMethod(nameof(Entity.Render), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, []), new ProfileConfig { Color = 0x6ECC8D });

                ProfileMethod(entityType.GetMethod(nameof(Entity.SceneBegin), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, [typeof(Scene)]), new ProfileConfig { Color = 0x6ECC8D });
                ProfileMethod(entityType.GetMethod(nameof(Entity.SceneEnd), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, [typeof(Scene)]), new ProfileConfig { Color = 0x6ECC8D });
            }
        }

        orig(self, entity);
    }
    private static void On_ComponentList_Add(On.Monocle.ComponentList.orig_Add_Component orig, ComponentList self, Component component) {
        using (var zone = Profiler.BeginZone(color: 0x64E8D2)) {
            var componentType = component.GetType();
            zone.Name = $"Apply Component profiling: {componentType}";
            if (profiledTypes.Add(componentType.FullName!)) {
                ProfileMethod(componentType.GetMethod(nameof(Component.Update), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, []), new ProfileConfig { Color = 0x90ACCC });
                ProfileMethod(componentType.GetMethod(nameof(Component.Render), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, []), new ProfileConfig { Color = 0xA2ECBA });

                ProfileMethod(componentType.GetMethod(nameof(Component.SceneEnd), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, [typeof(Scene)]), new ProfileConfig { Color = 0x6ECC8D });
            }
        }

        orig(self, component);
    }
    private static void On_RendererList_Add(On.Monocle.RendererList.orig_Add orig, RendererList self, Renderer renderer) {
        using (var zone = Profiler.BeginZone(color: 0x64E8D2)) {
            var rendererType = renderer.GetType();
            zone.Name = $"Apply Renderer profiling: {rendererType}";
            if (profiledTypes.Add(rendererType.FullName!)) {
                ProfileMethod(rendererType.GetMethod(nameof(Renderer.Update), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, [typeof(Scene)]), new ProfileConfig { Color = 0x416996 });

                ProfileMethod(rendererType.GetMethod(nameof(Renderer.BeforeRender), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, [typeof(Scene)]), new ProfileConfig { Color = 0x499B63 });
                ProfileMethod(rendererType.GetMethod(nameof(Renderer.Render), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, [typeof(Scene)]), new ProfileConfig { Color = 0x499B63 });
                ProfileMethod(rendererType.GetMethod(nameof(Renderer.AfterRender), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, [typeof(Scene)]), new ProfileConfig { Color = 0x499B63 });
            }
        }

        orig(self, renderer);
    }

    private static void IL_LoadingThread(ILContext il) {
        var zoneVar = new VariableDefinition(il.Import(typeof(Profiler.Zone)));
        il.Body.Variables.Add(zoneVar);

        var cur = new ILCursor(il);

        cur.EmitZoneStart(zoneVar, isStatic: false, isVirtual: false, new ProfileConfig { ZoneName = "Setup Renderers", Color = 0x7A36DF });
        cur.GotoNext(MoveType.After, instr => instr.MatchCallvirt<RendererList>(nameof(RendererList.UpdateLists)));
        cur.EmitZoneEnd(zoneVar);

        cur.EmitZoneStart(zoneVar, isStatic: false, isVirtual: false, new ProfileConfig { ZoneName = "Setup systems", Color = 0x7A36DF });
        cur.GotoNext(instr => instr.MatchLdsfld("Celeste.GFX", nameof(GFX.FGAutotiler)));
        cur.EmitZoneEnd(zoneVar);

        cur.EmitZoneStart(zoneVar, isStatic: false, isVirtual: false, new ProfileConfig { ZoneName = "Setup tiles", Color = 0x7A36DF });
        cur.Index = il.Instrs.Count - 1;
        cur.EmitZoneEnd(zoneVar);
    }

    private struct ProfileConfig() {
        public string? ZoneName = null;
        public uint Color = 0x000000;

        public string? MemberNameOverride = null;
        public string? FileNameOverride = null;
        public int LineOverride = 0;

        public Delegate? CustomZoneNameDelegate = null;
        public Action<ILCursor>? CustomZoneNameIL = null;
    }

    /// Inserts a profiler zone for the specified method
    private static void ProfileMethod(MethodBase? method, ProfileConfig config) {
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

            if (ResolveDebugInformation(method) is { } debugInfo) {
                Logger.Verbose(TracyHelperModule.Tag, $"Resolved debug information for {method}: {debugInfo.FilePath} line {debugInfo.Line}");
                config.FileNameOverride = debugInfo.FilePath;
                config.LineOverride = debugInfo.Line;
            }

            // Create a try-finally block to properly dispose the zone
            var exceptionHandler = new ExceptionHandler(ExceptionHandlerType.Finally);
            il.Body.ExceptionHandlers.Add(exceptionHandler);

            // Store zone in a local variable
            var zoneVar = new VariableDefinition(zoneType);
            il.Body.Variables.Add(zoneVar);

            // Store return value in a local variable (if needed)
            var returnVar = new VariableDefinition(il.Method.ReturnType);
            bool nonVoidReturnType = il.Method.ReturnType != il.Import(typeof(void));

            if (nonVoidReturnType) {
                il.Body.Variables.Add(returnVar);
            }

            // Begin profiler zone
            cur.EmitZoneStart(zoneVar, isStatic: method.IsStatic, isVirtual: method.IsVirtual, config);

            // Begin try-block
            exceptionHandler.TryStart = cur.Next;

            // Convert all "ret" into "leave" instructions
            var returnLabel = cur.DefineLabel();
            for (; cur.Index < il.Instrs.Count; cur.Index++) {
                if (cur.Next?.OpCode == OpCodes.Ret) {
                    if (nonVoidReturnType) {
                        // Store return result
                        cur.EmitStloc(returnVar);
                    }

                    cur.Next.OpCode = OpCodes.Leave;
                    cur.Next.Operand = returnLabel;
                }
            }

            // End try-block
            cur.Index = il.Instrs.Count - 1;
            if (nonVoidReturnType) {
                // Store return result
                cur.Next!.OpCode = OpCodes.Stloc;
                cur.Next!.Operand = returnVar;
            } else {
                // Avoid dealing with retargeting labels
                cur.Next!.OpCode = OpCodes.Nop;
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

            if (nonVoidReturnType) {
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

    private static (string FilePath, int Line)? ResolveDebugInformation(MethodBase method) {
        var asm = method.DeclaringType!.Assembly;
        var asmName = method.DeclaringType!.Assembly.GetName();

        // Find assembly path
        string asmPath;
        if (AssemblyLoadContext.GetLoadContext(asm) is EverestModuleAssemblyContext asmCtx) {
            asmPath = Everest.Relinker.GetCachedPath(asmCtx.ModuleMeta, asmName.Name);
        } else {
            asmPath = asm.Location;
        }

        var asmDef = AssemblyDefinition.ReadAssembly(asmPath, new ReaderParameters { ReadSymbols = true });
        var typeDef = asmDef.MainModule.GetType(method.DeclaringType!.FullName, runtimeName: true).Resolve();
        var methodDef = typeDef.Methods.Single(m => {
            if (method.Name != m.Name) {
                return false;

            }

            var runtimeParams = method.GetParameters();
            if (runtimeParams.Length != m.Parameters.Count) {
                return false;
            }

            for (int i = 0; i < runtimeParams.Length; i++) {
                var runtimeParam = runtimeParams[i];
                var asmParam = m.Parameters[i];

                if (runtimeParam.ParameterType.FullName != asmParam.ParameterType.FullName) {
                    return false;
                }
            }

            return true;
        });

        if (!methodDef.HasCustomDebugInformations || !methodDef.DebugInformation.HasSequencePoints) {
            return null;
        }

        var sequencePoint = methodDef.DebugInformation.SequencePoints[0];
        return (sequencePoint.Document.Url, sequencePoint.StartLine);
    }

    private static void EmitZoneStart(this ILCursor cur, VariableReference zoneVariable, bool isStatic, bool isVirtual, ProfileConfig config)
    {
        var method = cur.Method;

        // Setup zone name
        if (config.CustomZoneNameDelegate != null)
        {
            for (int i = 0; i < method.Parameters.Count; i++) {
                cur.EmitLdarg(i);
            }
            cur.EmitDelegate(config.CustomZoneNameDelegate);
        } else if (config.CustomZoneNameIL != null) {
            config.CustomZoneNameIL(cur);
        } else if (config.ZoneName != null) {
            cur.EmitLdstr(config.ZoneName);
        } else {
            if (isStatic | !isVirtual) {
                cur.EmitLdnull();
            } else {
                cur.EmitLdstr((config.MemberNameOverride ?? method.Name) + " (");
                cur.EmitLdarg0();
                cur.EmitCallvirt(typeof(object).GetMethod(nameof(GetType))!);
                cur.EmitCallvirt(typeof(Type).GetProperty(nameof(Type.FullName))!.GetGetMethod()!);
                cur.EmitLdstr(")");
                cur.EmitCall(typeof(string).GetMethod(nameof(string.Concat), [typeof(string), typeof(string), typeof(string)])!);
            }
        }

        cur.EmitLdcI4(1/*true*/); // active
        cur.EmitLdcI4(config.Color); // color
        cur.EmitLdnull(); // text
        cur.EmitLdcI4(config.LineOverride); // lineNumber
        // Slightly sketchy way to get the type name, but it's the best option..
        cur.EmitLdstr(config.FileNameOverride ?? $"{method.Name.Split("::")[0].Split(".")[^1]}.cs"); // filePath
        cur.EmitLdstr(config.MemberNameOverride ?? method.Name); // memberName
        cur.EmitDelegate(Profiler.BeginZone);
        cur.EmitStloc(zoneVariable);
    }
    private static void EmitZoneEnd(this ILCursor cur, VariableReference zoneVariable)
    {
        cur.EmitLdloca(zoneVariable);
        cur.EmitCall(typeof(Profiler.Zone).GetMethod(nameof(Profiler.Zone.Dispose))!);
    }
}
