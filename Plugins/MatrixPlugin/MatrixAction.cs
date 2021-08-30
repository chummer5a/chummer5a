using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Xml;

namespace MatrixPlugin
{
    public class MatrixAction : INotifyPropertyChanged
    {
        private static readonly string[] Attributes = { "LOG", "WIL", "INT", "CHA" };
        private static readonly string[] Skills = { "Computer", "Software", "Cybercombat", "Hacking", "Electronic Warfare", "Firewall", "Data Processing", "Attack", "Sleaze" };

        private readonly Action action;
        private readonly Action defenceAction;

        public MatrixAction()
        {
            action = new Action();
            defenceAction = new Action();
        }

        public MatrixAction(XmlNode xmlAction)
        {
            Name = xmlAction.SelectSingleNode("name").FirstChild.Value;
            Description = "";
            Type = "";
            action = new Action();
            defenceAction = new Action();
            ActionModifier = 0;

            if (xmlAction.SelectSingleNode("test/bonusstring") != null)
                Description = xmlAction.SelectSingleNode("test/bonusstring").FirstChild.Value;

            string limit = xmlAction.SelectSingleNode("test/limit").FirstChild.Value;
            if (xmlAction.SelectSingleNode("type") != null)
                Type = xmlAction.SelectSingleNode("type").FirstChild.Value;

            string[] Dice = xmlAction.SelectSingleNode("test/dice").FirstChild.Value.Split('.');
            foreach (string attr in MatrixAction.Attributes)
            {
                if (Dice[0].Contains(attr))
                    ActionAttribute = attr;
                if (Dice.Length > 1 && Dice[1].Contains(attr))
                    DefenceAttribute = attr;
            }
            foreach (string skill in MatrixAction.Skills)
            {
                if (Dice[0].Contains(skill))
                    ActionSkill = skill;
                if (Dice.Length > 1 && Dice[1].Contains(skill))
                    DefenceSkill = skill;
                if (limit.Contains(skill))
                    Limit = skill;
            }

            if (Regex.IsMatch(Dice[0], "([-]{0,1})[ ]([\\d]+)"))
            {
                Match match = Regex.Match(Dice[0], "([-]{0,1})[ ]([\\d]+)");
                string result = match.Groups[1].Value + match.Groups[2].Value;
                ActionModifier = int.Parse(result);
            }
            if (Dice.Length > 1 && Regex.IsMatch(Dice[1], "([-]{0,1})[ ]([\\d]+)"))
            {
                Match match = Regex.Match(Dice[1], "([-]{0,1})[ ]([\\d]+)");
                string result = match.Groups[1].Value + match.Groups[2].Value;
                DefenceModifier = int.Parse(result);
            }
        }

        #region Properties
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string ActionSkill { get => action.Skill; set => action.Skill = value; }
        public string ActionAttribute { get => action.Attribute; set => action.Attribute = value; }
        public int ActionModifier { get => action.Modifier; set => action.Modifier = value; }
        public string Limit { get => action.Limit; set => action.Limit = value; }
        public int LimitModifier { get => action.LimitModifier; set => action.LimitModifier = value; }
        public string DefenceSkill { get => defenceAction.Skill; set => defenceAction.Skill = value; }
        public string DefenceAttribute { get => defenceAction.Attribute; set => defenceAction.Attribute = value; }
        public int DefenceModifier { get => defenceAction.Modifier; set => defenceAction.Modifier = value; }
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

    }
}
