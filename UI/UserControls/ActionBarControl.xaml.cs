using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using NuGet;
using ShieldVSExtension.Common.Helpers;
using ShieldVSExtension.ViewModels;

namespace ShieldVSExtension.UI.UserControls;

public partial class ActionBarControl
{
    // private readonly SharedViewModel _vm;

    public bool Loading { get; set; }

    public ActionBarControl()
    {
        InitializeComponent();
        // _vm = new SharedViewModel();
        // DataContext = _vm;

        ViewModelBase.ProjectChangedHandler += OnProjectChanged;
        ViewModelBase.InstalledHandler += OnInstalled;
    }

    private void OnInstalled(bool installed)
    {
        ActiveButton.IsChecked = installed;
    }

    private void OnProjectChanged(ProjectViewModel payload)
    {
        Payload = payload;

        LoaderControl.Visibility = Visibility.Visible;
        ActiveButton.Visibility = Visibility.Collapsed;

        var helper = new NugetHelper();
        var isInstalled = helper.IsPackageInstalled(payload.Project, SemanticVersion.Parse("1.10.0"));

        ActiveButton.IsChecked = isInstalled;

        LoaderControl.Visibility = Visibility.Collapsed;
        ActiveButton.Visibility = Visibility.Visible;
    }

    private async void ActiveOnChanged(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleButton { IsInitialized: true } control) return;

        control.Visibility = Visibility.Collapsed;
        LoaderControl.Visibility = Visibility.Visible;

        await Task.Delay(1500);

        try
        {
            var helper = new NugetHelper();
            var installed = helper.IsPackageInstalled(Payload.Project, SemanticVersion.Parse("1.10.0"));

            if (!installed)
            {
                await helper.InstallPackageAsync(Payload.Project, SemanticVersion.Parse("1.10.0"));
            }
            else
            {
                await helper.UninstallPackageAsync(Payload.Project);
            }

            ActiveButton.IsChecked = !installed;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
        finally
        {
            await Task.Delay(1500);

            LoaderControl.Visibility = Visibility.Collapsed;
            ActiveButton.Visibility = Visibility.Visible;
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
        typeof(ActionBarControl),
        new PropertyMetadata(null));

    #endregion
}