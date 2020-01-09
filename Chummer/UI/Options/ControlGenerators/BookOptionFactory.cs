using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Chummer.Backend.Options;

namespace Chummer.UI.Options.ControlGenerators
{
    class BookOptionFactory : IOptionWinFromControlFactory
    {
        public bool IsSupported(OptionItem backingEntry)
        {
            OptionGroup group = backingEntry as OptionGroup;

            if (group?.Children.Count == 5)
            {
                return group.Children.Any(x => x is OptionDictionaryEntryProxy<string, bool>) &&
                       group.Children.OfType<OptionEntryProxy>()
                           .Count(x => x.TargetProperty.PropertyType == typeof(string)) == 3 &&
                       group.Children.Any(
                           x =>
                               x is OptionEntryProxy &&
                               ((OptionEntryProxy) x).TargetProperty.PropertyType == typeof(int));
            }

            return false;
        }

        public Control Construct(OptionItem backingEntry)
        {
            return new BookSettingControl() {Book = backingEntry as OptionGroup};
        }
    }
}
