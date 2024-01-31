using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using ShieldVSExtension.Common.Configuration;
using ShieldVSExtension.Common.Helpers;

namespace ShieldVSExtension.ViewModels
{
    public sealed class MainViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Design-Time Ctor

#if DEBUG
        [Obsolete("For design-time only")]
        public MainViewModel()
        {
            TargetDirectory = "test";

            var p1 = new ProjectViewModel();
            p1.Name = "shield.exe";
            p1.IsEnabled = true;
            p1.FolderName = "common";
            p1.Files.Add(new ProjectFileViewModel { FileName = "file1" });
            p1.Files.Add(new ProjectFileViewModel { FileName = "file2" });
            p1.Files.Add(new ProjectFileViewModel { FileName = "file3" });
            p1.OutputFullPath = "C:\\Windows";
            p1.TargetDirectory = "C:\\Temp";

            var p2 = new ProjectViewModel();
            p2.Name = "shield2.exe";
            p2.IsEnabled = false;
            p1.FolderName = "common/lib";
            p2.Files.Add(new ProjectFileViewModel { FileName = "file4" });
            p2.Files.Add(new ProjectFileViewModel { FileName = "file5" });
            p2.Files.Add(new ProjectFileViewModel { FileName = "file6" });

            var p3 = new ProjectViewModel();
            p2.Name = "shield3.exe";
            p2.IsEnabled = true;
            p1.FolderName = "common";
            p3.OutputFullPath = "C:\\Windows";

            Projects = new Collection<ProjectViewModel>
            {
                p1,
                p2,
                p3
            };

            SelectedProject = p1;

            _solutionConfiguration = new SolutionConfiguration();
        }
#endif

        #endregion

        #region TargetDirectory Property

        private string _targetDirectory;

        public string TargetDirectory
        {
            get => _targetDirectory;
            set
            {
                if (_targetDirectory == value) return;

                _targetDirectory = value;
                OnPropertyChanged();
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
                OnPropertyChanged();
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
                OnPropertyChanged();
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
                OnPropertyChanged();
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
                OnPropertyChanged();
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
                OnPropertyChanged();
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
                OnPropertyChanged();
            }
        }

        #endregion

        #region SelectedProjects Property

        public ICollection<ProjectViewModel> SelectedProjects { get; }

        #endregion

        #region Projects Property

        public ICollection<ProjectViewModel> Projects { get; }

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
                OnPropertyChanged();
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
                OnPropertyChanged();
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
                OnPropertyChanged();
            }
        }

        #endregion

        public ObservableCollection<ProjectPreset> ProjectPresets { get; set; }

        public ObservableCollection<string> ProjectEditions { get; set; }

        private readonly SolutionConfiguration _solutionConfiguration;

        public MainViewModel(DTE2 dte, SolutionConfiguration solutionConfiguration)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            _solutionConfiguration = solutionConfiguration;

            var projects = new List<ProjectViewModel>();
            var dteProjects = dte.Solution.GetProjects()
                .OrderBy(p => Path.GetDirectoryName(p.UniqueName))
                .ThenBy(p => p.UniqueName)
                .ToArray();

            for (var i = 0; i < 10; i++)
            {
                foreach (var dteProject in dteProjects)
                {
                    try
                    {
                        var projectConfiguration =
                            solutionConfiguration.Projects.FirstOrDefault(p =>
                                p.ProjectName == dteProject.UniqueName) ??
                            new ProjectConfiguration();

                        var projectViewModel = new ProjectViewModel(dteProject, projectConfiguration.Files)
                        {
                            IsEnabled = projectConfiguration.IsEnabled,
                            IncludeSubDirectories = projectConfiguration.IncludeSubDirectories,
                            TargetDirectory = projectConfiguration.TargetDirectory,
                            InheritFromProject = projectConfiguration.InheritFromProject,
                            ApplicationPreset = projectConfiguration.ApplicationPreset,
                            ReplaceOriginalFile = projectConfiguration.ReplaceOriginalFile,
                        };

                        if (!string.IsNullOrEmpty(projectConfiguration.FileToProtect))
                            projectViewModel.FileToProtect = projectConfiguration.FileToProtect;

                        projects.Add(projectViewModel);
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }

            Projects = projects;
            ProjectPresets = new ObservableCollection<ProjectPreset>
            {
                new ProjectPreset { Id = 1, Name = "Maximum" }, new ProjectPreset { Id = 2, Name = "Balance" },
                new ProjectPreset { Id = 3, Name = "Optimized" }
            };
            ProjectEditions = new ObservableCollection<string>
            {
                "Keep my plan",
                "Essentials",
                "Professional",
                "Enterprise"
            };
            TargetDirectory = solutionConfiguration.TargetDirectory;
            CreateShieldProjectIfNotExists = solutionConfiguration.CreateShieldProjectIfNotExists;
            FindCustomConfigurationFile = solutionConfiguration.FindCustomConfigurationFile;
            ProjectPreset = solutionConfiguration.ProjectPreset;
            ShieldProjectEdition = solutionConfiguration.ShieldProjectEdition;
            ShieldProjectName = solutionConfiguration.ShieldProjectName;
            BuildConfiguration = solutionConfiguration.BuildConfiguration;
            SelectedProjects = new ObservableCollection<ProjectViewModel>();
            IsValidClient = false;

            if (string.IsNullOrEmpty(ShieldProjectName))
                ShieldProjectName = Path.GetFileNameWithoutExtension(dte.Solution.FileName);

            if (dte.Solution.SolutionBuild.StartupProjects is object[] startupProjects)
            {
                var startupProject = startupProjects.OfType<string>().FirstOrDefault();
                if (startupProject != null)
                    SelectedProject = Projects.FirstOrDefault(p => p.Project.UniqueName == startupProject);
            }

            if (SelectedProject == null)
                SelectedProject = Projects.FirstOrDefault();
        }

        public void Enable(bool isEnabled)
        {
            foreach (var projectViewModel in SelectedProjects)
                projectViewModel.IsEnabled = isEnabled;
        }

        public void IncludeSubDirectories(bool include)
        {
            foreach (var projectViewModel in SelectedProjects)
                projectViewModel.IncludeSubDirectories = include;
        }

        public void AddOutput(string fileExtension)
        {
            foreach (var item in SelectedProjects)
            {
                var targetFileName = item.Name + fileExtension;

                if (!File.Exists(Path.Combine(item.OutputFullPath, targetFileName)))
                    continue;

                if (item.Files.Any(p => string.Equals(p.FileName, targetFileName, StringComparison.OrdinalIgnoreCase)))
                    continue;

                item.Files.Add(new ProjectFileViewModel(targetFileName));
            }
        }

        public void AddOutputByPattern(string searchPattern)
        {
            foreach (var item in SelectedProjects)
            {
                if (item.Files.Any(p => string.Equals(p.FileName, searchPattern, StringComparison.OrdinalIgnoreCase)))
                    continue;

                item.Files.Add(new ProjectFileViewModel(searchPattern));
            }
        }

        public void ClearFiles()
        {
            foreach (var projectViewModel in SelectedProjects)
                projectViewModel.Files.Clear();
        }

        public void ClearTargetDirectory()
        {
            foreach (var item in SelectedProjects)
                item.TargetDirectory = null;
        }

        public void Save()
        {
            _solutionConfiguration.TargetDirectory = TargetDirectory;
            _solutionConfiguration.ShieldProjectName = ShieldProjectName;
            _solutionConfiguration.BuildConfiguration = BuildConfiguration;
            _solutionConfiguration.CreateShieldProjectIfNotExists = CreateShieldProjectIfNotExists;
            _solutionConfiguration.FindCustomConfigurationFile = FindCustomConfigurationFile;
            _solutionConfiguration.ProjectPreset = ProjectPreset;
            _solutionConfiguration.ShieldProjectEdition = ShieldProjectEdition;
            _solutionConfiguration.Projects.Clear();

            foreach (var projectViewModel in Projects)
            {
                var projectConfiguration = new ProjectConfiguration
                {
                    IsEnabled = projectViewModel.IsEnabled,
                    ProjectName = projectViewModel.Project.UniqueName,
                    IncludeSubDirectories = projectViewModel.IncludeSubDirectories,
                    TargetDirectory = projectViewModel.TargetDirectory,
                    InheritFromProject = projectViewModel.InheritFromProject,
                    ApplicationPreset = projectViewModel.ApplicationPreset,
                    FileToProtect = projectViewModel.FileToProtect,
                    ReplaceOriginalFile = projectViewModel.ReplaceOriginalFile,
                };

                foreach (var projectFileViewModel in projectViewModel.Files)
                    projectConfiguration.Files.Add(projectFileViewModel.FileName);

                _solutionConfiguration.Projects.Add(projectConfiguration);
            }
        }
    }
}