using System.Windows.Forms;
using Chummer.Backend.Options;

namespace Chummer.UI.Options.ControlGenerators
{
    class StringControlFactory : IOptionWinFromControlFactory

    {
        public bool IsSupported(OptionItem backingEntry)
        {
            OptionEntryProxy proxy = backingEntry as OptionEntryProxy;
            if (proxy != null)
            {
                return proxy.TargetProperty.PropertyType == typeof(string);
            }

            return false;
        }

        public Control Construct(OptionItem backingEntry)
        {
            OptionEntryProxy proxy = (OptionEntryProxy)backingEntry;
            TextBox tb = new TextBox();
            tb.DataBindings.Add(
                nameof(TextBox.Text),
                proxy,
                nameof(OptionEntryProxy.Value),
                false,
                DataSourceUpdateMode.OnPropertyChanged);

            return tb;
        }
    }
}
