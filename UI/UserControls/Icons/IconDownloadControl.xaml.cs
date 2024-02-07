using System.Windows;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;
using NuGet;
using ShieldVSExtension.Common.Helpers;
using ShieldVSExtension.ViewModels;

namespace ShieldVSExtension.UI.UserControls.Icons;

/// <summary>
/// Interaction logic for IconDownloadControl.xaml
/// </summary>
public partial class IconDownloadControl
{
    public IconDownloadControl()
    {
        InitializeComponent();
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not PackIcon { IsInitialized: true }) return;
        if (Payload == null) return;

        // MessageBox.Show(Payload.Name);

        var helper = new NugetHelper();
        helper.InstallPackageAsync(Payload.Project, SemanticVersion.Parse("1.10.0")).GetAwaiter();
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
        typeof(IconDownloadControl),
        new PropertyMetadata(null));

    #endregion
}