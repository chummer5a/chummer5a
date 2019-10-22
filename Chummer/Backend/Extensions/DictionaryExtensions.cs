using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Chummer
{
    internal static class DictionaryExtensions
    {
        public static IDictionary<string, object> ToDictionary(this object source)
        {
            var fields = source.GetType().GetFields(
                BindingFlags.GetField |
                BindingFlags.Public |
                BindingFlags.Instance).ToDictionary
            (
                propInfo => propInfo.Name,
                propInfo => propInfo.GetValue(source) ?? string.Empty
            );

            var properties = source.GetType().GetProperties(
                BindingFlags.GetField |
                BindingFlags.GetProperty |
                BindingFlags.Public |
                BindingFlags.Instance).ToDictionary
            (
                propInfo => propInfo.Name,
                propInfo => propInfo.GetValue(source, null) ?? string.Empty
            );

            return fields.Concat(properties).ToDictionary(key => key.Key, value => value.Value); ;
        }
        public static bool EqualsByValue(this Dictionary<string, bool> firstDic, Dictionary<string, bool> secondDic)
        {
            if (firstDic.Count != secondDic.Count)
                return false;
            if (firstDic.Keys.Except(secondDic.Keys).Any())
                return false;
            if (secondDic.Keys.Except(firstDic.Keys).Any())
                return false;
            return firstDic.All(pair =>
              pair.Value.ToString().Equals(secondDic[pair.Key].ToString())
            );
        }
        public static bool IsAnonymousType(this object instance)
        {

            if (instance == null)
                return false;

            return instance.GetType().Namespace == null;
        }
        public static IDictionary<string, object> ToFlattenDictionary(this object source, string parentPropertyKey = null, IDictionary<string, object> parentPropertyValue = null)
        {
            var propsDic = parentPropertyValue ?? new Dictionary<string, object>();
            foreach (var item in source.ToDictionary())
            {
                var key = string.IsNullOrEmpty(parentPropertyKey) ? item.Key : $"{parentPropertyKey}.{item.Key}";
                if (item.Value.IsAnonymousType())
                    return item.Value.ToFlattenDictionary(key, propsDic);
                propsDic.Add(key, item.Value);
            }
            return propsDic;
        }
    }
}
