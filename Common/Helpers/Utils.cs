using System.Diagnostics;
using System.Reflection;
using System;

namespace ShieldVSExtension.Common.Helpers;

public struct Version
{
    public string Major;
    public string Minor;
    public string Build;
    public string Get() => $"{Major}.{Minor}.{Build}";
}

internal static class Utils
{
    public static Version GetVersionNumber()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
        var versions = versionInfo.FileVersion.Split('.');
        Version version = default;
        if (versions.Length < 3) return version;

        version.Major = versions[0];
        version.Minor = versions[1];
        version.Build = versions[2];

        return version;
    }

    public static string Truncate(this string value, int maxLength, string tail = "...") =>
        value.Length <= maxLength ? value : $"{value.Substring(0, maxLength)}{tail}";

    public static void GoToWebsite(string path) => Process.Start(path);
}