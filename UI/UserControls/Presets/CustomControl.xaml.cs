using System.Diagnostics;
using System.Threading.Tasks;
using ShieldVSExtension.Storage;
using ShieldVSExtension.ViewModels;
using System.Windows;
using System.Windows.Controls;
using ShieldVSExtension.Common;
using ShieldVSExtension.Common.Extensions;
using ShieldVSExtension.Common.Helpers;
using ShieldVSExtension.Common.Models;
using Microsoft.VisualStudio.Shell;

namespace ShieldVSExtension.UI.UserControls.Presets;

/// <summary>
/// Interaction logic for CustomControl.xaml
/// </summary>
public partial class CustomControl
{
    public SecureLocalStorage LocalStorage { get; set; }
    public ProjectViewModel Payload { get; set; }

    public CustomControl()
    {
        InitializeComponent();

        // Loaded += OnLoaded;
        ViewModelBase.ProjectChangedHandler += OnRefresh;
        ViewModelBase.TabSelectedHandler += OnSelected;
    }

    private void OnRefresh(ProjectViewModel payload)
    {
        if (payload == null) return;

        Payload = payload;
        LoadDataAsync().GetAwaiter();
    }

    private void OnSelected(EPresetType preset)
    {
        if (preset != EPresetType.Custom || Payload == null) return;

        SaveConfiguration();
        LoadDataAsync().GetAwaiter();
    }

    private async Task LoadDataAsync()
    {
        if (Payload == null) return;

        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        LocalStorage = new SecureLocalStorage(new CustomLocalStorageConfig(null, Globals.ShieldLocalStorageName)
            .WithDefaultKeyBuilder());

        var data = LocalStorage.Get<ShieldConfiguration>(Payload.Project.UniqueName) ?? new ShieldConfiguration();
        if (string.IsNullOrWhiteSpace(data.ProjectToken)) return;

        if (data.Preset != EPresetType.Custom.ToFriendlyString()) return;

        var result = await Common.Services.ProtectionService.GetAllByTokenAsync(data.ProjectToken);

        if (result.Length > 0)
        {
            var protections = Newtonsoft.Json.JsonConvert.DeserializeObject<ProtectionModel[]>(result);
            foreach (var protection in protections)
            {
                if (data.Protections.ContainsKey(protection.Id))
                {
                    protection.IsSelected = true;
                }

                protection.IsEnabled = true;
            }

            ProtectionList.ItemsSource = protections;
            return;
        }

        MessageBox.Show("Error while fetching data from server");
    }


    private void SaveConfiguration()
    {
        try
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            LocalStorage = new SecureLocalStorage(new CustomLocalStorageConfig(null, Globals.ShieldLocalStorageName)
                .WithDefaultKeyBuilder());

            var data = LocalStorage.Get<ShieldConfiguration>(Payload.Project.UniqueName) ?? new ShieldConfiguration();
            if (string.IsNullOrWhiteSpace(data.ProjectToken)) return;

            var custom = EPresetType.Custom.ToFriendlyString();
            if (data.Preset == custom) return;

            data.Preset = custom;
            LocalStorage.Set(Payload.Project.UniqueName, data);

            FileManager.WriteJsonShieldConfiguration(Payload.FolderName,
                JsonHelper.Stringify(LocalStorage.Get<ShieldConfiguration>(Payload.Project.UniqueName)));
        }
        catch (System.Exception e)
        {
            Debug.Fail(e.Message);
        }
    }

    private void ProtectionOnSelected(object sender, RoutedEventArgs e)
    {
        if (sender is not CheckBox { IsInitialized: true } control) return;
        if (!control.IsMouseOver) return;

        ThreadHelper.ThrowIfNotOnUIThread();

        LocalStorage = new SecureLocalStorage(new CustomLocalStorageConfig(null, Globals.ShieldLocalStorageName)
            .WithDefaultKeyBuilder());

        var data = LocalStorage.Get<ShieldConfiguration>(Payload.Project.UniqueName) ?? new ShieldConfiguration();
        if (string.IsNullOrWhiteSpace(data.ProjectToken))
        {
            MessageBox.Show("You need a Project Token to enable a protection");
            return;
        }

        var protection = (ProtectionModel)control.DataContext;

        if (control.IsChecked == true)
        {
            data.Protections[protection.Id] = null;
        }
        else
        {
            data.Protections.Remove(protection.Id);
        }

        LocalStorage.Set(Payload.Project.UniqueName, data);

        FileManager.WriteJsonShieldConfiguration(Payload.FolderName,
            JsonHelper.Stringify(LocalStorage.Get<ShieldConfiguration>(Payload.Project.UniqueName)));
    }
}