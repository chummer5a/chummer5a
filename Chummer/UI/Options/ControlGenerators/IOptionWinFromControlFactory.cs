using System;
using System.Reflection;
using System.Windows.Forms;
using Chummer.Backend.Options;

namespace Chummer.UI.Options.ControlGenerators
{
    public interface IOptionWinFromControlFactory
    {
        bool IsSupported(OptionItem backingEntry);

        Control Construct(OptionItem backingEntry);
    }
}