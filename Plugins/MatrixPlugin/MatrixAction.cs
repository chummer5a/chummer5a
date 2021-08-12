using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace MatrixPlugin
{
    public class MatrixAction
    {
        public string Name;
        public string Description;
        public string Attribute;
        public string Skill;
        public string Limit;
        public string DAttribute;
        public string DSkill;
        public int Modifier;

        private static string[] Attributes = { "LOG", "WIL", "INT", "CHA" };
        private static string[] Skills = { "Computer", "Software", "Cybercombat", "Hacking", "Electronic Warfare", "Firewall", "Data Processing", "Attack", "Sleaze" };

        public MatrixAction()
        {

        }

        public MatrixAction(XmlNode xmlAction)
        {
            Name = xmlAction.SelectSingleNode("name").FirstChild.Value;
            Description = "";//xmlAction.SelectSingleNode("test/bonusstring") == null ? "" : xmlAction.SelectSingleNode("test/bonusstring").FirstChild.Value;
            Attribute = "";
            Skill = "";
            Limit = "";
            DAttribute = "";
            DSkill = "";
            Modifier = 0;

            string limit = xmlAction.SelectSingleNode("test/limit").FirstChild.Value;
            
            string[] Dice = xmlAction.SelectSingleNode("test/dice").FirstChild.Value.Split('.');
            foreach (string attr in MatrixAction.Attributes)
            {
                if (Dice[0].Contains(attr))
                    Attribute = attr;
                if (Dice.Length > 1 && Dice[1].Contains(attr))
                    DAttribute = attr;
            }
            foreach (string skill in MatrixAction.Skills)
            {
                if (Dice[0].Contains(skill))
                    Skill = skill;
                if (Dice.Length > 1 && Dice[1].Contains(skill))
                    DSkill = skill;
                if (limit.Contains(skill))
                    Limit = skill;
            }
            //if (Regex.IsMatch(Dice[0], "[-]* [\\d]{1,}"))
                //Modifier = int.Parse(Regex.Match(Dice[0], "[-]* [\\d]{1,}").Value);


        }
    }
}
