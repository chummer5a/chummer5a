using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Chummer;
using Chummer.Datastructures;

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

        public override bool AllowDelete
        {
            get
            {
                return !CharacterObject.Created;
            }
        }

        public override int CurrentSpCost()
        {
            return Math.Max(BasePoints, 0);
        }

        /// <summary>
        /// How much karma this costs. Return value during career mode is undefined
        /// </summary>
        /// <returns></returns>
        public override int CurrentKarmaCost()
        {
            return Math.Max(RangeCost(Base + FreeKarma(), TotalBaseRating), 0);
        }

        public override bool IsExoticSkill
        {
            get
            {
                return true;
            }
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
            get
            {
                return _strSpecific;
            }
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
