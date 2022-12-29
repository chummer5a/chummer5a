/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */
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
