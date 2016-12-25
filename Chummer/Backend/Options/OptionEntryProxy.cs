using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using Chummer.Annotations;
using Octokit;

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
            string toolTip = null) : base(displayString)
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
            get { return _value; }
            set
            {
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
    }
}