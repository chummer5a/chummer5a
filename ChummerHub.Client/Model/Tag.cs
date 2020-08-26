using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using Chummer;
using Newtonsoft.Json;

namespace SINners.Models
{
    [DebuggerDisplay("{" + nameof(Display) + "}")]
    public partial class Tag
    {
        [IgnoreDataMember]
        [XmlIgnore]
        [JsonIgnore]
        public Tag MyParentTag { get; set; }

        public Tag (object myRuntimeObject, HubTagAttribute hubTag)
        {
            Id = Guid.NewGuid();
            MyRuntimeObject = myRuntimeObject;
            MyRuntimeHubTag = hubTag;
        }

        public Tag (bool isUserGenerated)
        {
            Id = Guid.NewGuid();
            IsUserGenerated = isUserGenerated;
        }

        public Tag(object myRuntimeObject, HubClassTagAttribute hubClassTag)
        {
            Id = Guid.NewGuid();
            MyRuntimeObject = myRuntimeObject;
            MyRuntimeHubClassTag = hubClassTag;
        }

        [IgnoreDataMember]
        [XmlIgnore]
        [JsonIgnore]
        public string Display
        {
            get
            {
                StringBuilder sbdReturn = new StringBuilder(TagName);
                if (!string.IsNullOrEmpty(TagValue))
                    sbdReturn.Append(": " + TagValue);
                Tag tempParent = MyParentTag;
                while (tempParent != null)
                {
                    string tempstr = tempParent.TagName;
                    if (!string.IsNullOrEmpty(tempParent.TagValue))
                        tempstr += ": " + tempParent.TagValue;
                    sbdReturn.Insert(0, tempstr + " -> ");
                    tempParent = tempParent.MyParentTag;
                }
                if(!string.IsNullOrEmpty(TagComment))
                    sbdReturn.Append(" (" + TagComment + ")");
                return sbdReturn.ToString();
            }
        }

        [IgnoreDataMember]
        [XmlIgnore]
        [JsonIgnore]
        public bool DeleteIfEmpty { get; set; }

        [IgnoreDataMember]
        [XmlIgnore]
        [JsonIgnore]
        public object MyRuntimeObject { get; set; }

        [IgnoreDataMember]
        [XmlIgnore]
        [JsonIgnore]
        public HubTagAttribute MyRuntimeHubTag { get; set; }

        [IgnoreDataMember]
        [XmlIgnore]
        [JsonIgnore]
        public HubClassTagAttribute MyRuntimeHubClassTag { get; set; }

        internal void SetSinnerIdRecursive(Guid? id)
        {
            SiNnerId = id;
            if (Tags != null && Tags.Count > 0)
                foreach(var childtag in Tags)
                    childtag.SetSinnerIdRecursive(id);
        }

        internal void SetTagTypeEnumFromCLRType(Type typeValue)
        {
            if (typeof(int).IsAssignableFrom(typeValue))
            {
                TagType = "int";
            }
            else if (typeof(double).IsAssignableFrom(typeValue))
            {
                TagType = "double";
            }
            else if (typeof(bool).IsAssignableFrom(typeValue))
            {
                TagType = "bool";
            }
            else if (typeof(string).IsAssignableFrom(typeValue))
            {
                TagType = "string";
            }
            else if (typeof(Guid).IsAssignableFrom(typeValue))
            {
                TagType = "Guid";
            }
            else
            {
                TagType = "other";
            }

            if (typeof(IEnumerable).IsAssignableFrom(typeValue) && !typeof(string).IsAssignableFrom(typeValue))
            {
                TagType = "list";
                TagValue = string.Empty;
                TagValueFloat = null;
            }
            else if (TagValue == typeValue.FullName)
            {
                TagValue = string.Empty;
                TagValueFloat = null;
            }
        }

        internal void AddPropertyValuesToTagComment(object objItem, HubClassTagAttribute objAttribute)
        {
            if (objItem == null || objAttribute == null)
                return;
            Type objItemType = objItem.GetType();
            StringBuilder sbdPropertyValues = new StringBuilder();
            foreach (string strProperty in objAttribute.ListCommentProperties)
            {
                PropertyInfo objProperty = objItemType.GetProperties().FirstOrDefault(p => p.Name == strProperty);
                if (objProperty == null)
                    throw new ArgumentOutOfRangeException("Could not find property " + strProperty + " on instance of type " + objItemType + ".");

                string strPropertyValue = objProperty.GetValue(objItem)?.ToString();
                if (!string.IsNullOrEmpty(strPropertyValue))
                    sbdPropertyValues.Append(strPropertyValue + " ");
            }
            if (sbdPropertyValues.Length > 0)
                sbdPropertyValues.Remove(sbdPropertyValues.Length - 1, 1); // Remove trailing whitespace
            TagComment += sbdPropertyValues;
        }
    }
}
