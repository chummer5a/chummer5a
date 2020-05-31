using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace ChummerHub.Services.GoogleDrive
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'EmbeddedResource'
    public static class EmbeddedResource
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'EmbeddedResource'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'EmbeddedResource.GetResource(string)'
        public static string GetResource(string namespaceAndFileName)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'EmbeddedResource.GetResource(string)'
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
