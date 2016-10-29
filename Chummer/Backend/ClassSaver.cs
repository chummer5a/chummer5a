using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Xml;
using Chummer.Backend.Attributes.SaveAttributes;
using Octokit;

namespace Chummer.Backend
{
    public class ClassSaver
    {
        private static readonly HashSet<Type> AttemptSaveList = new HashSet<Type>
        {
            typeof(int),
            typeof(decimal),
            typeof(double),
            typeof(float),
            typeof(string),
            typeof(bool)
        };

        public void Save(object toSave, XmlTextWriter destination)
        {
            if (toSave == null) throw new NullReferenceException(nameof(toSave));
            if (destination == null) throw new NullReferenceException(nameof(destination));

            SaveInner(toSave, destination.WriteElementString);
        }

        //Not 99% sure ref is needed, but makes it explicit what happens and might prevent if anybody uses this on a
        //struct in the future
        public void Load(ref object target, XmlNode source)
        {
            if (target == null) throw new NullReferenceException(nameof(target));
            if (source == null) throw new NullReferenceException(nameof(source));

            LoadInner(ref target, field => source[field]?.InnerText);


        }

        private void LoadInner(ref object target, Func<string, string> read)
        {
            PropertyInfo[] properties = target.GetType().GetProperties();

            foreach (PropertyInfo property in properties)
            {
                if (property.GetCustomAttribute<SaveIgnorePropertyAttribute>() != null) continue;

                if (!AttemptSaveList.Contains(property.PropertyType)) continue;

                string name = property.GetCustomAttribute<SavePropertyAsAttribute>()?.Name ??
                              property.Name.ToLowerInvariant();


                string unparsed = read(name);

                if (unparsed == null) continue;

                try
                {
                    var converter = TypeDescriptor.GetConverter(property.PropertyType);

                    property.SetValue(target, converter.ConvertFrom(unparsed));
                }
                catch (Exception ex)
                {
                    Utils.BreakIfDebug();
                    Log.Exception(ex);
                }

            }
        }

        private static void SaveInner(object toSave, Action<string, string> save)
        {
            PropertyInfo[] properties = toSave.GetType().GetProperties();

            foreach (PropertyInfo property in properties)
            {
                if (property.GetCustomAttribute<SaveIgnorePropertyAttribute>() != null) continue;

                if (!AttemptSaveList.Contains(property.PropertyType)) continue;

                string name = property.GetCustomAttribute<SavePropertyAsAttribute>()?.Name ??
                              property.Name.ToLowerInvariant();


                save(name, property.GetValue(toSave)?.ToString() ?? "null");
            }
        }
    }
}