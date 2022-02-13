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

using System.Collections.Generic;
using System.Linq;

namespace Chummer.Backend.Skills
{
    public static class SkillExtensions
    {
        public static int TotalCostSp(this IEnumerable<Skill> list)
        {
            return list.Sum(skill => skill.CurrentSpCost);
        }

        public static int TotalCostKarma(this IEnumerable<Skill> list)
        {
            return list.Sum(skill => skill.CurrentKarmaCost);
        }

        public static int TotalCostSp(this IEnumerable<SkillGroup> list)
        {
            return list.Sum(skill => skill.CurrentSpCost);
        }

        public static int TotalCostKarma(this IEnumerable<SkillGroup> list)
        {
            return list.Sum(skill => skill.CurrentKarmaCost);
        }
    }
}
