using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Corellian.DeathStar.Signals
{
    internal static class UnixKill
    {
        [DllImport("libc", SetLastError = true)]
        public static extern int kill(int pid, int sig);
    }
}
