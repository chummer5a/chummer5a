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
using System.Threading;
using System.Threading.Tasks;

namespace Chummer.Backend.Skills
{
    public sealed class SkillSorter : IComparer<Skill>
    {
        private readonly Comparison<Skill> _comparison;

        public SkillSorter(Comparison<Skill> comparison)
        {
            _comparison = comparison ?? throw new ArgumentNullException(nameof(comparison));
        }

        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <returns>
        /// A signed integer that indicates the relative values of <paramref name="x"/> and <paramref name="y"/>, as shown in the following table.Value Meaning Less than zero<paramref name="x"/> is less than <paramref name="y"/>.Zero<paramref name="x"/> equals <paramref name="y"/>.Greater than zero<paramref name="x"/> is greater than <paramref name="y"/>.
        /// </returns>
        /// <param name="x">The first object to compare.</param><param name="y">The second object to compare.</param>
        public int Compare(Skill x, Skill y)
        {
            return _comparison(x, y);
        }
    }

    public sealed class KnowledgeSkillSorter : IComparer<KnowledgeSkill>
    {
        private readonly Comparison<KnowledgeSkill> _comparison;

        public KnowledgeSkillSorter(Comparison<KnowledgeSkill> comparison)
        {
            _comparison = comparison ?? throw new ArgumentNullException(nameof(comparison));
        }

        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <returns>
        /// A signed integer that indicates the relative values of <paramref name="x"/> and <paramref name="y"/>, as shown in the following table.Value Meaning Less than zero<paramref name="x"/> is less than <paramref name="y"/>.Zero<paramref name="x"/> equals <paramref name="y"/>.Greater than zero<paramref name="x"/> is greater than <paramref name="y"/>.
        /// </returns>
        /// <param name="x">The first object to compare.</param><param name="y">The second object to compare.</param>
        public int Compare(KnowledgeSkill x, KnowledgeSkill y)
        {
            return _comparison(x, y);
        }
    }

    public sealed class SkillGroupSorter : IComparer<SkillGroup>
    {
        private readonly Comparison<SkillGroup> _comparison;

        public SkillGroupSorter(Comparison<SkillGroup> comparison)
        {
            _comparison = comparison ?? throw new ArgumentNullException(nameof(comparison));
        }

        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <returns>
        /// A signed integer that indicates the relative values of <paramref name="x"/> and <paramref name="y"/>, as shown in the following table.Value Meaning Less than zero<paramref name="x"/> is less than <paramref name="y"/>.Zero<paramref name="x"/> equals <paramref name="y"/>.Greater than zero<paramref name="x"/> is greater than <paramref name="y"/>.
        /// </returns>
        /// <param name="x">The first object to compare.</param><param name="y">The second object to compare.</param>
        public int Compare(SkillGroup x, SkillGroup y)
        {
            return _comparison(x, y);
        }
    }

    public sealed class SkillSortBySkillGroup : IComparer<Skill>
    {
        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <returns>
        /// A signed integer that indicates the relative values of <paramref name="x"/> and <paramref name="y"/>, as shown in the following table.Value Meaning Less than zero<paramref name="x"/> is less than <paramref name="y"/>.Zero<paramref name="x"/> equals <paramref name="y"/>.Greater than zero<paramref name="x"/> is greater than <paramref name="y"/>.
        /// </returns>
        /// <param name="x">The first object to compare.</param><param name="y">The second object to compare.</param>
        public int Compare(Skill x, Skill y)
        {
            int intReturn;
            if (x?.SkillGroupObject != null)
                intReturn = y?.SkillGroupObject?.Rating.CompareTo(x.SkillGroupObject.Rating) ?? -1;
            else if (y?.SkillGroupObject != null)
                intReturn = 1;
            else
                intReturn = 0;
            if (intReturn == 0)
            {
                intReturn = SkillsSection.CompareSkillGroups(x?.SkillGroupObject, y?.SkillGroupObject);
                if (intReturn == 0)
                    intReturn = SkillsSection.CompareSkills(x, y);
            }

            return intReturn;
        }
    }

    public sealed class AsyncSkillSorter : IAsyncComparer<Skill>
    {
        private readonly Func<Skill, Skill, CancellationToken, Task<int>> _comparison;

        public AsyncSkillSorter(Func<Skill, Skill, CancellationToken, Task<int>> comparison)
        {
            _comparison = comparison ?? throw new ArgumentNullException(nameof(comparison));
        }

        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <returns>
        /// A signed integer that indicates the relative values of <paramref name="x"/> and <paramref name="y"/>, as shown in the following table.Value Meaning Less than zero<paramref name="x"/> is less than <paramref name="y"/>.Zero<paramref name="x"/> equals <paramref name="y"/>.Greater than zero<paramref name="x"/> is greater than <paramref name="y"/>.
        /// </returns>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public Task<int> CompareAsync(Skill x, Skill y, CancellationToken token = default)
        {
            return _comparison(x, y, token);
        }
    }

    public sealed class AsyncKnowledgeSkillSorter : IAsyncComparer<KnowledgeSkill>
    {
        private readonly Func<KnowledgeSkill, KnowledgeSkill, CancellationToken, Task<int>> _comparison;

        public AsyncKnowledgeSkillSorter(Func<KnowledgeSkill, KnowledgeSkill, CancellationToken, Task<int>> comparison)
        {
            _comparison = comparison ?? throw new ArgumentNullException(nameof(comparison));
        }

        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <returns>
        /// A signed integer that indicates the relative values of <paramref name="x"/> and <paramref name="y"/>, as shown in the following table.Value Meaning Less than zero<paramref name="x"/> is less than <paramref name="y"/>.Zero<paramref name="x"/> equals <paramref name="y"/>.Greater than zero<paramref name="x"/> is greater than <paramref name="y"/>.
        /// </returns>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public Task<int> CompareAsync(KnowledgeSkill x, KnowledgeSkill y, CancellationToken token = default)
        {
            return _comparison(x, y, token);
        }
    }

    public sealed class AsyncSkillGroupSorter : IAsyncComparer<SkillGroup>
    {
        private readonly Func<SkillGroup, SkillGroup, CancellationToken, Task<int>> _comparison;

        public AsyncSkillGroupSorter(Func<SkillGroup, SkillGroup, CancellationToken, Task<int>> comparison)
        {
            _comparison = comparison ?? throw new ArgumentNullException(nameof(comparison));
        }

        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <returns>
        /// A signed integer that indicates the relative values of <paramref name="x"/> and <paramref name="y"/>, as shown in the following table.Value Meaning Less than zero<paramref name="x"/> is less than <paramref name="y"/>.Zero<paramref name="x"/> equals <paramref name="y"/>.Greater than zero<paramref name="x"/> is greater than <paramref name="y"/>.
        /// </returns>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public Task<int> CompareAsync(SkillGroup x, SkillGroup y, CancellationToken token = default)
        {
            return _comparison(x, y, token);
        }
    }

    public sealed class AsyncSkillSortBySkillGroup : IAsyncComparer<Skill>
    {
        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <returns>
        /// A signed integer that indicates the relative values of <paramref name="x"/> and <paramref name="y"/>, as shown in the following table.Value Meaning Less than zero<paramref name="x"/> is less than <paramref name="y"/>.Zero<paramref name="x"/> equals <paramref name="y"/>.Greater than zero<paramref name="x"/> is greater than <paramref name="y"/>.
        /// </returns>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async Task<int> CompareAsync(Skill x, Skill y, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int intReturn;
            SkillGroup objLeftGroup = x?.SkillGroupObject;
            SkillGroup objRightGroup = y?.SkillGroupObject;
            if (objLeftGroup != null)
            {
                if (objRightGroup != null)
                    intReturn = (await objRightGroup.GetRatingAsync(token).ConfigureAwait(false)).CompareTo(await objLeftGroup.GetRatingAsync(token).ConfigureAwait(false));
                else
                    intReturn = -1;
            }
            else if (objRightGroup != null)
                intReturn = 1;
            else
                intReturn = 0;
            if (intReturn == 0)
            {
                intReturn = await SkillsSection.CompareSkillGroupsAsync(objLeftGroup, objRightGroup, token).ConfigureAwait(false);
                if (intReturn == 0)
                    intReturn = await SkillsSection.CompareSkillsAsync(x, y, token).ConfigureAwait(false);
            }

            return intReturn;
        }
    }
}
