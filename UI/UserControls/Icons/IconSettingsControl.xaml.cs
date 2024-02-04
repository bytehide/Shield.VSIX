using MaterialDesignThemes.Wpf;
using ShieldVSExtension.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace ShieldVSExtension.UI.UserControls.Icons
{
    /// <summary>
    /// Interaction logic for IconSettingsControl.xaml
    /// </summary>
    public partial class IconSettingsControl
    {
        public IconSettingsControl()
        {
            InitializeComponent();
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not PackIcon { IsInitialized: true }) return;
            if (Payload == null) return;

            MessageBox.Show($"settings for {Payload.Name} project");
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
            typeof(IconSettingsControl),
            new PropertyMetadata(null));

        #endregion
    }
}