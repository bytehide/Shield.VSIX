using ShieldVSExtension.Common.Helpers;
using ShieldVSExtension.Common.Models;
using ShieldVSExtension.Common;
using ShieldVSExtension.Storage;
using ShieldVSExtension.ViewModels;
using System.Windows.Controls;
using ShieldVSExtension.Common.Extensions;

namespace ShieldVSExtension.UI.UserControls.Presets;

/// <summary>
/// Interaction logic for OptimizedControl.xaml
/// </summary>
public partial class OptimizedControl : UserControl
{
    public SecureLocalStorage LocalStorage { get; set; }
    public ProjectViewModel Payload { get; set; }

    public OptimizedControl()
    {
        InitializeComponent();

        ViewModelBase.ProjectChangedHandler += OnRefresh;
        ViewModelBase.TabSelectedHandler += OnSelected;
    }

    private void OnRefresh(ProjectViewModel payload)
    {
        if (payload == null) return;

        Payload = payload;
    }

    private void OnSelected(EPresetType preset)
    {
        if (preset != EPresetType.Optimized || Payload == null) return;

        SaveConfiguration();
    }

    private void SaveConfiguration()
    {
        try
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            LocalStorage = new SecureLocalStorage(new CustomLocalStorageConfig(null, Globals.ShieldLocalStorageName)
                .WithDefaultKeyBuilder());
            var data = LocalStorage.Get<ShieldConfiguration>(Payload.Project.UniqueName) ?? new ShieldConfiguration();
            if (string.IsNullOrWhiteSpace(data.ProjectToken)) return;

            var optimized = EPresetType.Optimized.ToFriendlyString();
            if (data.Preset == optimized) return;

            data.Preset = optimized;
            LocalStorage.Set(Payload.Project.UniqueName, data);

            FileManager.WriteJsonShieldConfiguration(Payload.FolderName,
                JsonHelper.Stringify(LocalStorage.Get<ShieldConfiguration>(Payload.Project.UniqueName)));
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.Message);
        }
    }
}