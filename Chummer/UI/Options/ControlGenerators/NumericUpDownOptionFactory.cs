using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Chummer.Backend.Options;

namespace Chummer.UI.Options.ControlGenerators
{
    public class NumericUpDownOptionFactory : IOptionWinFromControlFactory
    {
        public bool IsSupported(OptionItem backingEntry)
        {
            OptionEntryProxy proxy = backingEntry as OptionEntryProxy;
            return proxy != null && proxy.TargetProperty.PropertyType == (typeof(int));
        }


        public Control Construct(OptionItem backingEntry)
        {
            OptionEntryProxy proxy = (OptionEntryProxy) backingEntry;

            NumericUpDown nud = new NumericUpDown(){Maximum = 10000, Location = new Point(0, -2)};
            /*nud.DataBindings.Add(
                nameof(NumericUpDown.Value),
                proxy,
                nameof(OptionEntryProxy.Value),
                false,
                DataSourceUpdateMode.OnPropertyChanged);
*/
            nud.Width = 40;
            if (Utils.IsLinux)
            {
                nud.BackColor = Color.WhiteSmoke;
            }

            //Done this way instead of databinding apparently only works one way for me.
            //Might be mono issue
            proxy.PropertyChanged += (back, args) =>
            {
                OptionEntryProxy backing = (OptionEntryProxy) back;
                if (args.PropertyName == "Enabled")
                {
                    nud.Enabled = backing.Enabled;
                }
                else if(args.PropertyName == "Value")
                {
                    nud.Value = Convert.ToDecimal(backing.Value);
                }
            };

            nud.ValueChanged += (sender, args) =>
            {
                proxy.Value = ((NumericUpDown) sender).Value;
            };

            nud.Enabled = proxy.Enabled;
            nud.Value = Convert.ToDecimal(proxy.Value);

            return nud;
        }


    }
}