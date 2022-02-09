using System;
using System.Runtime.InteropServices;

namespace SoundCharts.Explorer.Logging;

public static class OSLog
{
    [DllImport("System", EntryPoint = "os_log_create")]
    public static extern IntPtr os_log_create(string subsystem, string category);

    [DllImport("SoundCharts.Explorer.NativeLogging", EntryPoint = "Log")]
    public static extern void Log(IntPtr log, string msg);
}
