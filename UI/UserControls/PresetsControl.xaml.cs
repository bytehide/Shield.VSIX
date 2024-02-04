using System.Windows;
using EnvDTE;
using ShieldVSExtension.ViewModels;

namespace ShieldVSExtension.UI.UserControls
{
    /// <summary>
    /// Interaction logic for PresetsControl.xaml
    /// </summary>
    public partial class PresetsControl
    {
        // private SharedViewModel _vm;

        public PresetsControl()
        {
            InitializeComponent();
            RootLayout.DataContext = this;
            // _vm = vm;
            // DataContext = this;

            Loaded += PresetsControl_Loaded;
        }

        private async void PresetsControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var result = await Common.Services.ProtectionService.GetAllByToken();

            if (result.Length > 0)
            {
                var protections =
                    Newtonsoft.Json.JsonConvert.DeserializeObject<Common.Models.ProtectionModel[]>(result);
                ProtectionListBox.ItemsSource = protections;

                // search shield.config.json in root directory and create it if not exists
            }
            else
            {
                MessageBox.Show("Error while fetching data from server");
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
    }
}