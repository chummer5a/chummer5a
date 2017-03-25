namespace Chummer.Backend.Attributes.OptionAttributes
{
    public sealed class IsPathAttribute : System.Attribute
    {
        private string _filter;

        public IsPathAttribute()
        {}

        public string Filter
        {
            get { return _filter; }
            set { _filter = value; }
        }
    }
}