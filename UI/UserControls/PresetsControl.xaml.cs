using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ShieldVSExtension.Common;
using ShieldVSExtension.Common.Models;
using ShieldVSExtension.Storage;
using ShieldVSExtension.ViewModels;

namespace ShieldVSExtension.UI.UserControls;

/// <summary>
/// Interaction logic for PresetsControl.xaml
/// </summary>
public partial class PresetsControl
{
    public SecureLocalStorage LocalStorage { get; set; }
    // public bool HasSettings { get; set; }

    // private SharedViewModel _vm;

    public PresetsControl()
    {
        InitializeComponent();
        // RootLayout.DataContext = this;
        // _vm = vm;
        // DataContext = this;
        Loaded += OnLoaded;
        ViewModelBase.ProjectChangedHandler += OnRefresh;
        Unloaded += OnFree;
    }

    private void OnLoaded(object sender, RoutedEventArgs e) => Refresh();

    private void OnRefresh(ProjectViewModel payload)
    {
        Payload = payload;
        Refresh();
    }

    private void Refresh()
    {
        Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
        if (Payload == null) return;

        LocalStorage = new SecureLocalStorage(new CustomLocalStorageConfig(null, Globals.ShieldLocalStorageName)
            .WithDefaultKeyBuilder());

        var data = LocalStorage.Get<ShieldConfiguration>(Payload.Project.UniqueName);
        if (data == null || string.IsNullOrWhiteSpace(data.ProjectToken))
        {
            ToggleTabs(false);
            SettingsTab.IsSelected = true;

            return;
        }

        var preset = data?.Preset;
        if (preset == null) return;

        // HasSettings = true;
        ToggleTabs(true);

        switch (preset)
        {
            case "Maximum":
                MaximumTab.IsSelected = true;
                break;
            case "Balance":
                BalanceTab.IsSelected = true;
                break;
            case "Optimized":
                OptimizedTab.IsSelected = true;
                break;
            case "Custom":
                CustomTab.IsSelected = true;
                break;
            default:
                SettingsTab.IsSelected = true;
                break;
        }
    }

    private void ToggleTabs(bool isEnabled)
    {
        MaximumTab.IsEnabled = isEnabled;
        OptimizedTab.IsEnabled = isEnabled;
        BalanceTab.IsEnabled = isEnabled;
        CustomTab.IsEnabled = isEnabled;
    }

    private void OnSelectedTab(object sender, MouseButtonEventArgs e)
    {
        if (sender is not TabItem { IsInitialized: true } tabItem) return;

        var tabName = tabItem.Header.ToString();

        switch (tabName)
        {
            case "Maximum":
                ViewModelBase.TabSelectedHandler.Invoke(EPresetType.Maximum);
                break;
            case "Balance":
                ViewModelBase.TabSelectedHandler.Invoke(EPresetType.Balance);
                break;
            case "Optimized":
                ViewModelBase.TabSelectedHandler.Invoke(EPresetType.Optimized);
                break;
            case "Custom":
                ViewModelBase.TabSelectedHandler.Invoke(EPresetType.Custom);
                break;
            // case "Settings":
            //     ViewModelBase.TabSelectedHandler.Invoke(EPresetType.Settings);
            //     break;
        }
    }

    #region Commands

    public ProjectViewModel Payload
    {
        get => (ProjectViewModel)GetValue(PayloadProperty);
        set => SetValue(PayloadProperty, value);
    }

    public static readonly DependencyProperty PayloadProperty = DependencyProperty.Register(
        nameof(Payload),
        typeof(ProjectViewModel),
        typeof(PresetsControl),
        new PropertyMetadata(null));

    #endregion

    private void OnFree(object sender, RoutedEventArgs e)
    {
        ViewModelBase.ProjectChangedHandler -= OnRefresh;
        Unloaded -= OnFree;
    }
}