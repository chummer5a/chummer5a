using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Chummer.Annotations;

namespace Chummer.Backend.Options
{
    public class OptionDictionaryEntryProxy<TKey, TValue> : OptionItem, INotifyPropertyChanged
    {
        private readonly Dictionary<TKey, TValue> _backingDictionary;
        private TValue _value;

        public TKey Key { get; }

        public OptionDictionaryEntryProxy(string category, Dictionary<TKey, TValue> backingDictionary, TKey key) : base("", category)
        {
            _backingDictionary = backingDictionary;
            Key = key;
            _value = backingDictionary[Key];
        }

        public TValue Value
        {
            get { return _value; }
            set
            {
                _value = value;
                OnPropertyChanged();
                ValueChanged?.Invoke();
            }
        }

        public event Action ValueChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override bool Save()
        {
            if (!_backingDictionary[Key].Equals(Value))
            {
                _backingDictionary[Key] = Value;
                return true;
            }
            return false;
        }

        public override void Reload()
        {
            Value = _backingDictionary[Key];
        }
    }
}