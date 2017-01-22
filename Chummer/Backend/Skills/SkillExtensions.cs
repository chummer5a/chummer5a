using System.Collections.Generic;
using System.ComponentModel;

namespace Chummer.Skills
{
	public static class SkillExtensions
	{
		public static bool HasSpecialization(this Skill skill, string specialization)
		{
		    foreach (SkillSpecialization objLoopSpecialization in skill.Specializations)
		    {
		        if (objLoopSpecialization.Name == specialization || objLoopSpecialization.DisplayName == specialization)
		            return true;
		    }
			return false;
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
		    int intReturn = 0;
		    foreach (Skill objLoopSkill in list)
		    {
		        intReturn += objLoopSkill.CurrentSpCost();
		    }
			return intReturn;
		}

		public static int TotalCostKarma(this IEnumerable<Skill> list)
		{
            int intReturn = 0;
            foreach (Skill objLoopSkill in list)
            {
                intReturn += objLoopSkill.CurrentKarmaCost();
            }
            return intReturn;
		}

		public static int TotalCostSp(this IEnumerable<SkillGroup> list)
		{
            int intReturn = 0;
            foreach (SkillGroup objLoopSkillGroup in list)
            {
                intReturn += objLoopSkillGroup.CurrentSpCost();
            }
            return intReturn;
		}

		public static int TotalCostKarma(this IEnumerable<SkillGroup> list)
		{
            int intReturn = 0;
            foreach (SkillGroup objLoopSkillGroup in list)
            {
                intReturn += objLoopSkillGroup.CurrentKarmaCost();
            }
            return intReturn;
        }

	}
}
