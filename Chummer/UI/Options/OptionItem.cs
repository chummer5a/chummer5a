using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace Chummer.UI.Options
{
    public partial class OptionItem : UserControl
    {
        const int MUTABLE_START = 5, LABEL_START = 70, UNIT_HEIGHT = 23, SPACING_TOP = 5;
        private object _target = null;
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
        public void Setoptions(List<PropertyInfo> exposedProperties, object target = null)
        {
            if (target != null) _target = target;
            if (_target == null)
                throw new InvalidOperationException("target is never set, cannot work with null data");

            SetIptions(exposedProperties.Select(x => new Tuple<PropertyInfo, object>(x, null)).ToList());
        }

        //TODO: RENAME TO SOMETHING SENSIBLE
        public void SetIptions(List<Tuple<PropertyInfo, object>> exposedPropertiesObject)
        {
            Controls.Clear();
            _contents.Clear();

            int i = 0;
            foreach (Tuple<PropertyInfo, object> tuple in exposedPropertiesObject)
            {
                if((tuple.Item2 ?? _target) == null)
                    throw new InvalidOperationException("target is never set and got propertyinfo without linked object");

                Type t = tuple.Item1.PropertyType;

                Wrapper w = new Wrapper(this, tuple.Item1, tuple.Item2, tuple.Item1.Name, "TOOLTIP");
                w.ReadFrom(w.Target());
                w.SetPos(i);

                i++;
            }
        }

        public void WriteBack(object target = null)
        {
            if (target != null) _target = target;
            if(_target == null)
                throw new InvalidOperationException("target is never set, cannot work with null data");
        }

        public void ReadFrom(object target = null)
        {
            if (target != null) _target = target;
            if (_target == null)
                throw new InvalidOperationException("target is never set, cannot work with null data");
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
                    _dataControl = _nud = new NumericUpDown() {Maximum = 99999, Width = LABEL_START - MUTABLE_START };
                }
                else if (type == typeof(bool))
                {
                    _dataControl = _box = new CheckBox { Width = LABEL_START - MUTABLE_START };
                }
                else
                {
                    //Utils.BreakIfDebug();
                }

                
                parrent._destinationControl.Controls.Add(_label);
                parrent._destinationControl.Controls.Add(_dataControl);
                
                if(!string.IsNullOrWhiteSpace(tooltip))
                {
                    parrent.tipToolTip.SetToolTip(_label, tooltip);
                    if(_dataControl != null)parrent.tipToolTip.SetToolTip(_dataControl, tooltip);
                }
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
            }

            public void SetPos(int index)
            {
                _label.Location = new Point(LABEL_START, index * UNIT_HEIGHT + SPACING_TOP);  //TODO: center
                if (_dataControl != null) _dataControl.Location = new Point(MUTABLE_START, index * UNIT_HEIGHT + SPACING_TOP);
            }

            public object Target()
            {
                return _specTarget ?? _parrent._target;
            }
        }
    }
}
