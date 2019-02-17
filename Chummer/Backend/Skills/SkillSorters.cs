using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chummer.Skills;

namespace Chummer.Backend.Skills
{
    class SkillSorter : IComparer<Skill>
    {
        private readonly Comparison<Skill> _comparison;

        public SkillSorter(Comparison<Skill> comparison)
        {
            if (comparison == null) throw new ArgumentNullException(nameof(comparison));

            _comparison = comparison;
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

    class KnowledgeSkillSorter : IComparer<KnowledgeSkill>
    {
        private readonly Comparison<KnowledgeSkill> _comparison;

        public KnowledgeSkillSorter(Comparison<KnowledgeSkill> comparison)
        {
            if (comparison == null) throw new ArgumentNullException(nameof(comparison));

            _comparison = comparison;
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

    class SkillSortBySkillGroup : IComparer<Skill>
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
            if (x.SkillGroupObject != null)
            {
                if (y.SkillGroupObject != null)
                {
                    return y.SkillGroupObject.Rating.CompareTo(x.SkillGroupObject.Rating);
                }
                else
                {
                    return -1;
                }
            }
            else if (y.SkillGroupObject != null)
            {
                return 1;
            }
            return 0;
        }
    }
}
