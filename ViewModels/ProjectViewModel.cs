using Microsoft.VisualStudio.Shell;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using EnvDTE;
using ShieldVSExtension.Common.Configuration;
using ShieldVSExtension.Common.Contracts;
using ShieldVSExtension.Common.Extensions;
using ShieldVSExtension.Common.Helpers;
using ShieldVSExtension.Commands;
using System;

namespace ShieldVSExtension.ViewModels
{
    public class ProjectViewModel : ViewModelBase
    {
        // #region INotifyPropertyChanged
        // 
        // public event PropertyChangedEventHandler PropertyChanged;
        // 
        // // [NotifyPropertyChangedInvocator]
        // protected virtual void OnPropertyChanged(string propertyName)
        // {
        //     PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        // }
        // 
        // // public event PropertyChangedEventHandler PropertyChanged = delegate { };
        // // 
        // // private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        // // {
        // //     PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        // // }

        // #endregion

        private string _projectToken = string.Empty;

        public string ProjectToken
        {
            get => _projectToken;
            set
            {
                if (null == _projectToken || _projectToken.Equals(value, StringComparison.OrdinalIgnoreCase)) return;

                _projectToken = value;
                OnPropertyChanged(nameof(ProjectToken));

                // Payload = new PayloadType {Id = Id, Content = Utils.StringToSecureString(Content)};
                // OnPropertyChanged(nameof(Payload));
            }
        }

        #region IsEnabled Property

        private bool _isEnabled;

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled == value) return;

                _isEnabled = value;
                OnPropertyChanged(nameof(IsEnabled));
            }
        }

        #endregion

        #region IncludeSubDirectories Property

        private bool _includeSubDirectories;

        public bool IncludeSubDirectories
        {
            get => _includeSubDirectories;
            set
            {
                if (_includeSubDirectories == value)
                    return;

                _includeSubDirectories = value;
                OnPropertyChanged(nameof(IncludeSubDirectories));
            }
        }

        #endregion

        #region InheritFromProject Property

        private bool _inheritFromProject;

        public bool InheritFromProject
        {
            get => _inheritFromProject;
            set
            {
                if (_inheritFromProject == value)
                    return;

                _inheritFromProject = value;
                OnPropertyChanged(nameof(InheritFromProject));
            }
        }

        #endregion

        #region ReplaceOriginalFile Property

        private bool _replaceOriginalFile;

        public bool ReplaceOriginalFile
        {
            get => _replaceOriginalFile;
            set
            {
                if (_replaceOriginalFile == value)
                    return;

                _replaceOriginalFile = value;
                OnPropertyChanged(nameof(ReplaceOriginalFile));
            }
        }

        #endregion

        #region ApplicationPreset Property

        private ProjectPreset _applicationPreset;

        public ProjectPreset ApplicationPreset
        {
            get => _applicationPreset;
            set
            {
                if (_applicationPreset == value)
                    return;

                _applicationPreset = value;
                OnPropertyChanged(nameof(ApplicationPreset));
            }
        }

        #endregion

        #region TargetDirectory Property

        private string _targetDirectory;

        public string TargetDirectory
        {
            get => _targetDirectory;
            set
            {
                if (_targetDirectory == value)
                    return;

                _targetDirectory = value;
                OnPropertyChanged(nameof(TargetDirectory));
            }
        }

        #endregion

        #region FileToProtect Property

        private string _fileToProtect;

        public string FileToProtect
        {
            get => _fileToProtect;
            set
            {
                if (_fileToProtect == value)
                    return;

                _fileToProtect = value;
                OnPropertyChanged(nameof(FileToProtect));
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
                if (_buildConfiguration == value)
                    return;

                _buildConfiguration = value;
                OnPropertyChanged(nameof(BuildConfiguration));
            }
        }

        #endregion

        public string Name { get; internal set; }

        public string FolderName { get; internal set; }

        public string OutputFullPath { get; internal set; }

        public string ProjectFramework { get; internal set; }
        public string ProjectType { get; internal set; }
        public string ProjectLang { get; internal set; }

        public ObservableCollection<ProjectFileViewModel> Files { get; }

        public Project Project { get; }

        public ProjectViewModel()
        {
            Files = [];
            CheckProjectCommand = new RelayCommand(OnCheckProject);
        }

        public ProjectViewModel(Project project, IEnumerable<string> files)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            Project = project;

            Name = Path.GetFileNameWithoutExtension(project.UniqueName);

            FolderName = Path.GetDirectoryName(project.UniqueName);

            var properties = ThreadHelper.JoinableTaskFactory.Run(async () =>
                await Project.GetEvaluatedPropertiesAsync());

            properties.TryGetValue("TargetPath", out var targetPath);

            OutputFullPath = targetPath ?? project.GetFullOutputPath();

            ProjectFramework = project.GetFrameworkString();
            ProjectType = project.GetOutputType();
            ProjectLang = project.GetLanguageName();

            Files = new ObservableCollection<ProjectFileViewModel>(files.Select(p => new ProjectFileViewModel(p))
                .ToList());

            properties.TryGetValue("TargetFileName", out var targetFileName);

            if (!string.IsNullOrEmpty(targetFileName))
            {
                FileToProtect = targetFileName;
                return;
            }

            if (!string.IsNullOrEmpty(targetPath))
            {
                var fileName = Path.GetFileName(targetPath);
                if (string.IsNullOrEmpty(fileName))
                {
                    FileToProtect = targetFileName;
                    return;
                }
            }

            //throw new Exception("Can't find output file name.");

            //TODO: Remove:
            var outPutPaths =
                project.GetBuildOutputFilePaths(new BuildOutputFileTypes
                {
                    Built = true,
                    ContentFiles = false,
                    Documentation = false,
                    LocalizedResourceDlls = false,
                    SourceFiles = false,
                    Symbols = false,
                    XmlSerializer = false
                });
            var outPutFiles = outPutPaths.Select(Path.GetFileName);
            if (string.IsNullOrEmpty(ProjectType))
            {
                FileToProtect = outPutFiles.FirstOrDefault(x => x.EndsWith(".dll") || x.EndsWith(".exe"));
            }
            else if (ProjectType.ToLower().Contains("winexe"))
            {
                FileToProtect =
                    ProjectFramework.ToLower().Contains("framework")
                        ? outPutFiles.FirstOrDefault(x => x.EndsWith(".exe"))
                        : outPutFiles.FirstOrDefault(x => x.EndsWith(".dll"));
            }
            else if (ProjectType.ToLower().Contains("library"))
            {
                FileToProtect = outPutFiles.FirstOrDefault(x => x.EndsWith(".dll"));
            }
            else
            {
                FileToProtect = outPutFiles.FirstOrDefault(x => x.EndsWith(".dll") || x.EndsWith(".exe"));
            }
        }

        public void CheckOrCreateShieldConfiguration()
        {
            // var shieldConfig = new ShieldConfiguration
            // {
            //      Name = Name,
            //     Enabled = IsEnabled,
            //     Preset = EPresetType.Custom.GetName(),
            //     ProjectToken = string.Empty,
            //     ProtectionSecret = string.Empty,
            //     RunConfiguration = BuildConfiguration,
            //     /*Protections =
            //     [
            //         new Protection {Name = "IncludeSubDirectories", Value = IncludeSubDirectories},
            //         new Protection {Name = "InheritFromProject", Value = InheritFromProject},
            //         new Protection {Name = "ReplaceOriginalFile", Value = ReplaceOriginalFile},
            //         new Protection {Name = "TargetDirectory", Value = TargetDirectory},
            //         new Protection {Name = "FileToProtect", Value = FileToProtect}
            //     ]*/
            // };

            var shieldConfigPath = Path.Combine(FolderName, "shield.config.json");
            File.WriteAllText(shieldConfigPath, @"{}");
            // shieldConfig.Save(shieldConfigPath);
        }
    }
}