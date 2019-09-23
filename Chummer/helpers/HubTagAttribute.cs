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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chummer
{
    /// <summary>
    /// How should instances of this Class be tagged?
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class HubClassTagAttribute : System.Attribute
    {
        //private string _ListName;
        private string _ListInstanceNameFromProperty;
        private bool _DeleteEmptyTags = false;
        private List<string> _CommentProperties = new List<string>();
        private List<string> _ExtraProperties = new List<string>();


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="listInstanceNameFromProperty"></param>
        /// <param name="deleteEmptyTags"></param>
        /// <param name="commentProperties">a list of Properties to tag - delimiter is ";"</param>
        /// <param name="extraProperties"></param>
        public HubClassTagAttribute(string listInstanceNameFromProperty, bool deleteEmptyTags, string commentProperties, string extraProperties)
        {
            //_ListName = ListName;
            _ListInstanceNameFromProperty = listInstanceNameFromProperty;
            _DeleteEmptyTags = deleteEmptyTags;
            if(!String.IsNullOrEmpty(commentProperties))
                _CommentProperties = new List<string>(commentProperties.Split(';'));
            if(!String.IsNullOrEmpty(extraProperties))
                _ExtraProperties = new List<string>(extraProperties.Split(';'));
        }

        public List<string> ListCommentProperties { get { return _CommentProperties; } }

        public List<string> ListExtraProperties { get { return _ExtraProperties; } }


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
