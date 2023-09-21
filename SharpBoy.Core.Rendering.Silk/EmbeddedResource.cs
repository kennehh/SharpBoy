using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoy.Core.Rendering.Silk
{
    internal static class EmbeddedResource
    {
        private static Assembly assembly = Assembly.GetExecutingAssembly();

        public static string LoadResourceAsString(string resourceName)
        {
            var name = GetFullResourceName(resourceName);
            using (var stream = assembly.GetManifestResourceStream(name))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        private static string GetFullResourceName(string resourceName)
        {
            return assembly.GetManifestResourceNames().Single(x => x.Contains(resourceName));
        }
    }
}
