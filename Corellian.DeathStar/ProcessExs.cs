using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Corellian.DeathStar
{
    // The library windows-kill is from https://github.com/ElyDotDev/windows-kill
    public static class ProcessExs
    {
        private static class Constants
        {
            internal const int WindowsKillRetryCount = 3;
        }

        static ProcessExs()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var assemblyPath = Path.GetDirectoryName(Assembly.GetAssembly(typeof(ProcessExs))!.Location);

                var windowsKillLibraryPath = Environment.Is64BitProcess
                    ? Path.Combine(assemblyPath!, @"WindowsKill\x64\windows-kill-library.dll")
                    : Path.Combine(assemblyPath!, @"WindowsKill\x86\windows-kill-library.dll");

                NativeLibrary.Load(windowsKillLibraryPath);
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
                        WindowsKill.WindowsKill.sendSignal(Convert.ToUInt32(process.Id), WindowsKill.WindowsKill.SIGNAL_TYPE_CTRL_C);
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
                throw new PlatformNotSupportedException();
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
                    await Task.Delay(signalKillRetryDelay);
                }

                var signalKillSent = process.SignalKill();
                Debug.WriteLine($"SignalKill {process.Id} {signalKillSent}");

                for (var c = 0; c < signalKillCheckCount; c++)
                {
                    await Task.Delay(signalKillCheckDelay);

                    if (process is { HasExited: true })
                    {
                        return true;
                    }
                }
            }


            for (var r = 0; r < processKillRetryCount; r++)
            {
                if (r > 0 || signalKillRetryCount > 0)
                {
                    await Task.Delay(processKillRetryDelay);
                }

                process.Kill(true);
                Debug.WriteLine($"Kill {process.Id}");

                for (int i = 0; i < processKillCheckCount; i++)
                {
                    await Task.Delay(processKillCheckDelay);

                    if (process is { HasExited: true })
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}