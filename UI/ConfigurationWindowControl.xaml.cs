using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ShieldVSExtension.Common.Configuration;
using ShieldVSExtension.Common.Helpers;
using ShieldVSExtension.Storage;
using ShieldVSExtension.Storage.Configurations;
using ShieldVSExtension.ViewModels;

namespace ShieldVSExtension.UI
{
    public partial class ConfigurationWindowControl
    {
        private readonly ConfigurationViewModel _vm;
        private const string ExtensionConfigurationFile = "ExtensionConfiguration";

        public SecureLocalStorage LocalStorage { get; set; }

        private ShieldExtensionConfiguration ExtensionConfiguration { get; }

        public ConfigurationWindowControl(ConfigurationViewModel vm)
        {
            InitializeComponent();

            _vm = vm;
            DataContext = vm;

            LocalStorage = new SecureLocalStorage(
                new CustomLocalStorageConfig(null, "DotnetsaferShieldForVisualStudio").WithDefaultKeyBuilder()
            );

            ExtensionConfiguration = LocalStorage.Exists(ExtensionConfigurationFile)
                ? LocalStorage.Get<ShieldExtensionConfiguration>(ExtensionConfigurationFile)
                : new ShieldExtensionConfiguration();

            if (!string.IsNullOrEmpty(ExtensionConfiguration.ApiToken))
            {
                try
                {
                    _vm.IsValidClient = true;
                    ApiKeyBox.Password = ExtensionConfiguration.ApiToken;
                    ConnectButton.Content = ExtensionConfiguration.ApiToken != ApiKeyBox.Password
                        ? "Connect and save"
                        : "Retry connection";
                }
                catch (Exception)
                {
                    _vm.IsValidClient = false;
                }
            }

            else _vm.IsValidClient = false;

            if (!_vm.IsValidClient)
            {
                ShieldControl.SelectedIndex = 1;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _vm.IsValidClient = true;
                ExtensionConfiguration.ApiToken = ApiKeyBox.Password;
                ShieldControl.SelectedIndex = 0;
                SaveExtensionConfiguration();
            }
            catch (Exception)
            {
                _vm.IsValidClient = false;
                MessageBox.Show(
                    "The api key is not valid, check that it has not been revoked and the associated scopes.",
                    "Invalid Shield API Key", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveExtensionConfiguration()
            => LocalStorage.Set(ExtensionConfigurationFile, ExtensionConfiguration);


        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var removedItems = e.RemovedItems.OfType<ProjectViewModel>();
            foreach (var item in removedItems)
                _vm.SelectedProjects.Remove(item);

            var addedItems = e.AddedItems.OfType<ProjectViewModel>()
                .Except(_vm.SelectedProjects);
            foreach (var item in addedItems)
                _vm.SelectedProjects.Add(item);
        }


        private void ListBox_Loaded(object sender, RoutedEventArgs e)
        {
            ((ListBox)sender).ScrollIntoView(_vm.SelectedProject);
        }

        private void EnableMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _vm.Enable(true);
        }

        private void DisableMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _vm.Enable(false);
        }

        private void AddCustomProtectionConfigMenuItem_Click(object sender, RoutedEventArgs e)
        {
            foreach (var viewModelSelectedProject in _vm.SelectedProjects)
            {
                viewModelSelectedProject.InheritFromProject = false;
                viewModelSelectedProject.ApplicationPreset = _vm.ProjectPresets.First(preset =>
                    preset.Name.ToLower().Equals(((MenuItem)sender).Header.ToString().ToLower()));
            }
        }

        private void OutputFilesComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            var comboBox = (ComboBox)sender;
            comboBox.Focus();

            var projectViewModel = _vm.SelectedProject;
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
            ConnectButton.Content = ExtensionConfiguration.ApiToken != ApiKeyBox.Password
                ? "Connect and save"
                : "Retry connection";
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            _vm.Save();
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

        private void GitHubButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/dotnetsafer/Shield.VSIX");
        }

        private void WebSiteButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://dotnetsafer.com");
        }

        private void DocumentationButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://dotnetsafer.com/docs/product/shield-vs/1.0");
        }

        private void Generate_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("https://dotnetsafer.com/docs/product/shield-vs/1.0/Authentication");
        }

        private void ProtectionsPreset_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _vm.ShieldProjectEdition = (string)e.AddedItems[0];
        }

        private void ReadMore_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("https://dotnetsafer.com/docs/product/shield-vs/1.0/Credits");
        }

        private void AddProject_Click(object sender, RoutedEventArgs e)
        {
            var helper = new NugetHelper();
            helper.InstallPackageAsync(_vm.Projects.First().Project).GetAwaiter();
        }

        private void RemoveProject_Click(object sender, RoutedEventArgs e)
        {
            var helper = new NugetHelper();
            helper.UninstallPackageAsync(_vm.Projects.First().Project).GetAwaiter();
        }
    }
}