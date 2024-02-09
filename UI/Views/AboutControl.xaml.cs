using System.Windows;
using System.Windows.Controls;
using ShieldVSExtension.Common.Helpers;

namespace ShieldVSExtension.UI.Views;

/// <summary>
/// Interaction logic for AboutControl.xaml
/// </summary>
public partial class AboutControl
{
    public AboutControl()
    {
        InitializeComponent();

        var version = Utils.GetVersionNumber();
        VersionBox.Text = $"{version.Get()}";
        // CopyBox.Text = $"© 2012-{DateTime.Now.Year}, ByteHide Solutions S.L. All rights reserved";
    }

    private void ButtonSiteOnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button) return;
        Utils.GoToWebsite("https://www.bytehide.com");
    }

    private void ButtonTwitterOnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button) return;
        Utils.GoToWebsite("https://twitter.com/byte_hide");
    }
}