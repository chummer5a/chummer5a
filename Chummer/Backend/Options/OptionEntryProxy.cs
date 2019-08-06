using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using Chummer.Annotations;

namespace Chummer.Backend.Options
{
    public class OptionEntryProxy : OptionItem, INotifyPropertyChanged
    {
        public PropertyInfo TargetProperty { get; }

        public string ToolTip { get; }

        private readonly object _targetObject;
        private bool _enabledCached = true;
        private Func<List<OptionEntryProxy>, bool> _constaintDelegate = null;
        private List<OptionEntryProxy> _dependantProperties;

        public OptionEntryProxy([NotNull] object targetObject, [NotNull] PropertyInfo targetProperty, string displayString = null,
            string category = null, string toolTip = null) : base(displayString, category)
        {
            if (targetObject == null) throw new ArgumentNullException(nameof(targetObject));
            if (targetProperty == null) throw new ArgumentNullException(nameof(targetProperty));

            TargetProperty = targetProperty;
            ToolTip = toolTip;
            _targetObject = targetObject;

            _value = targetProperty.GetValue(_targetObject);
        }

        private object _value;

        public object Value
        {
            get
            {
                if (TargetProperty.PropertyType == typeof(int))
                {
                    return Convert.ToDecimal(_value);
                }
                return _value;
            }
            set
            {
                if (value?.GetType() != TargetProperty.PropertyType)
                {
                    value = Convert.ChangeType(value, TargetProperty.PropertyType);
                }
                _value = value;
                OnPropertyChanged();
                ValueChanged?.Invoke();
            }
        }

        public bool Enabled => _enabledCached;


        public event PropertyChangedEventHandler PropertyChanged;
        public event Action ValueChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void SetConstaint(Func<List<OptionEntryProxy>, bool>  constaintDelegate, List<OptionEntryProxy> dependantProperties)
        {
            if(_constaintDelegate != null) throw new InvalidOperationException("Can only set constaint once");

            _constaintDelegate = constaintDelegate;
            foreach (OptionEntryProxy dependantProperty in dependantProperties)
            {
                dependantProperty.ValueChanged += RefreshEnabled;
            }

            _dependantProperties = dependantProperties;
            RefreshEnabled();
        }

        private void RefreshEnabled()
        {
            bool newEnabledValue = _constaintDelegate(_dependantProperties);

            if (newEnabledValue != _enabledCached)
            {
                _enabledCached = newEnabledValue;
                OnPropertyChanged(nameof(Enabled));
            }
        }

        public override IEnumerable<string> SearchStrings()
        {
            foreach (string s in base.SearchStrings())
            {
                yield return s;
            }
            if(ToolTip != null) yield return ToolTip;
        }

        public override bool Save()
        {
            //Maybe, just maybe this will prevent fucking up if complex properties is used/sat
            object old = TargetProperty.GetValue(_targetObject);
            if (_value != null && !_value.Equals(old))
            {
                TargetProperty.SetValue(_targetObject, _value);
                return true;
            }
            return false;
        }

        public override void Reload()
        {
            Value = TargetProperty.GetValue(_targetObject);
            OnPropertyChanged(nameof(Value));
            ValueChanged?.Invoke();
        }
    }
}
