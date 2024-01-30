using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;

namespace ShieldVSExtension.Helpers
{
    public static class PropertiesExtensions
    {
        public static Property GetPropertyOrDefault(this EnvDTE.Properties properties, string propertyName)
        {
            try
            {
                return properties.Item(propertyName);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        public static T GetPropertyOrDefault<T>(this EnvDTE.Properties properties, string propertyName)
            where T : class
        {
            var property = GetPropertyOrDefault(properties, propertyName);

            return (T)property?.Value;
        }

        public static object TryGetPropertyValueOrDefault(this EnvDTE.Properties properties, string propertyName)
        {
            try
            {
                return properties.Item(propertyName).Value;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
