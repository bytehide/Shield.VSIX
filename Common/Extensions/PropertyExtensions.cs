using System;
using EnvDTE;

namespace ShieldVSExtension.Common.Extensions;

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