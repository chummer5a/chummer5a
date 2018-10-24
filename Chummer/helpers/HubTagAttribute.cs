using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chummer.helpers
{
    [AttributeUsage(AttributeTargets.Class)]
    public class HubClassTagAttribute : System.Attribute
    {
        //private string _ListName;
        private string _ListInstanceNameFromProperty;
        private bool _DeleteEmptyTags = false;

        

        public HubClassTagAttribute(string listInstanceNameFromProperty, bool deleteEmptyTags = false)
        {
            //_ListName = ListName;
            _ListInstanceNameFromProperty = listInstanceNameFromProperty;
            _DeleteEmptyTags = deleteEmptyTags;
        }

        //public string ListName
        //{ get { return _ListName; } }

        public string ListInstanceNameFromProperty
        { get { return _ListInstanceNameFromProperty; } }
        public bool DeleteEmptyTags
        { get { return _DeleteEmptyTags; } }

    }

    [AttributeUsage(AttributeTargets.Property)]
    public class HubTagAttribute : System.Attribute
    {
        private string _TagName;
        private string _TagValueFromProperty;
        private string _TagNameFromProperty;
        private bool _deleteIfEmpty = false;

        public HubTagAttribute()
        {

        }

        public HubTagAttribute(
                  string TagName,
                  string TagNameFromProperty,
                  string TagValueFromProperty,
                  bool deleteIfEmpty)
        {
            this._TagName = TagName;
            this._TagNameFromProperty = TagNameFromProperty;
            this._TagValueFromProperty = TagValueFromProperty;
            this._deleteIfEmpty = deleteIfEmpty;
        }


        public HubTagAttribute(
          string TagName,
          string TagNameFromProperty)
        {
            this._TagName = TagName;
            this._TagNameFromProperty = TagNameFromProperty;
        }

        public HubTagAttribute(
          string TagName)
        {
            this._TagName = TagName;
        }

        public HubTagAttribute(
          bool deleteIfEmpty)
        {
            this._deleteIfEmpty = deleteIfEmpty;
        }

        public string TagName
        { get { return _TagName; } }

        public string TagNameFromProperty
        { get { return _TagNameFromProperty; } }

        public string TagValueFromProperty
        { get { return _TagValueFromProperty; } }

        public bool DeleteIfEmpty
        { get { return _deleteIfEmpty; } }




    }
}
