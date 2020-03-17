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
            ListInstanceNameFromProperty = listInstanceNameFromProperty;
            DeleteEmptyTags = deleteEmptyTags;
            if(!string.IsNullOrEmpty(commentProperties))
                ListCommentProperties = new List<string>(commentProperties.Split(';'));
            if(!string.IsNullOrEmpty(extraProperties))
                ListExtraProperties = new List<string>(extraProperties.Split(';'));
        }

        public List<string> ListCommentProperties { get; } = new List<string>();

        public List<string> ListExtraProperties { get; } = new List<string>();


        public string ListInstanceNameFromProperty { get; }

        public bool DeleteEmptyTags { get; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class HubTagAttribute : System.Attribute
    {
        public HubTagAttribute()
        {

        }

        public HubTagAttribute(string TagName, string TagNameFromProperty, string TagValueFromProperty, bool deleteIfEmpty)
        {
            this.TagName = TagName;
            this.TagNameFromProperty = TagNameFromProperty;
            this.TagValueFromProperty = TagValueFromProperty;
            DeleteIfEmpty = deleteIfEmpty;
        }


        public HubTagAttribute(string TagName, string TagNameFromProperty)
        {
            this.TagName = TagName;
            this.TagNameFromProperty = TagNameFromProperty;
        }

        public HubTagAttribute(string TagName)
        {
            this.TagName = TagName;
        }

        public HubTagAttribute(bool deleteIfEmpty)
        {
            DeleteIfEmpty = deleteIfEmpty;
        }

        public string TagName { get; }

        public string TagNameFromProperty { get; }

        public string TagValueFromProperty { get; }

        public bool DeleteIfEmpty { get; }
    }
}
