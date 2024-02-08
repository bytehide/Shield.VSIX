namespace ShieldVSExtension.Common;

public struct Version
{
    public string Major;
    public string Minor;
    public string Build;

    public readonly string Get() => $"{Major}.{Minor}.{Build}";
}

public struct VersionInfo
{
    public bool CanUpdate;
    public bool ForceUpdate;
    public string Message;
}