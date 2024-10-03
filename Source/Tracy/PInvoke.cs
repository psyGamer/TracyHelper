// Based on https://github.com/clibequilibrium/Tracy-CSharp/blob/main/src/cs/production/Tracy/Generated/PInvoke.gen.cs

using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Celeste.Mod.TracyHelper.Tracy;

public static unsafe partial class PInvoke
{
    private const string LibraryName = "TracyClient";

    #region API

    [LibraryImport(LibraryName, EntryPoint = "___tracy_alloc_srcloc")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial ulong TracyAllocSrcloc(uint line, CString source, ulong sourceSz, CString function, ulong functionSz);

    [LibraryImport(LibraryName, EntryPoint = "___tracy_alloc_srcloc_name")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial ulong TracyAllocSrclocName(uint line, CString source, ulong sourceSz, CString function, ulong functionSz, CString name, ulong nameSz);

    [LibraryImport(LibraryName, EntryPoint = "___tracy_connected")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int TracyConnected();

    [LibraryImport(LibraryName, EntryPoint = "___tracy_emit_frame_image")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void TracyEmitFrameImage(void* image, ushort w, ushort h, byte offset, int flip);

    [LibraryImport(LibraryName, EntryPoint = "___tracy_emit_frame_mark")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void TracyEmitFrameMark(CString name);

    [LibraryImport(LibraryName, EntryPoint = "___tracy_emit_frame_mark_end")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void TracyEmitFrameMarkEnd(CString name);

    [LibraryImport(LibraryName, EntryPoint = "___tracy_emit_frame_mark_start")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void TracyEmitFrameMarkStart(CString name);

    [LibraryImport(LibraryName, EntryPoint = "___tracy_emit_gpu_calibration")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void TracyEmitGpuCalibration(TracyGpuCalibrationData param);

    [LibraryImport(LibraryName, EntryPoint = "___tracy_emit_gpu_calibration_serial")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void TracyEmitGpuCalibrationSerial(TracyGpuCalibrationData param);

    [LibraryImport(LibraryName, EntryPoint = "___tracy_emit_gpu_context_name")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void TracyEmitGpuContextName(TracyGpuContextNameData param);

    [LibraryImport(LibraryName, EntryPoint = "___tracy_emit_gpu_context_name_serial")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void TracyEmitGpuContextNameSerial(TracyGpuContextNameData param);

    [LibraryImport(LibraryName, EntryPoint = "___tracy_emit_gpu_new_context")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void TracyEmitGpuNewContext(TracyGpuNewContextData param);

    [LibraryImport(LibraryName, EntryPoint = "___tracy_emit_gpu_new_context_serial")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void TracyEmitGpuNewContextSerial(TracyGpuNewContextData param);

    [LibraryImport(LibraryName, EntryPoint = "___tracy_emit_gpu_time")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void TracyEmitGpuTime(TracyGpuTimeData param);

    [LibraryImport(LibraryName, EntryPoint = "___tracy_emit_gpu_time_serial")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void TracyEmitGpuTimeSerial(TracyGpuTimeData param);

    [LibraryImport(LibraryName, EntryPoint = "___tracy_emit_gpu_zone_begin")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void TracyEmitGpuZoneBegin(TracyGpuZoneBeginData param);

    [LibraryImport(LibraryName, EntryPoint = "___tracy_emit_gpu_zone_begin_alloc")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void TracyEmitGpuZoneBeginAlloc(TracyGpuZoneBeginData param);

    [LibraryImport(LibraryName, EntryPoint = "___tracy_emit_gpu_zone_begin_alloc_callstack")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void TracyEmitGpuZoneBeginAllocCallstack(TracyGpuZoneBeginCallstackData param);

    [LibraryImport(LibraryName, EntryPoint = "___tracy_emit_gpu_zone_begin_alloc_callstack_serial")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void TracyEmitGpuZoneBeginAllocCallstackSerial(TracyGpuZoneBeginCallstackData param);

    [LibraryImport(LibraryName, EntryPoint = "___tracy_emit_gpu_zone_begin_alloc_serial")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void TracyEmitGpuZoneBeginAllocSerial(TracyGpuZoneBeginData param);

    [LibraryImport(LibraryName, EntryPoint = "___tracy_emit_gpu_zone_begin_callstack")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void TracyEmitGpuZoneBeginCallstack(TracyGpuZoneBeginCallstackData param);

    [LibraryImport(LibraryName, EntryPoint = "___tracy_emit_gpu_zone_begin_callstack_serial")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void TracyEmitGpuZoneBeginCallstackSerial(TracyGpuZoneBeginCallstackData param);

    [LibraryImport(LibraryName, EntryPoint = "___tracy_emit_gpu_zone_begin_serial")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void TracyEmitGpuZoneBeginSerial(TracyGpuZoneBeginData param);

    [LibraryImport(LibraryName, EntryPoint = "___tracy_emit_gpu_zone_end")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void TracyEmitGpuZoneEnd(TracyGpuZoneEndData data);

    [LibraryImport(LibraryName, EntryPoint = "___tracy_emit_gpu_zone_end_serial")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void TracyEmitGpuZoneEndSerial(TracyGpuZoneEndData data);

    [LibraryImport(LibraryName, EntryPoint = "___tracy_emit_memory_alloc")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void TracyEmitMemoryAlloc(void* ptr, ulong size, int secure);

    [LibraryImport(LibraryName, EntryPoint = "___tracy_emit_memory_alloc_callstack")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void TracyEmitMemoryAllocCallstack(void* ptr, ulong size, int depth, int secure);

    [LibraryImport(LibraryName, EntryPoint = "___tracy_emit_memory_alloc_callstack_named")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void TracyEmitMemoryAllocCallstackNamed(void* ptr, ulong size, int depth, int secure, CString name);

    [LibraryImport(LibraryName, EntryPoint = "___tracy_emit_memory_alloc_named")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void TracyEmitMemoryAllocNamed(void* ptr, ulong size, int secure, CString name);

    [LibraryImport(LibraryName, EntryPoint = "___tracy_emit_memory_free")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void TracyEmitMemoryFree(void* ptr, int secure);

    [LibraryImport(LibraryName, EntryPoint = "___tracy_emit_memory_free_callstack")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void TracyEmitMemoryFreeCallstack(void* ptr, int depth, int secure);

    [LibraryImport(LibraryName, EntryPoint = "___tracy_emit_memory_free_callstack_named")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void TracyEmitMemoryFreeCallstackNamed(void* ptr, int depth, int secure, CString name);

    [LibraryImport(LibraryName, EntryPoint = "___tracy_emit_memory_free_named")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void TracyEmitMemoryFreeNamed(void* ptr, int secure, CString name);

    [LibraryImport(LibraryName, EntryPoint = "___tracy_emit_message")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void TracyEmitMessage(CString txt, ulong size, int callstack);

    [LibraryImport(LibraryName, EntryPoint = "___tracy_emit_message_appinfo")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void TracyEmitMessageAppinfo(CString txt, ulong size);

    [LibraryImport(LibraryName, EntryPoint = "___tracy_emit_messageC")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void TracyEmitMessageC(CString txt, ulong size, uint color, int callstack);

    [LibraryImport(LibraryName, EntryPoint = "___tracy_emit_messageL")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void TracyEmitMessageL(CString txt, int callstack);

    [LibraryImport(LibraryName, EntryPoint = "___tracy_emit_messageLC")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void TracyEmitMessageLC(CString txt, uint color, int callstack);

    [LibraryImport(LibraryName, EntryPoint = "___tracy_emit_plot")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void TracyEmitPlot(CString name, Double val);

    [LibraryImport(LibraryName, EntryPoint = "___tracy_emit_plot_config")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void TracyEmitPlotConfig(CString name, int type, int step, int fill, uint color);

    [LibraryImport(LibraryName, EntryPoint = "___tracy_emit_plot_float")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void TracyEmitPlotFloat(CString name, float val);

    [LibraryImport(LibraryName, EntryPoint = "___tracy_emit_plot_int")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void TracyEmitPlotInt(CString name, long val);

    [LibraryImport(LibraryName, EntryPoint = "___tracy_emit_zone_begin")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial TracyCZoneCtx TracyEmitZoneBegin(TracySourceLocationData* srcloc, int active);

    [LibraryImport(LibraryName, EntryPoint = "___tracy_emit_zone_begin_alloc")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial TracyCZoneCtx TracyEmitZoneBeginAlloc(ulong srcloc, int active);

    [LibraryImport(LibraryName, EntryPoint = "___tracy_emit_zone_begin_alloc_callstack")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial TracyCZoneCtx TracyEmitZoneBeginAllocCallstack(ulong srcloc, int depth, int active);

    [LibraryImport(LibraryName, EntryPoint = "___tracy_emit_zone_begin_callstack")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial TracyCZoneCtx TracyEmitZoneBeginCallstack(TracySourceLocationData* srcloc, int depth, int active);

    [LibraryImport(LibraryName, EntryPoint = "___tracy_emit_zone_color")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void TracyEmitZoneColor(TracyCZoneCtx ctx, uint color);

    [LibraryImport(LibraryName, EntryPoint = "___tracy_emit_zone_end")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void TracyEmitZoneEnd(TracyCZoneCtx ctx);

    [LibraryImport(LibraryName, EntryPoint = "___tracy_emit_zone_name")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void TracyEmitZoneName(TracyCZoneCtx ctx, CString txt, ulong size);

    [LibraryImport(LibraryName, EntryPoint = "___tracy_emit_zone_text")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void TracyEmitZoneText(TracyCZoneCtx ctx, CString txt, ulong size);

    [LibraryImport(LibraryName, EntryPoint = "___tracy_emit_zone_value")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void TracyEmitZoneValue(TracyCZoneCtx ctx, ulong value);

    [LibraryImport(LibraryName, EntryPoint = "___tracy_set_thread_name")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void TracySetThreadName(CString name);

    #endregion
    #region Types

    [StructLayout(LayoutKind.Explicit, Size = 8, Pack = 4)]
    public struct TracyCZoneContext
    {
        [FieldOffset(0)] // size = 4
        public uint Id;

        [FieldOffset(4)] // size = 4
        public int Active;
    }

    [StructLayout(LayoutKind.Explicit, Size = 24, Pack = 8)]
    public struct TracyGpuCalibrationData
    {
        [FieldOffset(0)] // size = 8
        public long GpuTime;

        [FieldOffset(8)] // size = 8
        public long CpuDelta;

        [FieldOffset(16)] // size = 1
        public byte Context;
    }

    [StructLayout(LayoutKind.Explicit, Size = 24, Pack = 8)]
    public struct TracyGpuContextNameData
    {
        [FieldOffset(0)] // size = 1
        public byte Context;

        [FieldOffset(8)] // size = 8
        public CString _Name;

        public string Name
        {
            get => CString.ToString(_Name);
            set => _Name = CString.FromString(value);
        }

        [FieldOffset(16)] // size = 2
        public ushort Len;
    }

    [StructLayout(LayoutKind.Explicit, Size = 16, Pack = 8)]
    public struct TracyGpuNewContextData
    {
        [FieldOffset(0)] // size = 8
        public long GpuTime;

        [FieldOffset(8)] // size = 4
        public float Period;

        [FieldOffset(12)] // size = 1
        public byte Context;

        [FieldOffset(13)] // size = 1
        public byte Flags;

        [FieldOffset(14)] // size = 1
        public byte Type;
    }

    [StructLayout(LayoutKind.Explicit, Size = 16, Pack = 8)]
    public struct TracyGpuTimeData
    {
        [FieldOffset(0)] // size = 8
        public long GpuTime;

        [FieldOffset(8)] // size = 2
        public ushort QueryId;

        [FieldOffset(10)] // size = 1
        public byte Context;
    }

    [StructLayout(LayoutKind.Explicit, Size = 16, Pack = 8)]
    public struct TracyGpuZoneBeginCallstackData
    {
        [FieldOffset(0)] // size = 8
        public ulong Srcloc;

        [FieldOffset(8)] // size = 4
        public int Depth;

        [FieldOffset(12)] // size = 2
        public ushort QueryId;

        [FieldOffset(14)] // size = 1
        public byte Context;
    }

    [StructLayout(LayoutKind.Explicit, Size = 16, Pack = 8)]
    public struct TracyGpuZoneBeginData
    {
        [FieldOffset(0)] // size = 8
        public ulong Srcloc;

        [FieldOffset(8)] // size = 2
        public ushort QueryId;

        [FieldOffset(10)] // size = 1
        public byte Context;
    }

    [StructLayout(LayoutKind.Explicit, Size = 4, Pack = 2)]
    public struct TracyGpuZoneEndData
    {
        [FieldOffset(0)] // size = 2
        public ushort QueryId;

        [FieldOffset(2)] // size = 1
        public byte Context;
    }

    [StructLayout(LayoutKind.Explicit, Size = 32, Pack = 8)]
    public struct TracySourceLocationData
    {
        [FieldOffset(0)] // size = 8
        public CString _Name;

        public string Name
        {
            get => CString.ToString(_Name);
            set => _Name = CString.FromString(value);
        }

        [FieldOffset(8)] // size = 8
        public CString _Function;

        public string Function
        {
            get => CString.ToString(_Function);
            set => _Function = CString.FromString(value);
        }

        [FieldOffset(16)] // size = 8
        public CString _File;

        public string File
        {
            get => CString.ToString(_File);
            set => _File = CString.FromString(value);
        }

        [FieldOffset(24)] // size = 4
        public uint Line;

        [FieldOffset(28)] // size = 4
        public uint Color;
    }

    public enum TracyPlotFormatEnum : int
    {
        TracyPlotFormatNumber = 0,
        TracyPlotFormatMemory = 1,
        TracyPlotFormatPercentage = 2,
        TracyPlotFormatWatt = 3
    }

    [StructLayout(LayoutKind.Explicit, Size = 8, Pack = 4)]
    public struct TracyCZoneCtx
    {
        [FieldOffset(0)]
        public TracyCZoneContext Data;

        public static implicit operator TracyCZoneContext(TracyCZoneCtx data) => data.Data;
        public static implicit operator TracyCZoneCtx(TracyCZoneContext data) => new() { Data = data };
    }

    #endregion
}
