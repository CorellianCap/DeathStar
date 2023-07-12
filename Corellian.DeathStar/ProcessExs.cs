using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Corellian.DeathStar.Signals;

namespace Corellian.DeathStar
{
    // The library windows-kill is from https://github.com/ElyDotDev/windows-kill
    public static class ProcessExs
    {
        private static class Constants
        {
            internal const uint WindowsKillSignal = WindowsKill.SIGNAL_TYPE_CTRL_C;
            internal const int WindowsKillRetryCount = 3;
            internal const int UnixKillSignal = 2;
        }

        static ProcessExs()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var x = Environment.Is64BitProcess ? "x64" : "x86";

                NativeLibraryEx.LoadFromResource(Assembly.GetAssembly(typeof(ProcessExs))!,
                    $"Corellian.DeathStar.Signals.{x}.windows-kill-library.dll", $"{x}\\windows-kill-library.dll");
            }
        }

        public static bool SignalKill(this Process process)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // HACK: for when windows-kill does not work
                for (var i = 0; i < Constants.WindowsKillRetryCount; i++)
                {
                    try
                    {
                        WindowsKill.sendSignal(Convert.ToUInt32(process.Id), Constants.WindowsKillSignal);
                        return true;
                    }
                    catch (SEHException)
                    {
                        // Ignore
                    }
                }
            }
            else
            {
                return UnixKill.kill(process.Id, Constants.UnixKillSignal) == 0;
            }

            return false;
        }

        public static async Task<bool> Stop(this Process process,
            int signalKillRetryCount, TimeSpan signalKillRetryDelay, int signalKillCheckCount, TimeSpan signalKillCheckDelay,
            int processKillRetryCount, TimeSpan processKillRetryDelay, int processKillCheckCount, TimeSpan processKillCheckDelay)
        {
            for (var r = 0; r < signalKillRetryCount; r++)
            {
                if (r > 0)
                {
                    await Task.Delay(signalKillRetryDelay).ConfigureAwait(false);
                }

                var signalKillSent = process.SignalKill();
                Debug.WriteLine($"SignalKill {process.Id} attempt {r} was sent {signalKillSent}");

                for (var c = 0; c < signalKillCheckCount; c++)
                {
                    await Task.Delay(signalKillCheckDelay).ConfigureAwait(false);

                    if (process is { HasExited: true })
                    {
                        Debug.WriteLine($"SignalKill {process.Id} has exited {process.HasExited}");
                        return true;
                    }
                    Debug.WriteLine($"SignalKill {process.Id} has exited {process.HasExited}");
                }
            }


            for (var r = 0; r < processKillRetryCount; r++)
            {
                if (r > 0 || signalKillRetryCount > 0)
                {
                    await Task.Delay(processKillRetryDelay).ConfigureAwait(false);
                }

                process.Kill(true);
                Debug.WriteLine($"Kill {process.Id} attempt {r}");

                for (int i = 0; i < processKillCheckCount; i++)
                {
                    await Task.Delay(processKillCheckDelay).ConfigureAwait(false);

                    if (process is { HasExited: true })
                    {
                        Debug.WriteLine($"SignalKill {process.Id} has exited {process.HasExited}");
                        return true;
                    }
                    Debug.WriteLine($"Kill {process.Id} has exited {process.HasExited}");
                }
            }

            return false;
        }
    }
}