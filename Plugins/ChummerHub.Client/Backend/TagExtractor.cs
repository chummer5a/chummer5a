using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Chummer;
using ChummerHub.Client.Sinners;
using PropertyInfo = System.Reflection.PropertyInfo;
using Type = System.Type;

namespace ChummerHub.Client.Backend
{
    public static class TagExtractor
    {
        /// <summary>
        /// This function searches recursively through the Object "obj" and generates Tags for each
        /// property found with an HubTag-Attribute.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>A list of Tags (that may have a lot of child-Tags as well).</returns>
        internal static IEnumerable<Tag> ExtractTagsFromAttributes(object obj)
        {
            if (string.IsNullOrEmpty(obj as string))
            {
                if (obj.GetType().Assembly.FullName.Contains("Chummer"))
                {
                    //check, if the type has an HubClassTagAttribute
                    List<HubClassTagAttribute> classprops = new List<HubClassTagAttribute>();
                    foreach (object objCustomAttribute in obj.GetType().GetCustomAttributes(typeof(HubClassTagAttribute), true))
                    {
                        if (objCustomAttribute is HubClassTagAttribute objToAdd)
                            classprops.Add(objToAdd);
                    }
                    if (classprops.Count > 0)
                    {
                        foreach (HubClassTagAttribute classprop in classprops)
                        {
                            Tag tag = new Tag(obj, classprop);
                            tag.SetTagTypeEnumFromCLRType(obj.GetType());
                            if (!string.IsNullOrEmpty(classprop.ListInstanceNameFromProperty))
                            {
                                tag.TagName = classprop.ListInstanceNameFromProperty;
                                PropertyInfo childprop = obj.GetType().GetProperties().FirstOrDefault(x => x.Name == classprop.ListInstanceNameFromProperty);
                                if (childprop == null)
                                    throw new ArgumentOutOfRangeException("Could not find property " + classprop.ListInstanceNameFromProperty + " on instance of type " + obj.GetType() + ".");
                                tag.TagValue += childprop.GetValue(obj);
                            }
                            if (string.IsNullOrEmpty(tag.TagName))
                                tag.TagName = obj.ToString();
                            tag.AddPropertyValuesToTagComment(obj, classprop);
                            tag.Tags = new List<Tag>(ExtractTagsFromExtraProperties(obj, classprop));
                            yield return tag;
                        }
                        yield break;
                    }
                }

                if (obj is IEnumerable islist)
                {
                    foreach (object item in islist)
                    {
                        List<HubClassTagAttribute> classprops = new List<HubClassTagAttribute>();
                        foreach (object objCustomAttribute in item.GetType().GetCustomAttributes(typeof(HubClassTagAttribute), true))
                        {
                            if (objCustomAttribute is HubClassTagAttribute objToAdd)
                                classprops.Add(objToAdd);
                        }
                        foreach (HubClassTagAttribute classprop in classprops)
                        {
                            Tag tag = new Tag(item, classprop)
                            {
                                TagType = 0,// "list",
                                //TagName = classprop.ListName
                            };
                            if (!string.IsNullOrEmpty(classprop.ListInstanceNameFromProperty))
                            {
                                tag.TagName = classprop.ListInstanceNameFromProperty;
                                PropertyInfo childprop = item.GetType().GetProperties().FirstOrDefault(x => x.Name == classprop.ListInstanceNameFromProperty);
                                if (childprop == null)
                                    throw new ArgumentOutOfRangeException("Could not find property " + classprop.ListInstanceNameFromProperty + " on instance of type " + item.GetType() + ".");
                                tag.TagValue += childprop.GetValue(item);
                            }
                            if (string.IsNullOrEmpty(tag.TagName))
                                tag.TagName = item.ToString();

                            tag.AddPropertyValuesToTagComment(item, classprop);
                            tag.Tags = new List<Tag>(ExtractTagsFromExtraProperties(item, classprop));
                            yield return tag;
                        }
                    }
                    yield break;
                }
            }
            foreach (PropertyInfo property in obj.GetType().GetProperties())
            {
                object[] aCustomAttributes = property.GetCustomAttributes(typeof(HubTagAttribute), true);
                if (aCustomAttributes.Length > 0 && aCustomAttributes[0] is HubTagAttribute objAttribute)
                {
                    foreach (Tag objChild in ExtractTagsFromPropertyWithSpecificAttribute(obj, property, objAttribute))
                    {
                        if (!objAttribute.DeleteIfEmpty || objChild.Tags.Count > 0 || !string.IsNullOrEmpty(objChild.TagValue))
                        {
                            yield return objChild;
                        }
                    }
                }
            }
        }

        private static IEnumerable<Tag> ExtractTagsFromExtraProperties(object objPropertyHaver, HubClassTagAttribute objPropertyFilterAttribute)
        {
            PropertyInfo[] aPropertyInfos = objPropertyHaver.GetType().GetProperties();
            foreach(string includeprop in objPropertyFilterAttribute.ListExtraProperties)
            {
                PropertyInfo propfound = aPropertyInfos.FirstOrDefault(x => x.Name == includeprop);
                if(propfound == null)
                {
                    //sometimes we simply don't have a specialication (for example)
                    if (includeprop == "Specialization")
                        continue;
                    throw new ArgumentOutOfRangeException("Could not find property " + includeprop + " on instance of type " + objPropertyHaver.GetType() + " with name "+ objPropertyHaver.ToString()+".");

                }
                object includeInstance = propfound.GetValue(objPropertyHaver);
                if(includeInstance != null && !string.IsNullOrEmpty(includeInstance.ToString()))
                {
                    Tag instanceTag = new Tag(includeInstance, objPropertyFilterAttribute)
                    {
                        TagName = includeprop,
                        TagValue = includeInstance.ToString()
                    };
                    instanceTag.SetTagTypeEnumFromCLRType(objPropertyHaver.GetType());
                    yield return instanceTag;
                }
            }
        }

        private static IEnumerable<Tag> ExtractTagsFromPropertyWithSpecificAttribute(object objValue, PropertyInfo objProperty, HubTagAttribute attribute)
        {
            if(objProperty != null)
            {
                objValue = objProperty.GetValue(objValue);
                if (objValue == null)
                {
                    //don't save "null" values
                    yield break;
                }
                if(objValue.GetType().IsAssignableFrom(typeof(bool)) && objValue as bool? == false)
                {
                    //don't save "false" values
                    yield break;
                }
                if(objValue.GetType().IsAssignableFrom(typeof(int)) && objValue as int? == 0)
                {
                    //don't save "0" values
                    yield break;
                }
            }
            else if (objValue == null)
            {
                yield break;
            }
            Type objValueType = objValue.GetType();

            Tag tag = new Tag(objValue, attribute)
            {
                TagValue = !string.IsNullOrEmpty(attribute.TagValueFromProperty)
                    ? objValueType.GetProperty(attribute.TagValueFromProperty)?.GetValue(objValue, null)?.ToString() ?? string.Empty
                    : objValue.ToString(),
                TagName = !string.IsNullOrEmpty(attribute.TagName)
                    ? attribute.TagName
                    : !string.IsNullOrEmpty(objProperty?.Name)
                        ? objProperty.Name
                        : objValue.ToString()
            };
            tag.SetTagTypeEnumFromCLRType(objValueType);
            if(!string.IsNullOrEmpty(attribute.TagNameFromProperty))
            {
                try
                {
                    tag.TagName += objValueType.GetProperty(attribute.TagNameFromProperty)?.GetValue(objValue, null)?.ToString() ?? string.Empty;
                }
                catch (Exception)
                {
#if DEBUG
                    Debugger.Break();
#else
                    throw;
#endif
                }

            }

            if(objProperty != null)
            {
                tag.Tags = new List<Tag>(ExtractTagsFromAttributes(objValue));
            }
            yield return tag;
        }
    }
}
