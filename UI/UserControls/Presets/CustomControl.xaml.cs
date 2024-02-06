using System.Threading.Tasks;
using ShieldVSExtension.Storage;
using ShieldVSExtension.ViewModels;
using System.Windows;
using ShieldVSExtension.Common;
using ShieldVSExtension.Common.Extensions;
using ShieldVSExtension.Common.Helpers;
using ShieldVSExtension.Common.Models;

namespace ShieldVSExtension.UI.UserControls.Presets
{
    /// <summary>
    /// Interaction logic for CustomControl.xaml
    /// </summary>
    public partial class CustomControl
    {
        public SecureLocalStorage LocalStorage { get; set; }
        public ProjectViewModel Payload { get; set; }

        public CustomControl()
        {
            InitializeComponent();

            // Loaded += OnLoaded;
            ViewModelBase.ProjectChangedHandler += OnRefresh;
            ViewModelBase.TabSelectedHandler += OnSelected;
        }

        private void OnRefresh(ProjectViewModel payload)
        {
            if (payload == null) return;

            Payload = payload;
            LoadDataAsync().GetAwaiter();
        }

        private void OnSelected(EPresetType preset)
        {
            if (preset != EPresetType.Custom || Payload == null) return;

            SaveConfiguration();
        }

        private async Task LoadDataAsync()
        {
            var result = await Common.Services.ProtectionService.GetAllByToken();
            if (result.Length > 0)
            {
                var protections =
                    Newtonsoft.Json.JsonConvert.DeserializeObject<Common.Models.ProtectionModel[]>(result);
                ProtectionListBox.ItemsSource = protections;
            }
            else
            {
                MessageBox.Show("Error while fetching data from server");
            }
        }

        private void SaveConfiguration()
        {
            try
            {
                Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

                LocalStorage = new SecureLocalStorage(new CustomLocalStorageConfig(null, Globals.ShieldLocalStorageName)
                    .WithDefaultKeyBuilder());
                var data = LocalStorage.Get<ShieldConfiguration>(Payload.Project.UniqueName) ??
                           new ShieldConfiguration();
                if (string.IsNullOrWhiteSpace(data.ProjectToken)) return;

                var buff = data.Preset;
                var custom = EPresetType.Custom.ToFriendlyString();
                if (data.Preset == custom) return;

                data.Preset = custom;
                LocalStorage.Set(Payload.Project.UniqueName, data);

                FileManager.WriteJsonShieldConfiguration(Payload.FolderName,
                    JsonHelper.Stringify(LocalStorage.Get<ShieldConfiguration>(Payload.Project.UniqueName)));
            }
            catch (System.Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
    }
}