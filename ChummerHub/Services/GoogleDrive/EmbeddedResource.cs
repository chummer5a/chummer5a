using System;
using System.IO;
using System.Reflection;
using System.Text;

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
                    if (stream != null)
                    {
                        using (var reader = new StreamReader(stream, Encoding.UTF8))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }

            catch (Exception exception)
            {
                System.Diagnostics.Trace.TraceError(exception.Message);
                throw new Exception($"Failed to read Embedded Resource {namespaceAndFileName}");
            }

            return string.Empty;
        }
    }
}
