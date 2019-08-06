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
using System.Xml;

namespace Chummer.Backend.Skills
{
    public sealed class ExoticSkill : Skill
    {
        private string _strSpecific;

        public ExoticSkill(Character character, XmlNode node) : base(character, node)
        {
        }

        public void Load(XmlNode node)
        {
            node.TryGetStringFieldQuickly("specific", ref _strSpecific);
        }

        public override bool AllowDelete => !CharacterObject.Created;

        public override int CurrentSpCost => Math.Max(BasePoints, 0);

        /// <summary>
        /// How much karma this costs. Return value during career mode is undefined
        /// </summary>
        /// <returns></returns>
        public override int CurrentKarmaCost => Math.Max(RangeCost(Base + FreeKarma, TotalBaseRating), 0);

        public override void WriteTo(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("skill");
            objWriter.WriteElementString("guid", Id.ToString("D"));
            objWriter.WriteElementString("suid", SkillId.ToString("D"));
            objWriter.WriteElementString("isknowledge", bool.FalseString);
            objWriter.WriteElementString("skillcategory", SkillCategory);
            objWriter.WriteElementString("karma", KarmaPoints.ToString(GlobalOptions.Instance.InvariantCultureInfo));
            objWriter.WriteElementString("base", BasePoints.ToString(GlobalOptions.Instance.InvariantCultureInfo)); //this could acctually be saved in karma too during career
            objWriter.WriteElementString("notes", Notes);
            if (!CharacterObject.Created)
            {
                objWriter.WriteElementString("buywithkarma", BuyWithKarma.ToString());
            }

            if (Specializations.Count != 0)
            {
                objWriter.WriteStartElement("specs");
                foreach (SkillSpecialization objSpecialization in Specializations)
                {
                    objSpecialization.Save(objWriter);
                }
                objWriter.WriteEndElement();
            }

            objWriter.WriteElementString("specific", _strSpecific);

            objWriter.WriteEndElement();

        }

        public string Specific
        {
            get => _strSpecific;
            set
            {
                _strSpecific = value;

                OnPropertyChanged();
            }
        }

        public string DisplaySpecific(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Specific;

            return LanguageManager.TranslateExtra(Specific, strLanguage);
        }

        public override string DisplaySpecializationMethod(string strLanguage)
        {
            return DisplaySpecific(strLanguage);
        }
    }
}
