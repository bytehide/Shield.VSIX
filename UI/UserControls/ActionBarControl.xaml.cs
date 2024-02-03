using System.Windows;
using EnvDTE;

namespace ShieldVSExtension.UI.UserControls
{
    /// <summary>
    /// Interaction logic for ActionBarControl.xaml
    /// </summary>
    public partial class ActionBarControl
    {
        // private SharedViewModel _vm;

        public ActionBarControl()
        {
            InitializeComponent();
            // RootLayout.DataContext = this;
            // _vm = vm;

            Loaded += PresetsControl_Loaded;
        }

        private void PresetsControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!TextBlock.IsInitialized) return;
            if (null == Payload)
            {
                TextBlock.Text = "No project selected";
                return;
            }

            TextBlock.Text = Payload?.Name;

            MessageBox.Show(Payload.Name);
        }

        #region Commands

        public Project Payload
        {
            get => (Project)GetValue(PayloadProperty);
            set => SetValue(PayloadProperty, value);
        }

        public static readonly DependencyProperty PayloadProperty = DependencyProperty.Register(
            nameof(Payload),
            typeof(Project),
            typeof(ActionBarControl),
            new PropertyMetadata(null));

        #endregion
    }
}