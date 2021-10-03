using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
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
        private readonly Action _action;
        private readonly Action _defenceAction;
        //Pattern for finding +X or -X in string
        private static string _dicePoolModifierRegexPattern = "([-]{0,1})[ ]([\\d]+)";

        public MatrixAction()
        {
            _action = new Action();
            _defenceAction = new Action();
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
            get => _action.Skill;
            set
            {
                _action.Skill = value;
                OnPropertyChanged();
            }
        }

        [XmlIgnore]
        public string ActionAttribute
        {
            get => _action.Attribute;
            set
            {
                _action.Attribute = value;
                OnPropertyChanged();
            }
        }

        [XmlIgnore]
        public int ActionModifier
        {
            get => _action.Modifier;
            set
            {
                _action.Modifier = value;
                OnPropertyChanged();
            }
        }

        [XmlIgnore]
        public string Limit
        {
            get => _action.Limit;
            set
            {
                _action.Limit = value;
                OnPropertyChanged();
            }
        }

        [XmlIgnore]
        public int LimitModifier
        {
            get => _action.LimitModifier;
            set
            {
                _action.LimitModifier = value;
                OnPropertyChanged();
            }
        }

        [XmlIgnore]
        public string DefenceSkill
        {
            get => _defenceAction.Skill;
            set
            {
                _defenceAction.Skill = value;
                OnPropertyChanged();
            }
        }

        [XmlIgnore]
        public string DefenceAttribute
        {
            get => _defenceAction.Attribute;
            set
            {
                _defenceAction.Attribute = value;
                OnPropertyChanged();
            }
        }

        [XmlIgnore]
        public int DefenceModifier
        {
            get => _defenceAction.Modifier;
            set
            {
                _defenceAction.Modifier = value;
                OnPropertyChanged();
            }
        }

        private Test tests;
        [XmlElement("test")]
        public Test Tests
        {
            get => tests;
            set
            {
                tests = value;
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
            if (!string.IsNullOrEmpty(actionCheck) && Regex.IsMatch(actionCheck, _dicePoolModifierRegexPattern))
            {
                Match match = Regex.Match(actionCheck, _dicePoolModifierRegexPattern);
                string result = match.Groups[1].Value + match.Groups[2].Value;
                ActionModifier = int.Parse(result);
            }
            if (!string.IsNullOrEmpty(defenceCheck) && Regex.IsMatch(defenceCheck, _dicePoolModifierRegexPattern))
            {
                Match match = Regex.Match(defenceCheck, _dicePoolModifierRegexPattern);
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
