using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ShieldVSExtension.UI_Extensions;

namespace ShieldVSExtension.ToolWindows
{
    public partial class ConfigurationWindowControl : Window
    {
        private readonly ConfigurationViewModel _viewModel;
        public SecureLocalStorage.SecureLocalStorage LocalStorage { get; set; }

        public ConfigurationWindowControl(ConfigurationViewModel viewModel)
        {
            InitializeComponent();

            _viewModel = viewModel;
            DataContext = viewModel;

            LocalStorage = new SecureLocalStorage.SecureLocalStorage(
                new SecureLocalStorage.CustomLocalStorageConfig(null,"DotnetsaferShieldForVisualStudio").WithDefaultKeyBuilder()
            );        
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var storage = new SecureLocalStorage.SecureLocalStorage(new SecureLocalStorage.DefaultLocalStorageConfig());

            storage.Store("manuelo", "como estas");
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var removedItems = e.RemovedItems.OfType<ConfigurationViewModel.ProjectViewModel>();
            foreach (var item in removedItems)
                _viewModel.SelectedProjects.Remove(item);

            var addedItems = e.AddedItems.OfType<ConfigurationViewModel.ProjectViewModel>().Except(_viewModel.SelectedProjects);
            foreach (var item in addedItems)
                _viewModel.SelectedProjects.Add(item);
        }
    

        private void ListBox_Loaded(object sender, RoutedEventArgs e)
        {
            ((ListBox)sender).ScrollIntoView(_viewModel.SelectedProject);
        }
    }
}