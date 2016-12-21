using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using Chummer.Annotations;

namespace Chummer.Backend.Options
{
    public class OptionEntryProxy : OptionRenderItem, INotifyPropertyChanged
    {
        public PropertyInfo TargetProperty { get; }
        public string DisplayString { get; }
        public string ToolTip { get; }
        private readonly object _targetObject;

        public OptionEntryProxy([NotNull] object targetObject, [NotNull] PropertyInfo targetProperty, string displayString = null,
            string toolTip = null)
        {
            if (targetObject == null) throw new ArgumentNullException(nameof(targetObject));
            if (targetProperty == null) throw new ArgumentNullException(nameof(targetProperty));

            TargetProperty = targetProperty;
            DisplayString = displayString;
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
            }
        }

        public bool Enabled => true;


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}