using System.Windows;
using ShieldVSExtension.ViewModels;
using System.Windows.Controls;
using ShieldVSExtension.Common;
using ShieldVSExtension.Common.Extensions;
using ShieldVSExtension.Common.Helpers;
using ShieldVSExtension.Common.Models;
using ShieldVSExtension.Storage;

namespace ShieldVSExtension.UI.UserControls
{
    /// <summary>
    /// Interaction logic for SettingsControl.xaml
    /// </summary>
    public partial class SettingsControl
    {
        public SecureLocalStorage LocalStorage { get; set; }

        // private readonly ProjectViewModel _vm;

        public SettingsControl()
        {
            InitializeComponent();
            // _vm = new ProjectViewModel();
            // DataContext = _vm;
        }

        private void SaveButtonOnClick(object sender, RoutedEventArgs e)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            if (sender is not Button { IsInitialized: true }) return;

            if (Payload == null) return;

            LocalStorage = new SecureLocalStorage(new CustomLocalStorageConfig(null, Globals.ShieldConfigurationFileName)
                .WithDefaultKeyBuilder());

            var data = LocalStorage.Get<ShieldConfiguration>(Globals.ShieldLocalStorageKey) ?? new ShieldConfiguration
            {
                Name = "MyConfiguration",
                Preset = EPresetType.Optimized.ToFriendlyString(),
                ProjectToken = "abc123",
                ProtectionSecret = "my-secret",
                Enabled = true,
                RunConfiguration = "Release",
                Protections = []
            };

            LocalStorage.Set(Globals.ShieldLocalStorageKey, data);

            FileManager.WriteJsonShieldConfiguration(Payload.FolderName,
                JsonHelper.Stringify(LocalStorage.Get<ShieldConfiguration>(Globals.ShieldLocalStorageKey)));

            MessageBox.Show($"Saving for {Payload.Name}");
        }

        #region Commands

        public ProjectViewModel Payload
        {
            get => (ProjectViewModel)GetValue(PayloadProperty);
            set
            {
                SetValue(PayloadProperty, value);
                // _vm.Payload = value;

                if (value != null)
                {
                    value.PropertyChanged += (sender, args) =>
                    {
                        if (args.PropertyName == "Name")
                        {
                            MessageBox.Show("Name changed");
                        }
                    };
                }
            }
        }

        public static readonly DependencyProperty PayloadProperty = DependencyProperty.Register(
            nameof(Payload),
            typeof(ProjectViewModel),
            typeof(SettingsControl),
            new PropertyMetadata(null));

        #endregion
    }
}