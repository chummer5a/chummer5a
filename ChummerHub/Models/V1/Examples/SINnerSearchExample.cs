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
using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;

namespace ChummerHub.Models.V1.Examples
{

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerSearchExample'
    public class SINnerSearchExample : IExamplesProvider<object>
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerSearchExample'
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerSearchExample.SINnerSearchExample()'
        public SINnerSearchExample()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerSearchExample.SINnerSearchExample()'
        {

        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerSearchExample.GetExamples()'
        public object GetExamples()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerSearchExample.GetExamples()'
        {
            return GetSINnerSearchExample();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member 'SINnerSearchExample.GetSINnerSearchExample()'
        public SearchTag GetSINnerSearchExample()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member 'SINnerSearchExample.GetSINnerSearchExample()'
        {
            Guid parentTagGuid = Guid.NewGuid();
            var sin = new SearchTag
            {
                TagName = "Reflection",
                TagValue = "",
                SearchOpterator = SearchTag.TagOperatorEnum.notnull,
                Tags = new List<Tag>()
                {
                    new SearchTag ()
                    {
                         Tags = new List<Tag>(),
                         TagName = "AttributeSection",
                         SearchOpterator = SearchTag.TagOperatorEnum.exists
                    }
                }

            };
            return sin;
        }
    }

}
