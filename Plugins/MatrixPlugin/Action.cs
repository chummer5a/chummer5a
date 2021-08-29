namespace MatrixPlugin
{
    public class Action
    {
        public string Skill { get; set; }
        public string Attribute { get; set; }
        public int Modifier { get; set; }
        public string Limit { get; set; }
        public int LimitModifier { get; set; }

        public Action(string skill = "", string attribute = "", int modifier = 0, string limit = "", int limitModifier = 0)
        {
            Skill = skill;
            Attribute = attribute;
            Modifier = modifier;
            Limit = limit;
            LimitModifier = limitModifier;
        }
    }
}
