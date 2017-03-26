namespace Chummer.Backend.Attributes.OptionAttributes
{
    public sealed class IsPathAttribute : System.Attribute
    {
        public bool Folder { get; set; }
        private string _filter;

        public IsPathAttribute(bool folder = false)
        {
            Folder = folder;
        }

        public string Filter
        {
            get { return _filter; }
            set { _filter = value; }
        }
    }
}