using ShieldVSExtension.ViewModels;
using System.Windows;

namespace ShieldVSExtension.UI.UserControls.Icons
{
    /// <summary>
    /// Interaction logic for IconSettingsControl.xaml
    /// </summary>
    public partial class IconSettingsControl
    {
        private readonly ProjectViewModel _vm;

        public IconSettingsControl()
        {
            InitializeComponent();
            _vm = new ProjectViewModel();
            DataContext = _vm;
        }

        // private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        // {
        //     if (sender is not PackIcon { IsInitialized: true }) return;
        //     if (Payload == null) return;
        // 
        //     MessageBox.Show($"settings for {Payload.Name} project");
        // }

        #region Commands

        // public object CheckProjectCommand { get; }

        public ProjectViewModel Payload
        {
            get => (ProjectViewModel)GetValue(PayloadProperty);
            set
            {
                SetValue(PayloadProperty, value);
                _vm.Payload = value;
            }
        }

        public static readonly DependencyProperty PayloadProperty = DependencyProperty.Register(
            nameof(Payload),
            typeof(ProjectViewModel),
            typeof(IconSettingsControl),
            new PropertyMetadata(null));

        #endregion
    }
}