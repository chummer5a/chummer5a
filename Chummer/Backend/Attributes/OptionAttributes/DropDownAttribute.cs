using System;
using System.Collections.Generic;
using System.Reflection;

namespace Chummer.Backend.Attributes.OptionAttributes
{
    class DropDownAttribute : System.Attribute
    {
        private readonly string _method;
        public string[] RealValues { get; set; }
        public string[] DirectDisplay { get; set; }
        public string[] TranslatedDisplay { get; set; }
        public DropDownAttribute(string[] realValues)
        {
            RealValues = realValues;
        }

        public DropDownAttribute(string method)
        {
            _method = method;
        }

        public List<ListItem<string>> GetDisplayList()
        {

            List<ListItem<string>> returns = new List<ListItem<string>>();
            if (_method != null)
            {
                return ReflectionInvokeStaticMethod(_method);
            }
            else if (DirectDisplay != null)
            {
                for (int i = 0; i < RealValues.Length; i++)
                {
                    returns.Add(new ListItem(RealValues[i], DirectDisplay[i]));
                }
            }
            else if (TranslatedDisplay != null)
            {
                for (int i = 0; i < RealValues.Length; i++)
                {
                    returns.Add(new ListItem(RealValues[i], LanguageManager.GetString(TranslatedDisplay[i])));
                }
            }
            else
            {
                for (int i = 0; i < RealValues.Length; i++)
                {
                    returns.Add(new ListItem(RealValues[i], RealValues[i]));
                }
            }

            return returns;
        }

        private List<ListItem<string>> ReflectionInvokeStaticMethod(string fullName)
        {
            int index = fullName.LastIndexOf(".", StringComparison.Ordinal);
            string typeName = fullName.Substring(0, index);
            string methodName = fullName.Substring(index+1);

            Type type = Type.GetType(typeName);
            MethodInfo method = type.GetMethod(methodName);

            return (List<ListItem<string>>) method.Invoke(null, null);
        }
    }
}
