using System.Linq;
using System.Windows;
using MaterialDesignThemes.Wpf;
using NuGet;
using ShieldVSExtension.Common;
using ShieldVSExtension.Common.Helpers;
using ShieldVSExtension.Common.Services;
using ShieldVSExtension.ViewModels;
using Task = System.Threading.Tasks.Task;

namespace ShieldVSExtension.UI;

/// <summary>
///   Interaction logic for MainWindowControl.xaml
/// </summary>
public partial class MainWindowControl
{
    // private const string ExtensionConfigurationFile = "ExtensionConfiguration";
    private readonly MainViewModel _vm;

    public MainWindowControl(MainViewModel vm)
    {
        InitializeMaterialDesign();
        InitializeComponent();

        _vm = vm;
        DataContext = vm;

        Loaded += OnLoaded;
        ViewModelBase.IsMsbuilderInstalledHandler += OnInstalled;
    }

    private void OnInstalled(bool installed)
    {
        _vm.SelectedProject.Installed = installed;
    }

    // private void OnVsixVersion(VersionInfo info)
    // {
    //     if (info is { ForceUpdate: false }) return;
    // 
    //     TabControl.IsEnabled = false;
    // }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ViewModelBase.ProjectChangedHandler.Invoke(_vm.Projects.FirstOrDefault());

        CheckVersionAsync().GetAwaiter();

        var helper = new NugetHelper();
        var isInstalled = helper.IsPackageInstalled(_vm.SelectedProject.Project, SemanticVersion.Parse("1.10.0"));
        ViewModelBase.IsMsbuilderInstalledHandler.Invoke(isInstalled);
    }

    private void InitializeMaterialDesign() => _ = new Card();

    private async Task CheckVersionAsync()
    {
        var vsixVersion = Utils.GetVersionNumber().Get();
        var info = await CoreService.GetVersionInfoAsync(EPackageType.Vsix, vsixVersion);

        if (info.CanUpdate || info.ForceUpdate)
        {
            if (info is { ForceUpdate: true })
            {
                TabControl.IsEnabled = false;
            }

            ViewModelBase.VsixVersionHandler.Invoke(info);
        }
    }
}