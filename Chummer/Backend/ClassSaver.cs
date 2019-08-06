using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Xml;
using Chummer.Backend.Attributes.SaveAttributes;
using Microsoft.Win32;

namespace Chummer.Backend
{
    public static class ClassSaver
    {
        private static readonly HashSet<Type> AttemptSaveList = new HashSet<Type>
        {
            typeof(int),
            typeof(decimal),
            typeof(double),
            typeof(float),
            typeof(string),
            typeof(bool),
            typeof(Enum),
            typeof(Version)
        };

        public static void Save(object toSave, XmlWriter destination)
        {
            if (toSave == null) throw new NullReferenceException(nameof(toSave));
            if (destination == null) throw new NullReferenceException(nameof(destination));

            SaveInner(toSave, destination.WriteElementString);
        }

        public static void Save(object toSave, RegistryKey destination)
        {
            if (toSave == null) throw new NullReferenceException(nameof(toSave));
            if (destination == null) throw new NullReferenceException(nameof(destination));

            SaveInner(toSave, destination.SetValue);
        }

        //Not 99% sure ref is needed, but makes it explicit what happens and might prevent if anybody uses this on a
        //struct in the future
        public static void Load<T>(ref T target, XmlNode source)
        {
            if (target == null) throw new NullReferenceException(nameof(target));
            if (source == null) throw new NullReferenceException(nameof(source));

            LoadInner(ref target, field => source[field]?.InnerText);

        }

        public static void Load<T>(ref T target, RegistryKey source)
        {
            if (target == null) throw new NullReferenceException(nameof(target));
            if (source == null) throw new NullReferenceException(nameof(source));

            LoadInner(ref target, field => source.GetValue(field)?.ToString());
        }

        private static void LoadInner<T>(ref T target, Func<string, string> read)
        {
            PropertyInfo[] properties = target.GetType().GetProperties();

            foreach (PropertyInfo property in properties)
            {
                if (property.GetCustomAttribute<SaveIgnorePropertyAttribute>() != null)
                    continue;

                if (Unsupported(property)) continue;

                string name = property.GetCustomAttribute<SavePropertyAsAttribute>()?.Name ??
                              property.Name.ToLowerInvariant();


                string unparsed = read(name);

                if (unparsed == null)
                    continue;

                try
                {
                    if (property.CanWrite)
                    {
                        var conv = GetConverter(property.PropertyType);
                        
                        property.SetValue(target, conv(unparsed));
                    }
                    else
                    {
                        //
                    }
                }
                catch (Exception ex)
                {
                    Utils.BreakIfDebug();
                    Log.Exception(ex);
                }

            }
        }

        private static Func<string, object>  GetConverter(Type type)
        {
            var converter = TypeDescriptor.GetConverter(type);
            if (converter.CanConvertFrom(typeof(string)))
                return converter.ConvertFrom;

            var method = type.GetMethod("Parse", BindingFlags.Static | BindingFlags.Public);
            if(method != null)
                return x => method.Invoke(null, new object[] {x});

            Utils.BreakIfDebug();
            throw new ArgumentException("Cannot load option of type " + type);
        }
        
        private static void SaveInner(object toSave, Action<string, string> save)
        {
            PropertyInfo[] properties = toSave.GetType().GetProperties();

            foreach (PropertyInfo property in properties)
            {
                if (property.GetCustomAttribute<SaveIgnorePropertyAttribute>() != null) continue;

                if (Unsupported(property)) continue;

                string name = property.GetCustomAttribute<SavePropertyAsAttribute>()?.Name ??
                              property.Name.ToLowerInvariant();


                save(name, property.GetValue(toSave)?.ToString() ?? "null");
            }
        }

        private static bool Unsupported(PropertyInfo property)
        {
            bool supported = AttemptSaveList.Contains(property.PropertyType) ||
                             AttemptSaveList.Contains(typeof(Enum)) && property.PropertyType.IsSubclassOf(typeof(Enum)) ;//||

                //Dark magic that checks if type property is collection type of allowed properties
                //(property.PropertyType.IsGenericType && typeof(ICollection<>).IsAssignableFrom(property.PropertyType.GetGenericTypeDefinition()));


            return !supported;
        }

    }
}
