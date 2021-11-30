using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chummer.Backend.Attributes;
using Chummer.Backend.Skills;

namespace Chummer.Backend.StaticMethods
{
    public static class StaticCharacterStrings
    {
        public static string FormComplexFormsBPString(Character objCharacter, string strPoints, string strColon, string strOf, int intFormsPointsUsed)
        {
            string strSpace = LanguageManager.GetString("String_Space");
            StringBuilder sbdComplexFormsBP = new StringBuilder();
            if (objCharacter.CFPLimit > 0)
            {
                sbdComplexFormsBP.Append(intFormsPointsUsed.ToString(GlobalSettings.CultureInfo)).Append(strOf).Append(objCharacter.CFPLimit.ToString(GlobalSettings.CultureInfo));
                if (intFormsPointsUsed > objCharacter.CFPLimit)
                {
                    sbdComplexFormsBP.Append(strColon).Append(strSpace)
                        .Append(((intFormsPointsUsed - objCharacter.CFPLimit)
                                 * objCharacter.ComplexFormKarmaCost)
                            .ToString(GlobalSettings.CultureInfo)).Append(strSpace).Append(strPoints);
                }
                return sbdComplexFormsBP.ToString();
            }

            sbdComplexFormsBP
                .Append(((intFormsPointsUsed - objCharacter.CFPLimit) * objCharacter.ComplexFormKarmaCost)
                    .ToString(GlobalSettings.CultureInfo)).Append(strSpace).Append(strPoints);


            return sbdComplexFormsBP.ToString();
        }

        public static string FormStrContactPoints(Character objCharacter, string strSpace, string strPoints,
            string strOf)
        {
            var intContactPoints = objCharacter.ContactPoints;
            var intPointsInContacts = CharacterCalculations.PointsInContacts(objCharacter);
            var intHighPlacesFriends = CharacterCalculations.NumberHighPlacesFriends(objCharacter);

            StringBuilder sbdContactPoints = new StringBuilder(objCharacter.ContactPointsUsed.ToString(GlobalSettings.CultureInfo));
            if (objCharacter.FriendsInHighPlaces)
            {
                sbdContactPoints.Append('/').Append(Math.Max(0, objCharacter.CHA.Value * 4 - intHighPlacesFriends).ToString(GlobalSettings.CultureInfo));
            }
            sbdContactPoints.Append(strOf).Append(intContactPoints.ToString(GlobalSettings.CultureInfo));
            if (objCharacter.FriendsInHighPlaces)
            {
                sbdContactPoints.Append('/').Append((objCharacter.CHA.Value * 4).ToString(GlobalSettings.CultureInfo));
            }
            if (intPointsInContacts > 0 || objCharacter.CHA.Value * 4 < intHighPlacesFriends)
            {
                sbdContactPoints.Append(strSpace).Append('(')
                    .Append(intPointsInContacts.ToString(GlobalSettings.CultureInfo)).Append(strSpace)
                    .Append(strPoints).Append(')'); ;
            }

            string strContactPoints = sbdContactPoints.ToString();
            return strContactPoints;
        }

        public static string MartialArtsBPToolTip(Character objCharacter, CharacterSettings objCharacterSettings, string strSpace)
        {
            StringBuilder sbdMartialArtsBPToolTip = new StringBuilder();

            foreach (MartialArt objMartialArt in objCharacter.MartialArts)
            {
                if (objMartialArt.IsQuality)
                    continue;
                int intLoopCost = objMartialArt.Cost;

                if (sbdMartialArtsBPToolTip.Length > 0)
                    sbdMartialArtsBPToolTip.AppendLine().Append(strSpace).Append('+').Append(strSpace);
                sbdMartialArtsBPToolTip.Append(objMartialArt.CurrentDisplayName).Append(strSpace).Append('(')
                    .Append(intLoopCost.ToString(GlobalSettings.CultureInfo)).Append(')');

                bool blnIsFirst = true;
                foreach (MartialArtTechnique objTechnique in objMartialArt.Techniques)
                {
                    if (blnIsFirst)
                    {
                        blnIsFirst = false;
                        continue;
                    }
                    intLoopCost = objCharacterSettings.KarmaTechnique;

                    sbdMartialArtsBPToolTip.AppendLine().Append(strSpace).Append('+').Append(strSpace)
                        .Append(objTechnique.CurrentDisplayName).Append(strSpace).Append('(')
                        .Append(intLoopCost.ToString(GlobalSettings.CultureInfo)).Append(')');
                }
            }


            return sbdMartialArtsBPToolTip.ToString();
        }

        public static string PositiveQualityTooltip(Character objCharacter, CharacterSettings objCharacterSettings, string strSpace)
        {
            StringBuilder sbdPositiveQualityTooltip = new StringBuilder();
            foreach (Quality objLoopQuality in objCharacter.Qualities.Where(q => q.ContributeToBP))
            {
                switch (objLoopQuality.Type)
                {
                    case QualityType.LifeModule:
                        sbdPositiveQualityTooltip.AppendFormat(
                            GlobalSettings.CultureInfo, "{0}{1}({2})", objLoopQuality.CurrentDisplayName,
                            strSpace, objLoopQuality.BP * objCharacterSettings.KarmaQuality).AppendLine();
                        break;

                    case QualityType.Positive:
                        sbdPositiveQualityTooltip.AppendFormat(
                            GlobalSettings.CultureInfo, "{0}{1}({2})", objLoopQuality.CurrentDisplayName,
                            strSpace, objLoopQuality.BP * objCharacterSettings.KarmaQuality).AppendLine();
                        break;
                }
            }

            if (objCharacter.Contacts.Any(x => x.EntityType == ContactType.Contact && x.IsGroup && !x.Free))
            {
                sbdPositiveQualityTooltip.AppendLine(LanguageManager.GetString("Label_GroupContacts"));
                foreach (Contact objGroupContact in objCharacter.Contacts.Where(x =>
                             x.EntityType == ContactType.Contact && x.IsGroup && !x.Free))
                {
                    string strNameToUse = objGroupContact.GroupName;
                    if (string.IsNullOrEmpty(strNameToUse))
                    {
                        strNameToUse = objGroupContact.Name;
                        if (string.IsNullOrEmpty(strNameToUse))
                            strNameToUse = LanguageManager.GetString("String_Unknown");
                    }
                    else if (!string.IsNullOrWhiteSpace(objGroupContact.Name))
                        strNameToUse += '/' + objGroupContact.Name;

                    sbdPositiveQualityTooltip.AppendFormat(GlobalSettings.CultureInfo, "{0}{1}({2})",
                        strNameToUse,
                        strSpace,
                        objGroupContact.ContactPoints * objCharacterSettings.KarmaContact).AppendLine();
                }
            }

            return sbdPositiveQualityTooltip.ToString();
        }

        public static string FormNegativeQualityTooltip(Character objCharacter, CharacterSettings objCharacterSettings, string strSpace)
        {
            StringBuilder sbdNegativeQualityTooltip = new StringBuilder();
            foreach (Quality objLoopQuality in objCharacter.Qualities.Where(q => q.ContributeToBP))
            {
                if (objLoopQuality.Type == QualityType.Negative)
                {
                    sbdNegativeQualityTooltip.AppendFormat(
                        GlobalSettings.CultureInfo, "{0}{1}({2})", objLoopQuality.CurrentDisplayName,
                        strSpace, objLoopQuality.BP * objCharacterSettings.KarmaQuality).AppendLine();
                }
            }

            return sbdNegativeQualityTooltip.ToString();
        }

        public static string FormAttributeCostString(Character objCharacter, ICollection<CharacterAttrib> attribs, ICollection<CharacterAttrib> extraAttribs = null, bool special = false)
        {
            int bp = CharacterCalculations.CalculateAttributeBP(attribs, extraAttribs);
            string s = bp.ToString(GlobalSettings.CultureInfo) + LanguageManager.GetString("String_Space") + LanguageManager.GetString("String_Karma");
            int att = CharacterCalculations.CalculateAttributePriorityPoints(objCharacter, attribs, extraAttribs);
            int total = special ? objCharacter.TotalSpecial : objCharacter.TotalAttributes;
            if (objCharacter.EffectiveBuildMethodUsesPriorityTables)
            {
                if (bp > 0)
                {
                    s = string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("String_OverPriorityPoints"),
                        total - att, total, bp);
                }
                else
                {
                    s = (total - att).ToString(GlobalSettings.CultureInfo) + LanguageManager.GetString("String_Of") + total.ToString(GlobalSettings.CultureInfo);
                }
            }
            return s;
        }

        public static string BuildFociBPTooltip(Character objCharacter, string strSpace)
        {
            StringBuilder sbdFociPointsTooltip = new StringBuilder();

            //Update the Focus UI
            foreach (Focus objFocus in objCharacter.Foci)
            {
                if (sbdFociPointsTooltip.Length > 0)
                    sbdFociPointsTooltip.AppendLine().Append(strSpace).Append('+').Append(strSpace);
                sbdFociPointsTooltip.Append(objFocus.GearObject.CurrentDisplayName).Append(strSpace).Append('(')
                    .Append(objFocus.BindingKarmaCost().ToString(GlobalSettings.CultureInfo))
                    .Append(')');
            }

            //Update the stacked Focus UI
            foreach (StackedFocus objFocus in objCharacter.StackedFoci)
            {
                if (!objFocus.Bonded)
                    continue;

                int intBindingCost = objFocus.BindingCost;

                if (sbdFociPointsTooltip.Length > 0)
                    sbdFociPointsTooltip.AppendLine().Append(strSpace).Append('+').Append(strSpace);
                sbdFociPointsTooltip.Append(objFocus.CurrentDisplayName).Append(strSpace).Append('(')
                    .Append(intBindingCost.ToString(GlobalSettings.CultureInfo)).Append(')');
            }

            return sbdFociPointsTooltip.ToString();
        }

        public static string SkillGroupBP(string strZeroKarma, string strOf, string strColon, string strSpace, string strKarma, Character character)
        {
            string strTemp = strZeroKarma;
            int intSkillGroupPointsMaximum = character.SkillsSection.SkillGroupPointsMaximum;
            if (intSkillGroupPointsMaximum > 0)
            {
                strTemp = character.SkillsSection.SkillGroupPoints.ToString(GlobalSettings.CultureInfo) + strOf +
                          intSkillGroupPointsMaximum.ToString(GlobalSettings.CultureInfo);
            }

            int intSkillGroupsTotalCostKarma = character.SkillsSection.SkillGroups.TotalCostKarma();
            if (intSkillGroupsTotalCostKarma > 0)
            {
                if (strTemp != strZeroKarma)
                {
                    strTemp += strColon + strSpace + intSkillGroupsTotalCostKarma.ToString(GlobalSettings.CultureInfo) +
                               strSpace + strKarma;
                }
                else
                {
                    strTemp = intSkillGroupsTotalCostKarma.ToString(GlobalSettings.CultureInfo) + strSpace + strKarma;
                }
            }


            return strTemp;
        }

        public static string KnowledgeSkillsBP(string strZeroKarma, string strOf, string strColon, string strSpace,
            string strKarma, Character character)
        {
            string strTemp = strZeroKarma;
            int intKnowledgeSkillPointsMaximum = character.SkillsSection.KnowledgeSkillPoints;
            if (intKnowledgeSkillPointsMaximum > 0)
            {
                strTemp = character.SkillsSection.KnowledgeSkillPointsRemain.ToString(GlobalSettings.CultureInfo) +
                          strOf + intKnowledgeSkillPointsMaximum.ToString(GlobalSettings.CultureInfo);
            }

            int intKnowledgeSkillsTotalCostKarma = character.SkillsSection.KnowledgeSkills.TotalCostKarma();
            if (intKnowledgeSkillsTotalCostKarma > 0)
            {
                if (strTemp != strZeroKarma)
                {
                    strTemp += strColon + strSpace + intKnowledgeSkillsTotalCostKarma.ToString(GlobalSettings.CultureInfo) +
                               strSpace + strKarma;
                }
                else
                {
                    strTemp = intKnowledgeSkillsTotalCostKarma.ToString(GlobalSettings.CultureInfo) + strSpace + strKarma;
                }
            }

            return strTemp;
        }

        public static string ActiveSkillBP(string strZeroKarma, string strOf, string strColon, string strSpace,
            string strKarma, Character character)
        {
            string strTemp = strZeroKarma;
            int intActiveSkillPointsMaximum = character.SkillsSection.SkillPointsMaximum;
            if (intActiveSkillPointsMaximum > 0)
            {
                strTemp = character.SkillsSection.SkillPoints.ToString(GlobalSettings.CultureInfo) + strOf +
                          intActiveSkillPointsMaximum.ToString(GlobalSettings.CultureInfo);
            }

            int intActiveSkillsTotalCostKarma = character.SkillsSection.Skills.TotalCostKarma();
            if (intActiveSkillsTotalCostKarma > 0)
            {
                if (strTemp != strZeroKarma)
                {
                    strTemp += strColon + strSpace + intActiveSkillsTotalCostKarma.ToString(GlobalSettings.CultureInfo) +
                               strSpace + strKarma;
                }
                else
                {
                    strTemp = intActiveSkillsTotalCostKarma.ToString(GlobalSettings.CultureInfo) + strSpace + strKarma;
                }
            }

            return strTemp;
        }

    }
}
