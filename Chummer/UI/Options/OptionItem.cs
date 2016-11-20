using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Chummer.Backend.Attributes.OptionDisplayAttributes;


namespace Chummer.UI.Options
{
    public partial class OptionItem : UserControl
    {
        private const int 
            CHECKBOX_START = 5, //Checkbox start in pixel from left edge
            LABEL_START = 20, //label start in pixel from left edge
            SPACING = 5, //spacing between elements. Vertical offsets can change this
            NUD_WIDTH = 60, //Width of numeric up downs
            OTHER_OPTION_OFFSET = 5,  //distance between the label and a following option
            CHECKBOX_VERTICAL_OFFSET = 0, //Vertical offset compared to rest for checkboxes
            LABEL_VERTICAL_OFFSET = 5,  //Vertical offset compared to rest for labels
            NUD_VERTICAL_OFFSET = 3, //Vertical offset compared to rest for numeric up downs
            OTHER_VERTICAL_OFFSET = 0; //Vertical offset compared to rest for other things


        private object _target;
        private readonly List<Wrapper> _contents = new List<Wrapper>();
        private readonly Control _destinationControl; //Probably not going to be used, but allows easy changing of destination
        public OptionItem()
        {
            InitializeComponent();
            _destinationControl = this;
        }

        /// <summary>
        /// Set the list of options to expose
        /// </summary>
        /// <param name="exposedProperties">A list of propertyInfo that each can be used to read or write a property that options should modify</param>
        /// <param name="target">An object containing the property obtions</param>
        public void SetManyToSingleOptions(List<PropertyInfo> exposedProperties, object target = null)
        {
            if (target != null) _target = target;
            if (_target == null)
                throw new InvalidOperationException("target is never set, cannot work with null data");

            SetManyToManyOptions(exposedProperties.Select(x => new Tuple<PropertyInfo, object>(x, null)).ToList());
        }

        //TODO: RENAME TO SOMETHING SENSIBLE
        public void SetManyToManyOptions(List<Tuple<PropertyInfo, object>> exposedPropertiesObject)
        {
            Controls.Clear();
            _contents.Clear();

            int i = 0;
            foreach (Tuple<PropertyInfo, object> tuple in exposedPropertiesObject)
            {
                if((tuple.Item2 ?? _target) == null)
                    throw new InvalidOperationException("target is never set and got propertyinfo without linked object");

                Type t = tuple.Item1.PropertyType;
                string strDisplayName = tuple.Item1.GetCustomAttribute<DisplayConfigurationAttribute>() != null ? tuple.Item1.GetCustomAttribute<DisplayConfigurationAttribute>().DisplayName : tuple.Item1.Name;
				string strTooltip = tuple.Item1.GetCustomAttribute<DisplayConfigurationAttribute>() != null ? tuple.Item1.GetCustomAttribute<DisplayConfigurationAttribute>().Tooltip : "";

				Wrapper w = new Wrapper(this, tuple.Item1, tuple.Item2, strDisplayName, strTooltip);
                w.ReadFrom(w.Target());
                w.SetPos(ref i);
                _contents.Add(w);
                
            }
        }

        public void WriteBack(object target = null)
        {
            if (target == null) target = _target;
            if (target == null)
                throw new InvalidOperationException("target is never set, cannot work with null data");

            foreach (Wrapper item in _contents)
            {
                item.WriteTo(target);
            }


        }

        public void ReadFrom(object target = null)
        {
            if (target == null) target = _target;
            if (target == null)
                throw new InvalidOperationException("target is never set, cannot work with null data");

            foreach (Wrapper item in _contents)
            {
                item.ReadFrom(target);
            }
        }

        private class Wrapper
        {
            //kinda kinda breaks single responsability with target, it either null meaning "from parent" or a specific one
            private readonly OptionItem _parrent;
            private PropertyInfo _info;
            private readonly object _specTarget;


            private Label _label;
            private Control _dataControl;
            #region Different Control Types

            private NumericUpDown _nud;
            private CheckBox _box;
            private ComboBox _dropDown;
            #endregion

            public Wrapper(OptionItem parrent, PropertyInfo propertyInfo, object staticTarget, string label, string tooltip)
            {
                _parrent = parrent;
                _info = propertyInfo;
                _specTarget = staticTarget;
                

                _label = new Label {Text = label, AutoSize = true};
               

                Type type = propertyInfo.PropertyType;
                if (type == typeof(int))
                {
                    _dataControl = _nud = new NumericUpDown() {Maximum = 99999, Width = NUD_WIDTH};
                }
                else if (type == typeof(bool))
                {
                    _dataControl = _box = new CheckBox();
                }
                else if(type.IsSubclassOf(typeof(Enum)))
                {
                    var a = Enum.GetValues(type).Cast<Enum>();

                    _dataControl = _dropDown = new ComboBox() {DropDownStyle = ComboBoxStyle.DropDownList};
                    List<KeyValuePair<string, Enum>> contents = new List<KeyValuePair<string, Enum>>();

                    foreach (Enum o in a)
                    {
                        
                        contents.Add(new KeyValuePair<string, Enum>(GenerateName(o), o));
                    }

                    _dropDown.ValueMember = "Value";
                    _dropDown.DisplayMember = "Key";
                    _dropDown.DataSource = contents;
                }
                else
                {
                    Utils.BreakIfDebug();
                }

                
                parrent._destinationControl.Controls.Add(_label);
                parrent._destinationControl.Controls.Add(_dataControl);
                
                if(!string.IsNullOrWhiteSpace(tooltip))
                {
                    parrent.tipToolTip.SetToolTip(_label, tooltip);
                    if(_dataControl != null)parrent.tipToolTip.SetToolTip(_dataControl, tooltip);
                }
            }

            private string GenerateName(Enum @enum)
            {
                StringBuilder sb = new StringBuilder();
                foreach (char c in @enum.ToString())
                {
                    if (char.IsUpper(c))
                    {
                        if (sb.Length != 0 && !char.IsWhiteSpace(sb[sb.Length - 1]))
                            sb.Append(' ');
                    }
                    sb.Append(c);
                }

                return sb.ToString();
            }

            public void WriteTo(object target)
            {
                if (_nud != null)
                {
                    _info.SetValue(target, (int)/*TODO: Is (int) needed?*/_nud.Value);
                }
                else if (_box != null)
                {
                    _info.SetValue(target, _box.Checked);
                }
                else
                {
                    Utils.BreakIfDebug();
                }
            }

            public void ReadFrom(object target)
            {
                if (_nud != null)
                {
                    _nud.Value = (int)_info.GetValue(target);
                }
                else if (_box != null)
                {
                    _box.Checked = (bool) _info.GetValue(target);
                }
                else if (_dropDown != null)
                {
                    _dropDown.SelectedValue = _info.GetValue(target);
                }
                else
                {
                    Utils.BreakIfDebug();
                }
            }

            public void SetPos(ref int used)
            {
                int top = used + SPACING;

                _label.Location = new Point(LABEL_START, top + LABEL_VERTICAL_OFFSET);  //TODO: center
                if (_dataControl != null)
                {
                    if (_dataControl is CheckBox)
                    {
                        _dataControl.Location = new Point(CHECKBOX_START, top + CHECKBOX_VERTICAL_OFFSET);
                    }
                    else
                    {
                        _dataControl.Location = new Point(_label.Right + OTHER_OPTION_OFFSET, top + (_dataControl is NumericUpDown ? NUD_VERTICAL_OFFSET : OTHER_VERTICAL_OFFSET));
                    }
                }

                used = Math.Max(_label.Bottom, _dataControl?.Bottom ?? 0);
            }

            public object Target()
            {
                return _specTarget ?? _parrent._target;
            }
        }
    }
}
