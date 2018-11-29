using Chummer;
using ChummerHub.Client.UI;
using SINners.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ChummerHub.Client.Backend
{

    
    public class SearchTagExtractor
    {
        


        /// <summary>
        /// This function searches recursivly through the Object "obj" and generates Tags for each
        /// property found with an HubTag-Attribute.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="parenttag"></param>
        /// <returns>A list of Tags (that may have a lot of child-Tags as well).</returns>
        internal static IList<SearchTag> ExtractTagsFromAttributes(Object obj, SearchTag parenttag)
        {
            List<SearchTag> resulttags = new List<SearchTag>();
            List<Tuple<PropertyInfo, Chummer.helpers.HubTagAttribute, Object>> props = new List<Tuple<PropertyInfo, Chummer.helpers.HubTagAttribute, Object>>();


            var test =  (from p in obj.GetType().GetProperties() select p).ToList();
            var test2 = (from p in obj.GetType().GetProperties() let attr = p.GetCustomAttributes(typeof(Chummer.helpers.HubTagAttribute), true)  select attr).ToList();
            var test4 = obj.GetType().GenericTypeArguments;

            

            var props2 = from p in obj.GetType().GetProperties()
                         let attr = p.GetCustomAttributes(typeof(Chummer.helpers.HubTagAttribute), true)
                         where attr.Length > 0
                         select new Tuple<PropertyInfo, Chummer.helpers.HubTagAttribute, Object>(p, attr.First() as Chummer.helpers.HubTagAttribute, obj);

            props.AddRange(props2);
            

            if (!typeof(String).IsAssignableFrom(obj.GetType()))
            {
                IEnumerable islist = obj as IEnumerable;

                if (islist == null)
                {
                    islist = obj as ICollection;
                }
                if (islist != null)
                {
                    Type listtype = StaticUtils.GetListType(islist);
                    try
                    {
                        Object generic = Activator.CreateInstance(listtype, new object[] { SINnersSearch.MySearchCharacter.MyCharacter });
                        var result = ExtractTagsFromAttributes(generic, parenttag);
                        return result;
                    }
                    catch(Exception e1)
                    {
                        try
                        {
                            Object generic = Activator.CreateInstance(listtype);
                            var result = ExtractTagsFromAttributes(generic, parenttag);
                            return result;
                        }
                        catch(Exception e2)
                        {
                            //seriously, that gets out of hand...
                            System.Diagnostics.Trace.TraceError(e2.ToString());
                            throw;
                        }
                    }
                    
                }
            }

            foreach (var prop in props)
            {
                SearchTag temptag = new SearchTag(prop.Item1, prop.Item2);
                temptag.MyParentTag = parenttag;
                temptag.SSearchOpterator = EnumSSearchOpterator.bigger.ToString();
                temptag.STagName = prop.Item1.Name;
                temptag.STagValue = "";
                temptag.MyRuntimePropertyValue = prop.Item1.GetValue(prop.Item3);
                resulttags.Add(temptag);
            }
            return resulttags;
        }


        internal static IList<SearchTag> ExtractTagsFromAttributesForProperty(Tuple<PropertyInfo, Chummer.helpers.HubTagAttribute, Object> prop, SearchTag parenttag)
        {
            List<SearchTag> proptaglist = new List<SearchTag>();
            Chummer.helpers.HubTagAttribute attribute = prop.Item2 as Chummer.helpers.HubTagAttribute;
            Object propValue;
            PropertyInfo property = prop.Item1 as PropertyInfo;
            if (property == null)
            {
                propValue = prop.Item3;
            }
            else
            {
                propValue = property.GetValue(prop.Item3);
                if (propValue.GetType().IsAssignableFrom(typeof(bool)))
                {
                    if (propValue as bool? == false)
                    { //dont save "false" values
                        return proptaglist;
                    }
                }
                if (propValue.GetType().IsAssignableFrom(typeof(int)))
                {
                    if (propValue as int? == 0)
                    {   //dont save "0" values
                        return proptaglist;
                    }
                }
            }


            var tag = new SearchTag(property, attribute);
            tag.MyRuntimePropertyValue = propValue;
            tag.STags = new List<SearchTag>();
            tag.MyParentTag = parenttag;
            if (tag.MyParentTag != null)
                tag.MyParentTag.STags.Add(tag);
            tag.SParentTagId = parenttag?.Id;
            tag.Id = Guid.NewGuid();
            if (!String.IsNullOrEmpty(attribute.TagName))
                tag.STagName = attribute.TagName;
            else if (prop.Item1 != null)
                tag.STagName = prop.Item1.Name;
            else
                tag.STagName = prop.Item3.ToString();

            Type t = prop.Item3.GetType();
            if (!String.IsNullOrEmpty(attribute.TagNameFromProperty))
            {
                var addObject = t.GetProperty(attribute.TagNameFromProperty).GetValue(prop.Item3, null);
                tag.STagName += String.Format("{0}", addObject);
            }
            tag.STagValue = String.Format("{0}", tag.MyRuntimePropertyValue);
            Type typeValue = tag.MyRuntimePropertyValue.GetType();
            //if (typeof(int).IsAssignableFrom(typeValue))
            //{
            //    tag.TagType = "int";
            //}
            //else if (typeof(double).IsAssignableFrom(typeValue))
            //{
            //    tag.TagType = "double";
            //}
            //else if (typeof(bool).IsAssignableFrom(typeValue))
            //{
            //    tag.TagType = "bool";
            //}
            //else if (typeof(string).IsAssignableFrom(typeValue))
            //{
            //    tag.TagType = "string";
            //}
            //else if (typeof(Guid).IsAssignableFrom(typeValue))
            //{
            //    tag.TagType = "Guid";
            //}
            //else
            //{
            //    tag.TagType = "other";
            //}
            //if (tag.TagValue == typeValue.FullName)
            //    tag.TagValue = "";
            //if ((typeof(IEnumerable).IsAssignableFrom(typeValue)
            //    || typeof(ICollection).IsAssignableFrom(typeValue))
            //    && !typeof(string).IsAssignableFrom(typeValue))
            //{
            //    tag.TagType = "list";
            //    tag.TagValue = "";
            //}
            //if (!String.IsNullOrEmpty(attribute.TagValueFromProperty))
            //{
            //    var addObject = t.GetProperty(attribute.TagValueFromProperty).GetValue(prop.Item3, null);
            //    tag.TagValue = String.Format("{0}", addObject);
            //}
            proptaglist.Add(tag);
            if (prop.Item1 != null)
            {
                var childlist = ExtractTagsFromAttributes(tag.MyRuntimePropertyValue, tag);
                proptaglist.AddRange(childlist);
            }
            if (attribute.DeleteIfEmpty)
            {
                if (!tag.STags.Any() && String.IsNullOrEmpty(tag.STagValue))
                {
                    tag.MyParentTag.STags.Remove(tag);
                }
            }
            return proptaglist;
        }
    }
}
