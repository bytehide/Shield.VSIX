using ShieldVSExtension.Common.Configuration;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ShieldVSExtension.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        #region TargetDirectory Property

        private string _targetDirectory;

        public string TargetDirectory
        {
            get => _targetDirectory;
            set
            {
                if (_targetDirectory == value) return;

                _targetDirectory = value;
                OnPropertyChanged(nameof(TargetDirectory));
            }
        }

        #endregion

        #region ProjectPreset Property

        private ProjectPreset _projectPreset;

        public ProjectPreset ProjectPreset
        {
            get => _projectPreset;
            set
            {
                if (_projectPreset == value) return;

                _projectPreset = value;
                OnPropertyChanged(nameof(ProjectPreset));
            }
        }

        #endregion

        #region CreateShieldProjectIfNotExists Property

        private bool _createShieldProjectIfNotExists;

        public bool CreateShieldProjectIfNotExists
        {
            get => _createShieldProjectIfNotExists;
            set
            {
                if (_createShieldProjectIfNotExists == value) return;

                _createShieldProjectIfNotExists = value;
                OnPropertyChanged(nameof(CreateShieldProjectIfNotExists));
            }
        }

        #endregion

        #region FindCustomConfigurationFile Property

        private bool _findCustomConfigurationFile;

        public bool FindCustomConfigurationFile
        {
            get => _findCustomConfigurationFile;
            set
            {
                if (_findCustomConfigurationFile == value) return;

                _findCustomConfigurationFile = value;
                OnPropertyChanged(nameof(FindCustomConfigurationFile));
            }
        }

        #endregion

        #region IsValidClient Property

        private bool _isValidClient = true;

        public bool IsValidClient
        {
            get => _isValidClient;
            set
            {
                if (_isValidClient == value) return;

                _isValidClient = value;
                OnPropertyChanged(nameof(IsValidClient));
            }
        }

        #endregion

        #region SelectedProject Property

        private ProjectViewModel _selectedProject;

        public ProjectViewModel SelectedProject
        {
            get => _selectedProject;
            set
            {
                if (_selectedProject == value) return;

                _selectedProject = value;
                // _payload = value;

                // value.PropertyChanged += (sender, args) =>
                // {
                //     if (args.PropertyName == nameof(ProjectViewModel.Name))
                //     {
                //         ShieldProjectName = value.Name;
                //     }
                // };

                OnPropertyChanged(nameof(SelectedProject));
            }
        }

        #endregion

        #region Packages Property

        private List<string> _packages;

        public List<string> Packages
        {
            get => _packages;
            set
            {
                if (_packages == value) return;

                _packages = value;
                OnPropertyChanged(nameof(Packages));
            }
        }

        #endregion

        #region SelectedProjects Property

        public ICollection<ProjectViewModel> SelectedProjects { get; set; }

        #endregion

        #region Projects Property

        public ICollection<ProjectViewModel> Projects { get; set; }

        #endregion

        #region ShieldProjectName Property

        private string _shieldProjectName;

        public string ShieldProjectName
        {
            get => _shieldProjectName;
            set
            {
                if (_shieldProjectName == value) return;

                _shieldProjectName = value;
                OnPropertyChanged(nameof(ShieldProjectName));
            }
        }

        #endregion

        #region ShieldProjectEdition Property

        private string _shieldProjectEdition;

        public string ShieldProjectEdition
        {
            get => _shieldProjectEdition;
            set
            {
                if (_shieldProjectEdition == value) return;

                _shieldProjectEdition = value;
                OnPropertyChanged(nameof(ShieldProjectEdition));
            }
        }

        #endregion

        #region BuildConfiguration Property

        private string _buildConfiguration;

        public string BuildConfiguration
        {
            get => _buildConfiguration;
            set
            {
                if (_buildConfiguration == value) return;

                _buildConfiguration = value;
                OnPropertyChanged(nameof(BuildConfiguration));
            }
        }

        #endregion

        private ProjectViewModel _payload;

        public ProjectViewModel Payload
        {
            get => _payload;
            set
            {
                if (_payload == value) return;

                _payload = value;
                OnPropertyChanged(nameof(Payload));
            }
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // public event PropertyChangedEventHandler PropertyChanged;
        // 
        // protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        // {
        //     PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        // }
        // 
        // protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        // {
        //     if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        //     field = value;
        //     OnPropertyChanged(propertyName);
        //     return true;
        // }

        #endregion
    }
}