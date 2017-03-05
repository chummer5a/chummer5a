using System.Reflection;
using System.Windows.Forms;
using Chummer.Backend.Attributes.OptionAttributes;
using Chummer.Backend.Options;

namespace Chummer.UI.Options.ControlGenerators
{
    public class PathSelectiorFactory : IOptionWinFromControlFactory
    {
        public bool IsSupported(OptionItem backingEntry)
        {
            OptionEntryProxy proxy = backingEntry as OptionEntryProxy;
            if (proxy != null)
            {
                return proxy.TargetProperty.PropertyType == typeof(string) &&
                       proxy.TargetProperty.GetCustomAttribute<IsPathAttribute>() != null;
            }

            return false;
        }

        public Control Construct(OptionItem backingEntry)
        {
            return new PathSelectorControl() {PathEntry = backingEntry as OptionEntryProxy};
        }
    }
}