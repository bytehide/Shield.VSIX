using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Shield.Client;
using Shield.Client.Fr;
using ShieldVSExtension.Configuration;
using ShieldVSExtension.InternalSecureStorage;
using ShieldVSExtension.UI_Extensions;

namespace ShieldVSExtension.ToolWindows
{
    public partial class ConfigurationWindowControl : Window
    {
        private readonly ConfigurationViewModel _viewModel;
        private const string ExtensionConfigurationFile = "ExtensionConfiguration";

        public InternalSecureStorage.SecureLocalStorage LocalStorage { get; set; }

        private ShieldExtensionConfiguration ExtensionConfiguration { get; }

        public ConfigurationWindowControl(ConfigurationViewModel viewModel)
        {
            InitializeComponent();

            _viewModel = viewModel;
            DataContext = viewModel;

            LocalStorage = new SecureLocalStorage(
                new CustomLocalStorageConfig(null, "DotnetsaferShieldForVisualStudio").WithDefaultKeyBuilder()
            );

            ExtensionConfiguration = LocalStorage.Exists(ExtensionConfigurationFile) ?
                LocalStorage.Get<ShieldExtensionConfiguration>(ExtensionConfigurationFile) :
                new ShieldExtensionConfiguration();

            if (!string.IsNullOrEmpty(ExtensionConfiguration.ApiToken))
                try
                {
                    _ = ShieldClient.CreateInstance(ExtensionConfiguration.ApiToken);
                    _viewModel.IsValidClient = true;
                    ApiKeyBox.Password = ExtensionConfiguration.ApiToken;
                    ConnectButton.IsEnabled = false;
                }
                catch (Exception)
                {
                    _viewModel.IsValidClient = false;
                }
            else _viewModel.IsValidClient = false;

            if (!_viewModel.IsValidClient)
                ShieldControl.SelectedIndex = 1;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _ = ShieldClient.CreateInstance(ApiKeyBox.Password);
                _viewModel.IsValidClient = true;
                ExtensionConfiguration.ApiToken = ApiKeyBox.Password;
                ShieldControl.SelectedIndex = 0;
                SaveExtensionConfiguration();
            }
            catch (Exception)
            {
                _viewModel.IsValidClient = false;
                MessageBox.Show("The api key is not valid, check that it has not been revoked and the associated scopes.","Invalid Shield API Key",MessageBoxButton.OK,MessageBoxImage.Error);
            }
        }

        private void SaveExtensionConfiguration()
        => LocalStorage.Set(ExtensionConfigurationFile, ExtensionConfiguration);
        

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

        private void EnableMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Enable(true);
        }

        private void DisableMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Enable(false);
        }

        private void AddCustomProtectionConfigMenuItem_Click(object sender, RoutedEventArgs e)
        {
            foreach (var viewModelSelectedProject in _viewModel.SelectedProjects)
            {
                viewModelSelectedProject.InheritFromProject = false;
                viewModelSelectedProject.ApplicationPreset = _viewModel.ProjectPresets.First(preset =>
                    preset.Name.ToLower().Equals(((MenuItem) sender).Header.ToString().ToLower()));
            }
        }

        private void OutputFilesComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            var comboBox = (ComboBox)sender;
            comboBox.Focus();

            var projectViewModel = _viewModel.SelectedProject;
            if (projectViewModel == null)
                return;

            var path = projectViewModel.OutputFullPath;

            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            {
                comboBox.ItemsSource = null;
                return;
            }

            var files = Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly)
                .Select(Path.GetFileName)
                .OrderByDescending(p => p.StartsWith(projectViewModel.Name))
                .ThenBy(Path.GetFileNameWithoutExtension)
                .ToArray();

            comboBox.ItemsSource = files;
        }

        private void ApiKeyBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            ConnectButton.IsEnabled = ExtensionConfiguration.ApiToken != ApiKeyBox.Password;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Save();
            DialogResult = true;
            Close();
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void InheritConfigFromGlobal_Copy_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void ProtectionsPresetProject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}