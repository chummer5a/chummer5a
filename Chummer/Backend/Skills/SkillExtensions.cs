using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Chummer.Skills
{
    public static class SkillExtensions
    {
        public static bool HasSpecialization(this Skill skill, string specialization)
        {
            return skill.Specializations.Any(x => x.Name == specialization || x.DisplayName == specialization);
        }

        public static string GetDisplayName(this Skill skill)
        {
            return skill.DisplayName;
        }

        public static string GetDisplayCategory(this Skill skill)
        {
            return skill.DisplayCategory;
        }

        public static int TotalCostSp(this IEnumerable<Skill> list)
        {
            return list.Sum(skill => skill.CurrentSpCost());
        }

        public static int TotalCostKarma(this IEnumerable<Skill> list)
        {
            return list.Sum(skill => skill.CurrentKarmaCost());
        }

        public static int TotalCostSp(this IEnumerable<SkillGroup> list)
        {
            return list.Sum(skill => skill.CurrentSpCost());
        }

        public static int TotalCostKarma(this IEnumerable<SkillGroup> list)
        {
            return list.Sum(skill => skill.CurrentKarmaCost());
        }

    }
}
