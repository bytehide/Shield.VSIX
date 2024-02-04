using System.Windows;
using ShieldVSExtension.ViewModels;
using System.Windows.Input;

namespace ShieldVSExtension.UI.UserControls
{
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
            if (Payload == null) return;

            MessageBox.Show(Payload.Name);

            // var helper = new NugetHelper();
            // helper.InstallPackageAsync(Payload.Project, SemanticVersion.Parse("1.10.0")).GetAwaiter();
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
}