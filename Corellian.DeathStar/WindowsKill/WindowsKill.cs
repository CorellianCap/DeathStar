using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming
namespace Corellian.DeathStar.WindowsKill
{
    internal static class WindowsKill
    {
        public const uint SIGNAL_TYPE_CTRL_C = 0;

        public const uint SIGNAL_TYPE_CTRL_BREAK = 1;

        [DllImport("windows-kill-library.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "?sendSignal@WindowsKillLibrary@@YAXKK@Z", ExactSpelling = true)]
        public static extern void sendSignal(uint signal_pid, uint signal_type);
    }
}
