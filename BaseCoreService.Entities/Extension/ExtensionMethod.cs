using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BaseCoreService.Entities.Extension
{
    public static class ExtensionMethod
    {
        public static string Test(this string str, string hehe)
        {
            return str + hehe;
        }

        public static void Set(this object obj, string propName, object value)
        {
            if (obj != null)
            {
                var props = obj.GetType().GetProperties();
                if (props.Length > 0)
                {
                    var prop = props.SingleOrDefault(p => (p.CanWrite && p.Name == propName));
                    if (prop != null)
                    {
                        prop.SetValue(obj, value, null);
                    }
                }
            }
        }

        public static object? Get(this object obj, string propName)
        {
            PropertyInfo prop = null;
            object? value = null;
            if (obj != null)
            {
                prop = obj.GetType().GetProperty(propName);
                if (prop != null)
                {
                    value = prop.GetValue(obj, null);
                }
            }
            return value;
        }

        public static T Get<T>(this object obj, string propName)
        {
            PropertyInfo prop = null;
            T value = default(T);
            if (obj != null)
            {
                prop = obj.GetType().GetProperty(propName);
                if (prop != null)
                {
                    value = (T?)prop.GetValue(obj, null);
                }
            }
            return value;
        }

        public static T Get<T>(this Dictionary<string, object> dic, string propName)
        {
            if (dic.TryGetValue(propName, out var value))
            {
                return (T)value;
            }
            return default(T);
        }

        public static object Get(this Dictionary<string, object> dic, string propName)
        {
            if (dic.TryGetValue(propName, out var value))
            {
                return value;
            }
            return default(object);
        }

        public static Dictionary<string, object> Set(this Dictionary<string, object> dic, Dictionary<string, object> dic2)
        {
            if (dic == null)
            {
                dic = new Dictionary<string, object>();
            }
            if (dic2 != null && dic2.Count > 0)
            {

                foreach (var item in dic2)
                {
                    dic.Set(item.Key, item.Value);
                }
            }

            return dic;

        }

        public static void Set(this Dictionary<string, object> dic, string key, object value)
        {
            if (dic.ContainsKey(key))
            {
                dic[key] = value;
            }
            else
            {
                dic.Add(key, value);
            }
        }

        public static Type GetPropertyType(this Type type, string propertyName)
        {
            PropertyInfo[] propertyInfos = type.GetProperties();
            PropertyInfo property = null;
            if (propertyInfos != null)
            {
                property = propertyInfos.SingleOrDefault(p => p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
            }

            if (property != null)
            {
                return property.PropertyType;
            }
            return typeof(object);
        }

        /// <summary>
        /// Set value for prop by type
        /// </summary>
        /// <author>MDLong</author>
        /// <param name="obj"></param>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        public static void SetProperty(this object obj, string propertyName, string value)
        {
            // Get the type of the object
            Type type = obj.GetType();

            // Get the property by name
            var property = type.GetProperty(propertyName);
            if (property != null)
            {
                TypeConverter typeConverter = TypeDescriptor.GetConverter(property.PropertyType);
                object convertedValue = typeConverter.ConvertFromString(value);

                // Set the value of the property
                property.SetValue(obj, convertedValue);
            }

        }

        public static Dictionary<string, object> ToDictionary(this object obj)
        {
            var res = obj.ToDictionary<object>();
            return res;
        }

        public static Dictionary<string, TValue> ToDictionary<TValue>(this object obj)
        {
            try
            {
                var json = JsonConvert.SerializeObject(obj);
                var dictionary = JsonConvert.DeserializeObject<Dictionary<string, TValue>>(json);
                return dictionary;
            }
            catch (Exception)
            {
                return [];
            }
        }
    }
}
