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

		    if (exotic != null)
		    {
		        List<Tuple<string, string>> elem = new List<Tuple<string, string>>();

                foreach (XmlNode objLoopNode in exotic)
                {
                    string strLoopName = string.Empty;
                    if (objLoopNode.TryGetStringFieldQuickly("name", ref strLoopName))
                    {
                        string strLoopTranslate = objLoopNode.Attributes?["translate"]?.InnerText ?? strLoopName;
                        elem.Add(new Tuple<string, string>(strLoopName, strLoopTranslate));
                    }
                }

        public override bool AllowDelete => !CharacterObject.Created;

        public override int CurrentSpCost => Math.Max(BasePoints, 0);

        /// <summary>
        /// How much karma this costs. Return value during career mode is undefined
        /// </summary>
        /// <returns></returns>
        public override int CurrentKarmaCost => Math.Max(RangeCost(Base + FreeKarma, TotalBaseRating), 0);

		public void Load(XmlNode node)
		{
		    node.TryGetStringFieldQuickly("specific", ref _specific);
            node.TryGetStringFieldQuickly("translated", ref _translated);
		}

        /// <summary>
        /// Called during save to allow derived classes to save additional infomation required to rebuild state
        /// </summary>
        /// <param name="writer"></param>
        protected override void SaveExtendedData(XmlTextWriter writer)
        {
            writer.WriteElementString("specific", _strSpecific);
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

		/// <summary>
		/// Called during save to allow derived classes to save additional infomation required to rebuild state
		/// </summary>
		/// <param name="writer"></param>
		protected override void SaveExtendedData(XmlTextWriter writer)
		{
			writer.WriteElementString("specific", _specific);

			if (!string.IsNullOrEmpty(_translated))
                writer.WriteElementString("translated", _translated);
		}

		public string Specific {
		    get
		    {
		        return _specificTranslator.Read(_specific, ref _translated);
		    }
			set
			{
				_specificTranslator.Write(value, ref _specific, ref _translated);
				OnPropertyChanged();
			}
		}

		public override string DisplaySpecialization
		{
		    get
		    {
		        return Specific;
		    }
		}
	}
}

        public void Load(XmlNode node)
        {
            node.TryGetStringFieldQuickly("specific", ref _strSpecific);
        }
        public override bool IsExoticSkill => true;
        public override string DisplaySpecializationMethod(string strLanguage)
        {
            return DisplaySpecific(strLanguage);
        }
    }