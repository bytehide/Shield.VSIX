using System.Windows.Controls;
using System.Windows.Input;

namespace ShieldVSExtension.UI.UserControls
{
    /// <summary>
    /// Interaction logic for IconDownloadControl.xaml
    /// </summary>
    public partial class IconDownloadControl : UserControl
    {
        public IconDownloadControl()
        {
            InitializeComponent();
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // var nuget = new NugetHelper();
            // nuget.IsLatestVersionInstalled(Payload);
        }

        // #region Commands
        // 
        // public Project Payload
        // {
        //     get => (Project)GetValue(PayloadProperty);
        //     set => SetValue(PayloadProperty, value);
        // }
        // 
        // public static readonly DependencyProperty PayloadProperty = DependencyProperty.Register(
        //     nameof(Payload),
        //     typeof(Project),
        //     typeof(ActionBarControl),
        //     new PropertyMetadata(null));
        // 
        // #endregion
    }
}