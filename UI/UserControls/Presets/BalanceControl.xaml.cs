using System.Windows;
using ShieldVSExtension.Common.Helpers;
using ShieldVSExtension.Common.Models;
using ShieldVSExtension.Common;
using ShieldVSExtension.Storage;
using ShieldVSExtension.ViewModels;
using ShieldVSExtension.Common.Extensions;

namespace ShieldVSExtension.UI.UserControls.Presets;

/// <summary>
/// Interaction logic for BalanceControl.xaml
/// </summary>
public partial class BalanceControl
{
    public SecureLocalStorage LocalStorage { get; set; }
    public ProjectViewModel Payload { get; set; }

    public BalanceControl()
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
        if (preset != EPresetType.Balance || Payload == null) return;

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

            var balance = EPresetType.Balance.ToFriendlyString();
            if (data.Preset == balance) return;

            data.Preset = balance;
            LocalStorage.Set(Payload.Project.UniqueName, data);

            FileManager.WriteJsonShieldConfiguration(Payload.FolderName,
                JsonHelper.Stringify(LocalStorage.Get<ShieldConfiguration>(Payload.Project.UniqueName)));
        }
        catch (System.Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }
}