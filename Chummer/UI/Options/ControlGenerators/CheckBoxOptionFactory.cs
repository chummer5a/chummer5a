using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Chummer.Backend.Options;

namespace Chummer.UI.Options.ControlGenerators
{
    public class CheckBoxOptionFactory : IOptionWinFromControlFactory
    {
        public bool IsSupported(OptionItem backingEntry)
        {
            OptionEntryProxy proxy = backingEntry as OptionEntryProxy;
            return proxy != null && proxy.TargetProperty.PropertyType == (typeof(bool));
        }

        public Control Construct(OptionItem backingEntry)
        {
            CheckBox box = new CheckBox(){Location = new Point(0, -4)};
            box.DataBindings.Add(
                nameof(CheckBox.Checked),
                backingEntry,
                nameof(OptionEntryProxy.Value),false,
                DataSourceUpdateMode.OnPropertyChanged);

            box.DataBindings.Add(
                nameof(CheckBox.Enabled),
                backingEntry,
                nameof(OptionEntryProxy.Enabled),
                false,
                DataSourceUpdateMode.OnPropertyChanged);

            return box;
        }
    }
}