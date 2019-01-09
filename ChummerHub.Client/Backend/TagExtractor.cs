using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
            List<Tuple<PropertyInfo, Chummer.HubTagAttribute, Object>> props = new List<Tuple<PropertyInfo, Chummer.HubTagAttribute, Object>>();
        
           
            var props2 = from p in obj.GetType().GetProperties()
                         let attr = p.GetCustomAttributes(typeof(Chummer.HubTagAttribute), true)
                         where attr.Length == 1
                         select new Tuple<PropertyInfo, Chummer.HubTagAttribute, Object>(p, attr.First() as Chummer.HubTagAttribute, obj);

            props.AddRange(props2);

            if (!typeof(String).IsAssignableFrom(obj.GetType()))
            {
                if (obj.GetType().Assembly.FullName.Contains("Chummer"))
                {
                    //check, if the type has an HubClassTagAttribute
                    List<Tuple<Chummer.HubClassTagAttribute, Object>> classprops = (from p in obj.GetType().GetCustomAttributes(typeof(Chummer.HubClassTagAttribute), true)
                                                                                            select new Tuple<Chummer.HubClassTagAttribute, Object>(p as Chummer.HubClassTagAttribute, obj)).ToList();
                    if (classprops.Any())
                    {
                        foreach (var classprop in classprops)
                        {
                            var tag = new Tag(obj, classprop.Item1);
                            tag.SiNnerId = parenttag.SiNnerId;
                            tag.ParentTagId = parenttag.Id;
                            tag.MyParentTag = parenttag;
                            parenttag.Tags.Add(tag);
                            resulttags.Add(tag);
                            tag.MyRuntimeHubClassTag = classprop.Item1;
                            //tag.TagName = classprop.Item1.ListName;
                            SetTagTypeEnumFromCLRType(tag, obj.GetType());
                            if (!String.IsNullOrEmpty(classprop.Item1.ListInstanceNameFromProperty))
                            {
                                tag.TagName = classprop.Item1.ListInstanceNameFromProperty;
                                var childprop = from p in obj.GetType().GetProperties()
                                                where p.Name == classprop.Item1.ListInstanceNameFromProperty
                                                select p;
                                if (!childprop.Any())
                                    throw new ArgumentOutOfRangeException("Could not find property " + classprop.Item1.ListInstanceNameFromProperty + " on instance of type " + obj.GetType().ToString() + ".");
                                tag.TagValue += childprop.FirstOrDefault().GetValue(obj);
                            }
                            if (String.IsNullOrEmpty(tag.TagName))
                                tag.TagName = obj.ToString();
                            ExtractTagsAddIncludeProperties(obj, resulttags, classprop, tag);
                        }
                        return resulttags;
                    }

                }
                IEnumerable islist = obj as IEnumerable;
       
                if (islist == null)
                {
                    islist = obj as ICollection;
                }
                if (islist != null)
                {
                    int counter = 0;
                    foreach (var item in islist)
                    {
                        counter++;
                        List<Tuple<Chummer.HubClassTagAttribute, Object>> classprops = (from p in item.GetType().GetCustomAttributes(typeof(Chummer.HubClassTagAttribute), true)
                                                                                                select new Tuple<Chummer.HubClassTagAttribute, Object>(p as Chummer.HubClassTagAttribute, obj)).ToList();
                        foreach (var classprop in classprops)
                        {
                            var tag = new Tag(item, classprop.Item1);
                            tag.SiNnerId = parenttag.SiNnerId;
                            tag.ParentTagId = parenttag.Id;
                            tag.MyParentTag = parenttag;
                            parenttag.Tags.Add(tag);
                            resulttags.Add(tag);
                            tag.MyRuntimeHubClassTag = classprop.Item1;
                            //tag.TagName = classprop.Item1.ListName;
                            tag.TagType = "list";
                            if (!String.IsNullOrEmpty(classprop.Item1.ListInstanceNameFromProperty))
                            {
                                tag.TagName = classprop.Item1.ListInstanceNameFromProperty;
                                var childprop = from p in item.GetType().GetProperties()
                                                where p.Name == classprop.Item1.ListInstanceNameFromProperty
                                                select p;
                                if (!childprop.Any())
                                    throw new ArgumentOutOfRangeException("Could not find property " + classprop.Item1.ListInstanceNameFromProperty + " on instance of type " + item.GetType().ToString() + ".");
                                tag.TagValue += childprop.FirstOrDefault().GetValue(item);
                            }
                            if (String.IsNullOrEmpty(tag.TagName))
                                tag.TagName = item.ToString();

                            ExtractTagsAddIncludeProperties(item, resulttags, classprop, tag);
                        }
                    }
                    if (counter == 0)
                    {
                        //this whole tree is empty - remove it!
                        parenttag.MyParentTag.Tags.Remove(parenttag);
                        return null;
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

        private static void ExtractTagsAddIncludeProperties(object obj, List<Tag> resulttags, Tuple<Chummer.HubClassTagAttribute, object> classprop, Tag tag)
        {
            //add the TagComment
            foreach (string includeprop in classprop.Item1.ListCommentProperties)
            {
                var propfoundseq = from p in obj.GetType().GetProperties()
                                   where p.Name == includeprop
                                   select p;
                if (!propfoundseq.Any())
                {
                    throw new ArgumentOutOfRangeException("Could not find property " + includeprop + " on instance of type " + obj.GetType().ToString() + ".");
                }
                var includeInstance = propfoundseq.FirstOrDefault().GetValue(obj);
                if (includeInstance != null)
                {
                    tag.TagComment += includeInstance.ToString() + " ";
                }
            }
            tag.TagComment = tag.TagComment.TrimEnd(" ");
            //add the "Extra" to this Instance
            foreach(string includeprop in classprop.Item1.ListExtraProperties)
            {
                var propfoundseq = from p in obj.GetType().GetProperties()
                                   where p.Name == includeprop
                                   select p;
                if(!propfoundseq.Any())
                {
                    throw new ArgumentOutOfRangeException("Could not find property " + includeprop + " on instance of type " + obj.GetType().ToString() + ".");
                }
                var includeInstance = propfoundseq.FirstOrDefault().GetValue(obj);
                if(includeInstance != null && !String.IsNullOrEmpty(includeInstance.ToString()))
                {
                    var instanceTag = new Tag(includeInstance, classprop.Item1);
                    instanceTag.SiNnerId = tag.SiNnerId;
                    instanceTag.ParentTagId = tag.Id;
                    instanceTag.MyParentTag = tag;
                    tag.Tags.Add(instanceTag);
                    resulttags.Add(instanceTag);
                    instanceTag.MyRuntimeHubClassTag = classprop.Item1;
                    instanceTag.TagName = includeprop;
                    SetTagTypeEnumFromCLRType(instanceTag, obj.GetType());
                    instanceTag.TagValue = includeInstance.ToString();
                }
            }
            
        }

        internal static IList<Tag> ExtractTagsFromAttributesForProperty(Tuple<PropertyInfo, Chummer.HubTagAttribute, Object> prop, Tag parenttag)
        {
            List<Tag> proptaglist = new List<Tag>();
            Chummer.HubTagAttribute attribute = prop.Item2 as Chummer.HubTagAttribute;
            Object propValue;
            PropertyInfo property = prop.Item1 as PropertyInfo;
            if(property == null)
            {
                propValue = prop.Item3;
            }
            else
            {
                propValue = property.GetValue(prop.Item3);
                if(propValue.GetType().IsAssignableFrom(typeof(bool)))
                {
                    if(propValue as bool? == false)
                    { //dont save "false" values
                        return proptaglist;
                    }
                }
                if(propValue.GetType().IsAssignableFrom(typeof(int)))
                {
                    if(propValue as int? == 0)
                    {   //dont save "0" values
                        return proptaglist;
                    }
                }
            }


            var tag = new Tag(propValue, attribute);
            tag.SiNnerId = parenttag?.SiNnerId;
            tag.Tags = new List<Tag>();
            tag.MyParentTag = parenttag;
            if(tag.MyParentTag != null)
                tag.MyParentTag.Tags.Add(tag);
            tag.ParentTagId = parenttag?.Id;
            tag.Id = Guid.NewGuid();
            if(!String.IsNullOrEmpty(attribute.TagName))
                tag.TagName = attribute.TagName;
            else if(prop.Item1 != null)
                tag.TagName = prop.Item1.Name;
            else
                tag.TagName = prop.Item3.ToString();

            Type t = prop.Item3.GetType();
            if(!String.IsNullOrEmpty(attribute.TagNameFromProperty))
            {
                var addObject = t.GetProperty(attribute.TagNameFromProperty).GetValue(prop.Item3, null);
                tag.TagName += String.Format("{0}", addObject);
            }
            tag.TagValue = String.Format("{0}", tag.MyRuntimeObject);
            Type typeValue = tag.MyRuntimeObject.GetType();
            SetTagTypeEnumFromCLRType(tag, typeValue);
            if(!String.IsNullOrEmpty(attribute.TagValueFromProperty))
            {
                var addObject = t.GetProperty(attribute.TagValueFromProperty).GetValue(prop.Item3, null);
                tag.TagValue = String.Format("{0}", addObject);
            }
            proptaglist.Add(tag);
            if(prop.Item1 != null)
            {
                var childlist = ExtractTagsFromAttributes(tag.MyRuntimeObject, tag);
                if(childlist != null)
                {
                    proptaglist.AddRange(childlist);
                }
            }
            if(attribute.DeleteIfEmpty)
            {
                if(!tag.Tags.Any() && String.IsNullOrEmpty(tag.TagValue))
                {
                    tag.MyParentTag.Tags.Remove(tag);
                }
            }
            return proptaglist;
        }

        private static void SetTagTypeEnumFromCLRType(Tag tag, Type typeValue)
        {
            if(typeof(int).IsAssignableFrom(typeValue))
            {
                tag.TagType = "int";
            }
            else if(typeof(double).IsAssignableFrom(typeValue))
            {
                tag.TagType = "double";
            }
            else if(typeof(bool).IsAssignableFrom(typeValue))
            {
                tag.TagType = "bool";
            }
            else if(typeof(string).IsAssignableFrom(typeValue))
            {
                tag.TagType = "string";
            }
            else if(typeof(Guid).IsAssignableFrom(typeValue))
            {
                tag.TagType = "Guid";
            }
            else
            {

                tag.TagType = "other";
            }
            if(tag.TagValue == typeValue.FullName)
                tag.TagValue = "";
            if((typeof(IEnumerable).IsAssignableFrom(typeValue)
                || typeof(ICollection).IsAssignableFrom(typeValue))
                && !typeof(string).IsAssignableFrom(typeValue))
            {
                tag.TagType = "list";
                tag.TagValue = "";
            }
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
                    tag.ParentTagId = parenttag?.Id;
                    tag.Id = Guid.NewGuid();
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
