using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Chummer.Backend.Attributes.OptionAttributes;
using Chummer.Backend.Options;

namespace Chummer.UI.Options.ControlGenerators
{
    class DropDownFactory : IOptionWinFromControlFactory
    {
        //Probably going to give errors if display scaling is enabled
        private const int DOWN_ARRAY_SPACE = 20;
        private const int TEXTBOX_MATCH_LABEL = 4;

        public bool IsSupported(OptionItem backingEntry)
        {
            OptionEntryProxy v = backingEntry as OptionEntryProxy;
            if (v != null)
            {
                if (v.TargetProperty.PropertyType.IsEnum ||  v.TargetProperty.GetCustomAttribute<DropDownAttribute>() != null)
                    return true;
            }
            return false;
        }

        public Control Construct(OptionItem backingEntry)
        {

            OptionEntryProxy v = backingEntry as OptionEntryProxy;
            if (v != null)
            {

                ComboBox backing = new ComboBox()
                {
                    DropDownStyle = ComboBoxStyle.DropDownList
                };

                DropDownAttribute attribute;
                int width;
                if (v.TargetProperty.PropertyType.IsEnum)
                {
                    List<ListItem<Enum>> enumDisplayValues = CreateDisplayValuesFromEnum(v.TargetProperty);
                    width = enumDisplayValues.Select(s => TextRenderer.MeasureText(s.Name, Control.DefaultFont).Width).Max() + DOWN_ARRAY_SPACE;

                    // ReSharper disable once ObjectCreationAsStatement
                    new ComboBoxBinder<Enum>(v, backing, enumDisplayValues);
                }
                else if ((attribute = v.TargetProperty.GetCustomAttribute<DropDownAttribute>()) != null)
                {
                    var displayValues = CreateDisplayValuesFromDropDownAttribute(attribute);
                    width = displayValues.Select(s => TextRenderer.MeasureText(s.Name, Control.DefaultFont).Width).Max() + DOWN_ARRAY_SPACE;
                    
                    // ReSharper disable once ObjectCreationAsStatement
                    new ComboBoxBinder<string>(v, backing, displayValues);
                }
                else
                {
                    throw new ArgumentException("BackingEntry should either be an enum or have a DropDownAttribute");
                }

                backing.Width = width;
                backing.Top -= TEXTBOX_MATCH_LABEL;
                return backing;
            }

            throw new NotImplementedException();
        }

        private List<ListItem<string>> CreateDisplayValuesFromDropDownAttribute(DropDownAttribute source)
        {
            return source.GetDisplayList();
        }

        private List<ListItem<Enum>> CreateDisplayValuesFromEnum(PropertyInfo enumProperty)
        {
            List<ListItem<Enum>> list = new List<ListItem<Enum>>();
            string name = enumProperty.PropertyType.Name;
            foreach (var value in Enum.GetValues(enumProperty.PropertyType).Cast<Enum>())
            {
                string valueName = $"{name}_{value}";
                string display;
                //list.Add(new ListItem<Enum>(value, LanguageManager.TryGetString(valueName, out display)
                //    ? display
                //    : value.ToString()));
                if(LanguageManager.TryGetString(valueName, out display))
                {
                    list.Add(new ListItem<Enum>(value, display));
                }
                else
                {
                    list.Add(new ListItem<Enum>(value, value.ToString()));
                }
            }

            return list;
        }

        private class ComboBoxBinder<T>
        {
            private readonly OptionEntryProxy _backingField;
            private readonly ComboBox _uiElement;
            private readonly List<ListItem<T>> _items;
            
            public ComboBoxBinder(
                OptionEntryProxy backingField, 
                ComboBox uiElement,
                List<ListItem<T>> items
            )
            {
                _backingField = backingField;
                _uiElement = uiElement;
                _items = items;
                for (int i = 0; i < _items.Count; i++)
                {
                    _uiElement.Items.Add(_items[i].Name);
                    if (_items[i].Value.Equals(backingField.Value))
                        _uiElement.SelectedIndex = i;
                }

                backingField.ValueChanged += BackingFieldOnValueChanged;
                uiElement.SelectedIndexChanged += UiElementOnSelectedIndexChanged;
            }

            private bool _updating = false;
            private void UiElementOnSelectedIndexChanged(object sender, EventArgs eventArgs)
            {
                if (_updating) return;
                try
                {
                    _updating = true;
                    //TODO: This crashes with dropdowns and (scroll?)
                    if (_uiElement.SelectedIndex < 0)
                        _uiElement.SelectedIndex = 0;
                    if (_uiElement.SelectedIndex >= _items.Count)
                        _uiElement.SelectedIndex = _items.Count - 1;
                    _backingField.Value = _items[_uiElement.SelectedIndex].Value;
                }
                finally
                {
                    _updating = false;
                }
                
            }

            private void BackingFieldOnValueChanged()
            {
                if (_updating) return;
                try
                {
                    _updating = true;
                    int index = _items.FindIndex(x => x.Equals(_backingField.Value));
                    if (index != -1)
                        _uiElement.SelectedIndex = index;
                    else
                        _uiElement.SelectedItem = null;
                }
                finally
                {
                    _updating = false;
                }
            }
        }
    }
}
