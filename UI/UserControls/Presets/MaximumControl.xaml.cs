using System.Windows;
using ShieldVSExtension.Common;
using ShieldVSExtension.Common.Extensions;
using ShieldVSExtension.Common.Helpers;
using ShieldVSExtension.Common.Models;
using ShieldVSExtension.Storage;
using ShieldVSExtension.ViewModels;

namespace ShieldVSExtension.UI.UserControls.Presets;

/// <summary>
/// Interaction logic for MaximumControl.xaml
/// </summary>
public partial class MaximumControl
{
    public SecureLocalStorage LocalStorage { get; set; }
    public ProjectViewModel Payload { get; set; }

    public MaximumControl()
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
        if (preset != EPresetType.Maximum || Payload == null) return;

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

            var maximum = EPresetType.Maximum.ToFriendlyString();
            if (data.Preset == maximum) return;

            data.Preset = maximum;
            LocalStorage.Set(Payload.Project.UniqueName, data);

            FileManager.WriteJsonShieldConfiguration(Payload.FolderName,
                JsonHelper.Stringify(LocalStorage.Get<ShieldConfiguration>(Payload.Project.UniqueName)));
        }
        catch (System.Exception e)
        {
            MessageBox.Show(e.Message);
        }
    }
}