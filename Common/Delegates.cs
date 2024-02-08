using ShieldVSExtension.ViewModels;

namespace ShieldVSExtension.Common;

internal class Delegates
{
    public delegate void ProjectChangedHandler(ProjectViewModel payload);

    public delegate void TabSelectedHandler(EPresetType preset);

    public delegate void IsMsbuilderInstalledHandler(bool installed);

    public delegate void VsixVersionHandler(VersionInfo info);
}