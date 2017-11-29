using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Chummer;
using Chummer.Datastructures;

namespace Chummer.Skills
{ 
    class ExoticSkill : Skill
    {
        private static readonly TranslatedField<string> _specificTranslator = new TranslatedField<string>();
        private string _specific;
        private string _translated;

        static ExoticSkill()
        {
            XmlNodeList exotic = XmlManager.Load("weapons.xml")?.SelectNodes("/chummer/weapons/weapon");

            if (exotic != null)
            {
                foreach (XmlNode objLoopNode in exotic)
                {
                    string strLoopName = string.Empty;
                    if (objLoopNode.TryGetStringFieldQuickly("name", ref strLoopName))
                    {
                        string strLoopTranslate = objLoopNode.Attributes?["translate"]?.InnerText ?? strLoopName;
                        _specificTranslator.Add(strLoopName, strLoopTranslate);
                    }
                }
            }
        }


        public ExoticSkill(Character character, XmlNode node) : base(character, node)
        {
        }

        public void Load(XmlNode node)
        {
            node.TryGetStringFieldQuickly("specific", ref _specific);
            node.TryGetStringFieldQuickly("translated", ref _translated);
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
