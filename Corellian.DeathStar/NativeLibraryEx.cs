using System.Reflection;
using System.Runtime.InteropServices;

namespace Corellian.DeathStar
{
    internal static class NativeLibraryEx
    {
        internal static IntPtr LoadFromResource(Assembly assembly,
            string libraryResourceName,
            string libraryName)
        {
            var assemblyPath = Path.GetDirectoryName(assembly.Location)!;
            var libraryFile = Path.Combine(assemblyPath, libraryName);

            if (!File.Exists(libraryFile))
            {
                var libraryDirectory = Path.GetDirectoryName(libraryFile)!;
                if (!Directory.Exists(libraryDirectory))
                {
                    Directory.CreateDirectory(libraryDirectory);
                }

                using var resourceStream = assembly.GetManifestResourceStream(libraryResourceName)!;
                using var fileStream = File.Create(libraryFile);
                resourceStream.CopyTo(fileStream);
            }

            return NativeLibrary.Load(libraryFile);
        }
    }
}
