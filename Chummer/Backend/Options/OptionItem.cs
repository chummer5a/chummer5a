namespace Chummer.Backend.Options
{
    public class OptionItem : OptionRenderItem
    {
        public OptionItem(string displayString)
        {
            DisplayString = displayString;
        }

        public string DisplayString { get; }
    }
}