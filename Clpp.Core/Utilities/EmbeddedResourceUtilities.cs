using System.IO;
using System.Reflection;

namespace Clpp.Core.Utilities
{
    public static class EmbeddedResourceUtilities
    {
        public static string ReadEmbeddedStream(string resourceFilePath, Assembly assembly = null)
        {
            assembly = assembly ?? Assembly.GetExecutingAssembly();

            using (var stream = assembly.GetManifestResourceStream(resourceFilePath))
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}