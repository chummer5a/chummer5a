namespace ChummerHub.Client.Sinners
{
    public partial class SINnerSearchGroupMember
    {
        public string Display
        {
            get
            {
                string display = MySINner.Alias;
                if (!string.IsNullOrEmpty(Username))
                    display += " " + Username;
                return display;
            }
        }
    }
}
