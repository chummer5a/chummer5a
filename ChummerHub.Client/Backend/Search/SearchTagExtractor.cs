using SINners.Models;
using System;
using System.Collections;
using System.Collections.Generic;
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


            var props2 = from p in obj.GetType().GetProperties()
                         let attr = p.GetCustomAttributes(typeof(Chummer.helpers.HubTagAttribute), true)
                         where attr.Length == 1
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
                    foreach (var item in islist)
                    {
                        List<Tuple<Chummer.helpers.HubClassTagAttribute, Object>> classprops = (from p in item.GetType().GetCustomAttributes(typeof(Chummer.helpers.HubClassTagAttribute), true)
                                                                                                select new Tuple<Chummer.helpers.HubClassTagAttribute, Object>(p as Chummer.helpers.HubClassTagAttribute, obj)).ToList();
                        foreach (var classprop in classprops)
                        {
                            var tag = new SearchTag(item, classprop.Item1);
                            tag.SParentTagId = parenttag.Id;
                            tag.MyParentTag = parenttag;
                            parenttag.STags.Add(tag);
                            resulttags.Add(tag);
                            tag.MyRuntimeHubClassTag = classprop.Item1;
                            //tag.TagName = classprop.Item1.ListName;
                            if (!String.IsNullOrEmpty(classprop.Item1.ListInstanceNameFromProperty))
                            {
                                var childprop = from p in item.GetType().GetProperties()
                                                where p.Name == classprop.Item1.ListInstanceNameFromProperty
                                                select p;
                                if (!childprop.Any())
                                    throw new ArgumentOutOfRangeException("Could not find property " + classprop.Item1.ListInstanceNameFromProperty + " on instance of type " + item.GetType().ToString() + ".");
                                tag.STagName += childprop.FirstOrDefault().GetValue(item);
                            }
                            if (String.IsNullOrEmpty(tag.STagName))
                                tag.STagName = item.ToString();

                            //Add complex Object
                            var childtags = ExtractTagsFromAttributes(item, tag);
                            if (classprop.Item1.DeleteEmptyTags)
                            {
                                if (!tag.STags.Any())
                                {
                                    parenttag.STags.Remove(tag);
                                    resulttags.Remove(tag);
                                }
                            }
                        }



                    }
                    return resulttags;
                }
            }

            foreach (var prop in props)
            {
                SearchTag temptag = new SearchTag(prop.Item1, prop.Item2);
                temptag.MyParentTag = parenttag;
                temptag.SSearchOpterator = EnumSSearchOpterator.bigger.ToString();
                temptag.STagName = prop.Item1.Name;
                temptag.STagValue = "";
                //var proptags = ExtractTagsFromAttributesForProperty(prop, parenttag);
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


            var tag = new SearchTag(propValue, attribute);

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
            tag.STagValue = String.Format("{0}", tag.MyRuntimeObject);
            Type typeValue = tag.MyRuntimeObject.GetType();
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
                var childlist = ExtractTagsFromAttributes(tag.MyRuntimeObject, tag);
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
