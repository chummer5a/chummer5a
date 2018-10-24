using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Chummer;
using SINners.Models;

namespace ChummerHub.Client.Backend
{
    public static class TagExtractor
    {
        private static IEnumerable<Type> _AllChummerTypes = null;
        public static IEnumerable<Type> AllChummerTypes
        {
            get
            {
                if (_AllChummerTypes == null)
                {
                    _AllChummerTypes = typeof(Character).Assembly.GetTypes();
                }
                return _AllChummerTypes;
            }
        }

        public static Dictionary<int, object> MyReflectionCollection { get; set; }

        /// <summary>
        /// This function searches recursivly through the Object "obj" and generates Tags for each
        /// property found with an HubTag-Attribute.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="parenttag"></param>
        /// <returns>A list of Tags (that may have a lot of child-Tags as well).</returns>

        internal static IList<Tag> ExtractTagsFromAttributes(Object obj, Tag parenttag)
        {
            List<Tag> resulttags = new List<Tag>();
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
                            var tag = new Tag(item, classprop.Item1);
                            tag.ParentTagId = parenttag.TagId;
                            tag.MyParentTag = parenttag;
                            parenttag.Tags.Add(tag);
                            resulttags.Add(tag);
                            tag.MyRuntimeHubClassTag = classprop.Item1;
                            //tag.TagName = classprop.Item1.ListName;
                            tag.TagType = "list";
                            if (!String.IsNullOrEmpty(classprop.Item1.ListInstanceNameFromProperty))
                            {
                                var childprop = from p in item.GetType().GetProperties()
                                                where p.Name == classprop.Item1.ListInstanceNameFromProperty
                                                select p;
                                if (!childprop.Any())
                                    throw new ArgumentOutOfRangeException("Could not find property " + classprop.Item1.ListInstanceNameFromProperty + " on instance of type " + item.GetType().ToString() + ".");
                                tag.TagName += childprop.FirstOrDefault().GetValue(item);
                            }
                            if (String.IsNullOrEmpty(tag.TagName))
                                tag.TagName = item.ToString();
                            
                            //Add complex Object
                            var childtags = ExtractTagsFromAttributes(item, tag);
                            if (classprop.Item1.DeleteEmptyTags)
                            {
                                if (!tag.Tags.Any())
                                {
                                    parenttag.Tags.Remove(tag);
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
                var proptags = ExtractTagsFromAttributesForProperty(prop, parenttag);
                resulttags.AddRange(proptags);
            }
            return resulttags;
        }

        internal static IList<Tag> ExtractTagsFromAttributesForProperty(Tuple<PropertyInfo, Chummer.helpers.HubTagAttribute, Object> prop, Tag parenttag)
        {
            List<Tag> proptaglist = new List<Tag>();
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


            var tag = new Tag(propValue, attribute);

            tag.Tags = new List<Tag>();
            tag.MyParentTag = parenttag;
            if (tag.MyParentTag != null)
                tag.MyParentTag.Tags.Add(tag);
            tag.ParentTagId = parenttag?.TagId;
            tag.TagId = Guid.NewGuid();
            if (!String.IsNullOrEmpty(attribute.TagName))
                tag.TagName = attribute.TagName;
            else if (prop.Item1 != null)
                tag.TagName = prop.Item1.Name;
            else
                tag.TagName = prop.Item3.ToString();
            Type t = prop.Item3.GetType();
            if (!String.IsNullOrEmpty(attribute.TagNameFromProperty))
            {
                var addObject = t.GetProperty(attribute.TagNameFromProperty).GetValue(prop.Item3, null);
                tag.TagName += String.Format("{0}", addObject);
            }
            tag.TagValue = String.Format("{0}", tag.MyRuntimeObject);
            Type typeValue = tag.MyRuntimeObject.GetType();
            if (typeof(int).IsAssignableFrom(typeValue))
            {
                tag.TagType = "int";
            }
            else if (typeof(double).IsAssignableFrom(typeValue))
            {
                tag.TagType = "double";
            }
            else if (typeof(bool).IsAssignableFrom(typeValue))
            {
                tag.TagType = "bool";
            }
            else if (typeof(string).IsAssignableFrom(typeValue))
            {
                tag.TagType = "string";
            }
            else if (typeof(Guid).IsAssignableFrom(typeValue))
            {
                tag.TagType = "Guid";
            }
            else
            {
                tag.TagType = "other";
            }
            if (tag.TagValue == typeValue.FullName)
                tag.TagValue = "";
            if ((typeof(IEnumerable).IsAssignableFrom(typeValue)
                || typeof(ICollection).IsAssignableFrom(typeValue))
                && !typeof(string).IsAssignableFrom(typeValue))
            {
                tag.TagType = "list";
                tag.TagValue = "";
            }
            if (!String.IsNullOrEmpty(attribute.TagValueFromProperty))
            {
                var addObject = t.GetProperty(attribute.TagValueFromProperty).GetValue(prop.Item3, null);
                tag.TagValue = String.Format("{0}", addObject);
            }
            proptaglist.Add(tag);
            if (prop.Item1 != null)
            {
                var childlist = ExtractTagsFromAttributes(tag.MyRuntimeObject, tag);
                proptaglist.AddRange(childlist);
            }
            if (attribute.DeleteIfEmpty)
            {
                if (!tag.Tags.Any() && String.IsNullOrEmpty(tag.TagValue))
                {
                    tag.MyParentTag.Tags.Remove(tag);
                }
            }
            return proptaglist;
        }

        internal static IList<Tag> ExtractTags(Object obj, int level, Tag parenttag)
        {
            if (MyReflectionCollection == null)
                MyReflectionCollection = new Dictionary<int, object>();
            List<Tag> resulttags = new List<Tag>();
            if (obj == null)
                return resulttags;
            if (MyReflectionCollection.ContainsKey(obj.GetHashCode()))
                return resulttags;
            MyReflectionCollection.Add(obj.GetHashCode(), obj);

            var type = obj.GetType();
            if (!AllChummerTypes.Contains(type))
                return resulttags;
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                object propValue = null;
                try
                {
                    propValue = property.GetValue(obj, null);
                }
                catch(Exception ex)
                {
                    System.Diagnostics.Debugger.Break();
                    continue;
                }
                if (propValue == null)
                    continue;
                if (String.IsNullOrEmpty(propValue.ToString()))
                    continue;
               
                var elems = propValue as IList;
                if (elems != null)
                {
                    foreach (var item in elems)
                    {
                        var itemtags = ExtractTags(item, level-1, parenttag);
                        if (parenttag == null)
                            resulttags.AddRange(itemtags);
                    }
                }
                else
                {
                   
                    //check if the propValue is a parent
                    Tag tempParent = parenttag;
                    bool found = false;
                    while(tempParent != null)
                    {
                        if (tempParent.MyRuntimeObject == propValue)
                        {
                            found = true;
                            break;
                        }
                        tempParent = tempParent.MyParentTag;
                    }
                    if (found)
                        continue;

                    var propValue1 = property.GetValue(obj);
                    var tag = new Tag();
                    
                    tag.Tags = new List<Tag>();
                    tag.MyParentTag = parenttag;
                    if (tag.MyParentTag != null)
                        tag.MyParentTag.Tags.Add(tag);
                    tag.ParentTagId = parenttag?.TagId;
                    tag.TagId = Guid.NewGuid();
                    tag.TagName = property.Name;
                
                    tag.MyRuntimeObject = propValue1;
                    tag.TagValue = String.Format("{0}", propValue1);
                    if (level > 0)
                    {
                        var childtags = ExtractTags(propValue, level-1, tag);
                    }
                    if ((tag.Tags.Count() == 0) && (String.IsNullOrEmpty(tag.TagValue)))
                    {
                        tag.MyParentTag.Tags.Remove(tag);
                    }
                    if (parenttag == null)
                        resulttags.Add(tag);
                }
            }
            return resulttags;
        }
    }
}
