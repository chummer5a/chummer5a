using Chummer;
using ChummerHub.Client.Sinners;
using ChummerHub.Client.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using PropertyInfo = System.Reflection.PropertyInfo;

namespace ChummerHub.Client.Backend
{
    public static class SearchTagExtractor
    {
        /// <summary>
        /// This function searches recursively through the Object "obj" and generates Tags for each
        /// property found with an HubTag-Attribute.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>A list of Tags (that may have a lot of child-Tags as well).</returns>
        internal static IEnumerable<SearchTag> ExtractTagsFromAttributes(object obj)
        {
            if (obj == null)
                yield break;

            if (string.IsNullOrEmpty(obj as string))
            {
                if (obj is IEnumerable islist)
                {
                    Type listtype = StaticUtils.GetListType(islist);
                    object generic;
                    try
                    {
                        generic = Activator.CreateInstance(listtype, ucSINnersSearch.MySearchCharacter.MyCharacter);
                    }
                    catch (Exception)
                    {
                        try
                        {
                            generic = Activator.CreateInstance(listtype);
                        }
                        catch(Exception e2)
                        {
                            //seriously, that gets out of hand...
                            Trace.TraceError(e2.ToString());
                            throw;
                        }
                    }
                    foreach (SearchTag tagChild in ExtractTagsFromAttributes(generic))
                        yield return tagChild;
                    yield break;
                }
            }
            foreach (PropertyInfo property in obj.GetType().GetProperties())
            {
                object[] aCustomAttributes = property.GetCustomAttributes(typeof(HubTagAttribute), true);
                if (aCustomAttributes.Length > 0 && aCustomAttributes[0] is HubTagAttribute objAttribute)
                {
                    yield return new SearchTag(property, objAttribute)
                    {
                        SearchOperator = "bigger",
                        TagName = property.Name,
                        TagValue = string.Empty,
                        MyRuntimePropertyValue = property.GetValue(obj)
                    };
                }
            }
        }
    }
}
