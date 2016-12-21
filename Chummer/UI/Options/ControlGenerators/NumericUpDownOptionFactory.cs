using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Chummer.Backend.Options;

namespace Chummer.UI.Options.ControlGenerators
{
    public class NumericUpDownOptionFactory : IOptionWinFromControlFactory
    {
        public bool IsSupported(PropertyInfo property) => property.PropertyType == (typeof(int));


        public Control Construct(OptionEntryProxy backingEntry)
        {
            NumericUpDown nud = new NumericUpDown(){Maximum = 10000, Location = new Point(0, -2)};
            nud.DataBindings.Add(
                nameof(NumericUpDown.Value),
                backingEntry,
                nameof(OptionEntryProxy.Value),
                false,
                DataSourceUpdateMode.OnPropertyChanged);

            nud.DataBindings.Add(
                nameof(NumericUpDown.Enabled),
                backingEntry,
                nameof(OptionEntryProxy.Enabled),
                false,
                DataSourceUpdateMode.OnPropertyChanged);

            if (Utils.IsLinux)
            {
                nud.Width = 60;
                nud.BackColor = Color.WhiteSmoke;
            }

            return nud;
        }
    }
}