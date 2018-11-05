using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ChummerHub.Services.GoogleDrive
{
    public static class EmbeddedResource
    {
        public static string GetResource(string namespaceAndFileName)
        {
            try
            {
                var assembly = typeof(EmbeddedResource).GetTypeInfo().Assembly;
                // This shows the available items.
                string[] resources = assembly.GetManifestResourceNames();


                using (var stream = assembly.GetManifestResourceStream(namespaceAndFileName))
                {
                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }

            catch (Exception exception)
            {
                System.Diagnostics.Trace.TraceError(exception.Message);
                throw new Exception($"Failed to read Embedded Resource {namespaceAndFileName}");
            }
        }
    }
}
