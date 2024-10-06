using System;
using System.Runtime.CompilerServices;
using Celeste.Mod.TracyHelper;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using MonoMod.InlineRT;
using MonoMod.Utils;

namespace Celeste.Mod.TracyHelper
{
    [MonoModCustomMethodAttribute(nameof(MonoModRules.ProfileMethod)), AttributeUsage(AttributeTargets.Method)]
    internal class ProfileMethod(
        string? zoneName = null,
        bool active = true,
        uint color = 0x000000,
        string? text = null,
        [CallerLineNumber] int lineNumber = 0,
        [CallerFilePath] string? filePath = null,
        [CallerMemberName] string? memberName = null
    ) : Attribute;
}

namespace MonoMod {
    internal static class MonoModRules {
        static MonoModRules() { }

        public static void ProfileMethod(MethodDefinition method, CustomAttribute attrib) {
            var t_Profiler = MonoModRule.Modder.FindType("Celeste.Mod.TracyHelper.Profiler").Resolve()!;
            var t_Profiler_Zone = MonoModRule.Modder.FindType("Celeste.Mod.TracyHelper.Profiler/Zone").Resolve()!;
            var m_Profiler_BeginZone = t_Profiler.FindMethod("BeginZone")!;
            var m_Profiler_Zone_Dispose = t_Profiler_Zone.FindMethod("Dispose")!;

            string? zoneName = (string?)attrib.ConstructorArguments[0].Value;
            bool active = (bool)attrib.ConstructorArguments[1].Value;
            uint color = (uint)attrib.ConstructorArguments[2].Value;
            string? text = (string?)attrib.ConstructorArguments[3].Value;
            int lineNumber = (int)attrib.ConstructorArguments[4].Value + 1;
            string filePath = (string?)attrib.ConstructorArguments[5].Value ?? method.DeclaringType.Name + ".cs";
            string memberName = (string?)attrib.ConstructorArguments[6].Value ?? method.FullName;

            new ILContext(method).Invoke(il => {
                var cur = new ILCursor(il);

                // Create a try-finally block to properly dispose the zone
                var exceptionHandler = new ExceptionHandler(ExceptionHandlerType.Finally);
                il.Body.ExceptionHandlers.Add(exceptionHandler);

                // Store zone in a local variable
                var zoneVar = new VariableDefinition(t_Profiler_Zone);
                il.Body.Variables.Add(zoneVar);

                // Store return value in a local variable (if needed)
                var returnVar = new VariableDefinition(il.Method.ReturnType);
                bool nonVoidReturnType = il.Method.ReturnType.FullName != "System.Void";

                if (nonVoidReturnType) {
                    il.Body.Variables.Add(returnVar);
                }

                // Begin profiler zone
                {
                    // Setup zone name
                    if (zoneName != null) {
                        cur.EmitLdstr(zoneName);
                    } else {
                        if (method.IsStatic | !method.IsVirtual) {
                            cur.EmitLdnull();
                        } else {
                            cur.EmitLdstr(memberName + " (");
                            cur.EmitLdarg0();
                            cur.EmitCallvirt(typeof(object).GetMethod(nameof(GetType))!);
                            cur.EmitCallvirt(typeof(Type).GetProperty(nameof(Type.FullName))!.GetGetMethod()!);
                            cur.EmitLdstr(")");
                            cur.EmitCall(typeof(string).GetMethod(nameof(string.Concat), [typeof(string), typeof(string), typeof(string)])!);
                        }
                    }

                    cur.EmitLdcI4(active ? 1 : 0);
                    cur.EmitLdcI4(color);
                    if (text != null) {
                        cur.EmitLdstr(text);
                    } else {
                        cur.EmitLdnull(); // text
                    }
                    cur.EmitLdcI4(lineNumber);
                    cur.EmitLdstr(filePath);
                    cur.EmitLdstr(memberName);
                    cur.EmitCall(m_Profiler_BeginZone);
                    cur.EmitStloc(zoneVar);
                }

                // // Begin try-block
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
                cur.EmitCall(m_Profiler_Zone_Dispose);

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

                Console.WriteLine(il);
            });
        }
    }
}
