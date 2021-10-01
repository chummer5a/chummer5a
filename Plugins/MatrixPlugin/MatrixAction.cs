using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;
using Chummer;
using Chummer.Backend.Attributes;

namespace MatrixPlugin
{
    /// <summary>
    /// Class with a representation of matrix actions
    /// it contains both skillcheck (action skillcheck and defence check)
    /// </summary>
    [XmlType("action")]
    public class MatrixAction : INotifyPropertyChanged
    {
        private static readonly string[] SkillNames = { "Computer", "Software", "Cybercombat", "Hacking", "Electronic Warfare" };
        private readonly Action action;
        private readonly Action defenceAction;

        public MatrixAction()
        {
            action = new Action();
            defenceAction = new Action();
        }

        public MatrixAction(XPathNavigator xmlAction)
        {
            XmlReader reader = xmlAction.ReadSubtree();
            action = new Action();
            defenceAction = new Action();
            Name = extractStringFromXmlNode(xmlAction, "name");
            Type = extractStringFromXmlNode(xmlAction,"type");
            Description = extractStringFromXmlNode(xmlAction,"test/bonusstring");

            string limit = extractStringFromXmlNode(xmlAction, "test/limit");

            string[] SkillCheck = xmlAction.SelectSingleNode("test/dice").Value.Split('.');
            //Parsing SkillCheck as
            //Skill + Attribute|MatrixAttribute + Modifier vs. Skill + MatrixAttribute + Modifier
            string actionCheck = SkillCheck[0];
            string defenceCheck = SkillCheck.Length > 1 ? SkillCheck[0] : "";
            //Parse Skills
            foreach (string skill in SkillNames)
            {
                if (actionCheck.Contains(skill))
                    ActionSkill = skill;
                if (defenceCheck.Contains(skill))
                    DefenceSkill = skill;
            }
            //Parse MatrixAttributes
            foreach (string skill in MatrixAttributes.MatrixAttributeStrings)
            {
                if (actionCheck.Contains(skill))
                    ActionSkill = skill;
                if (defenceCheck.Contains(skill))
                    DefenceSkill = skill;
                if (limit.Contains(skill))
                    Limit = skill;
            }
            //Parse MentalAttributes
            foreach (string attr in AttributeSection.MentalAttributes)
            {
                if (actionCheck.Contains(attr))
                    ActionAttribute = attr;
                if (defenceCheck.Contains(attr))
                    DefenceAttribute = attr;
            }
            //Parse Modifiers
            // as [-] Digits
            if (Regex.IsMatch(actionCheck, "([-]{0,1})[ ]([\\d]+)"))
            {
                Match match = Regex.Match(actionCheck, "([-]{0,1})[ ]([\\d]+)");
                string result = match.Groups[1].Value + match.Groups[2].Value;
                ActionModifier = int.Parse(result);
            }
            if (Regex.IsMatch(defenceCheck, "([-]{0,1})[ ]([\\d]+)"))
            {
                Match match = Regex.Match(defenceCheck, "([-]{0,1})[ ]([\\d]+)");
                string result = match.Groups[1].Value + match.Groups[2].Value;
                DefenceModifier = int.Parse(result);
            }
        }

        private string extractStringFromXmlNode(XPathNavigator xmlAction,string path)
        {
            var xmlType = xmlAction.SelectSingleNode(path);
            if (xmlType != null)
                return xmlType.Value;
            return "";
        }

        #region Properties
        [XmlElement("name")]
        public string Name { get; set; }
        [XmlIgnore]
        public string Description { get; set; }
        [XmlElement("type")]
        public string Type { get; set; }
        [XmlIgnore]
        public string ActionSkill
        {
            get => action.Skill;
            set
            {
                action.Skill = value;
                OnPropertyChanged();
            }
        }
        [XmlIgnore]
        public string ActionAttribute
        {
            get => action.Attribute;
            set
            {
                action.Attribute = value;
                OnPropertyChanged();
            }
        }
        [XmlIgnore]
        public int ActionModifier
        {
            get => action.Modifier;
            set
            {
                action.Modifier = value;
                OnPropertyChanged();
            }
        }
        [XmlIgnore]
        public string Limit
        {
            get => action.Limit;
            set
            {
                action.Limit = value;
                OnPropertyChanged();
            }
        }
        [XmlIgnore]
        public int LimitModifier
        {
            get => action.LimitModifier;
            set
            {
                action.LimitModifier = value;
                OnPropertyChanged();
            }
        }
        [XmlIgnore]
        public string DefenceSkill
        {
            get => defenceAction.Skill;
            set
            {
                defenceAction.Skill = value;
                OnPropertyChanged();
            }
        }
        [XmlIgnore]
        public string DefenceAttribute
        {
            get => defenceAction.Attribute;
            set
            {
                defenceAction.Attribute = value;
                OnPropertyChanged();
            }
        }
        [XmlIgnore]
        public int DefenceModifier
        {
            get => defenceAction.Modifier;
            set
            {
                defenceAction.Modifier = value;
                OnPropertyChanged();
            }
        }

        private Test testObj;
        [XmlElement("test")]
        public Test Tests
        {
            get => testObj;
            set
            {
                testObj = value;
                Description = value.bonusstring;
                ParseTest(value);
            }
        }

        #endregion

        private void ParseTest(Test test)
        {
            string[] SkillCheck = test.dice.Split('.');
            //Parsing SkillCheck as
            //Skill + Attribute|MatrixAttribute + Modifier vs. Skill + MatrixAttribute + Modifier
            string actionCheck = SkillCheck[0];
            string defenceCheck = SkillCheck.Length > 1 ? SkillCheck[1] : "";
            //Parse Skills
            foreach (string skill in SkillNames)
            {
                if (actionCheck.Contains(skill))
                    ActionSkill = skill;
                if (defenceCheck.Contains(skill))
                    DefenceSkill = skill;
            }
            //Parse MatrixAttributes
            foreach (string skill in MatrixAttributes.MatrixAttributeStrings)
            {
                if (!string.IsNullOrEmpty(defenceCheck) && defenceCheck.Contains(skill))
                    DefenceSkill = skill;
                if (!string.IsNullOrEmpty(test.limit) && test.limit.Contains(skill))
                    Limit = skill;
            }
            //Parse MentalAttributes
            foreach (string attr in AttributeSection.MentalAttributes)
            {
                if (!string.IsNullOrEmpty(actionCheck) && actionCheck.Contains(attr))
                    ActionAttribute = attr;
                if (!string.IsNullOrEmpty(defenceCheck) && defenceCheck.Contains(attr))
                    DefenceAttribute = attr;
            }
            //Parse Modifiers
            // as [-] Digits
            if (!string.IsNullOrEmpty(actionCheck) && Regex.IsMatch(actionCheck, "([-]{0,1})[ ]([\\d]+)"))
            {
                Match match = Regex.Match(actionCheck, "([-]{0,1})[ ]([\\d]+)");
                string result = match.Groups[1].Value + match.Groups[2].Value;
                ActionModifier = int.Parse(result);
            }
            if (!string.IsNullOrEmpty(defenceCheck) && Regex.IsMatch(defenceCheck, "([-]{0,1})[ ]([\\d]+)"))
            {
                Match match = Regex.Match(defenceCheck, "([-]{0,1})[ ]([\\d]+)");
                string result = match.Groups[1].Value + match.Groups[2].Value;
                DefenceModifier = int.Parse(result);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        /// <summary>
        /// Inner class with a representation of skillcheck
        /// as a Skill + Attribute [Limit]
        /// it also has a modifiers for dicepool of skillcheck and limit
        /// </summary>
        private class Action
        {
            public string Skill { get; set; } = "";
            public string Attribute { get; set; } = "";
            public int Modifier { get; set; } = 0;
            public string Limit { get; set; } = "";
            public int LimitModifier { get; set; } = 0;

            public Action()
            {
            }
        }

        /// <summary>
        /// Inner class for xml deserialization
        /// </summary>
        public class Test
        {
            [XmlElement("dice")]
            public string dice { get; set; }

            [XmlElement("bonusstring")]
            public string bonusstring { get; set; }
            [XmlElement("limit")]
            public string limit { get; set; }

            public Test()
            { }
        }
    }
}
