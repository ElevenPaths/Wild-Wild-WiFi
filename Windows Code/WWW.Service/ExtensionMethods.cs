using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WildWildWifi.Service
{
    public static class ExtensionMethods
    {
        public static void AddOrUpdate(this KeyValueConfigurationCollection values, string key, string value)
        {
            if (values.AllKeys.Any(p => p.Equals(key)))
            {
                values[key].Value = value;
            }
            else
            {
                values.Add(key, value);
            }
        }

        public static T ReadOrDefault<T>(this KeyValueConfigurationCollection values, string key)
        {
            if (values.AllKeys.Any(p => p.Equals(key)))
            {
                return (T)Convert.ChangeType(values[key].Value, typeof(T));
            }
            else
            {
                return default(T);
            }
        }
    }
}
