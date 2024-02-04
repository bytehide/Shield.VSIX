using Microsoft.VisualStudio.Shell;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using EnvDTE;
using ShieldVSExtension.Common.Configuration;
using ShieldVSExtension.Common.Contracts;
using ShieldVSExtension.Common.Extensions;
using ShieldVSExtension.Common.Helpers;

namespace ShieldVSExtension.ViewModels
{
    public class ProjectViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        // [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // public event PropertyChangedEventHandler PropertyChanged = delegate { };
        // 
        // private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        // {
        //     PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        // }

        #endregion

        #region IsEnabled Property

        private bool _isEnabled;

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                if (_isEnabled == value)
                    return;

                _isEnabled = value;
                OnPropertyChanged(nameof(IsEnabled));
            }
        }

        #endregion

        #region IncludeSubDirectories Property

        private bool _includeSubDirectories;

        public bool IncludeSubDirectories
        {
            get { return _includeSubDirectories; }
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
            get { return _inheritFromProject; }
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
            get { return _replaceOriginalFile; }
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
            get { return _applicationPreset; }
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
            get { return _targetDirectory; }
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
            get { return _fileToProtect; }
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
            get { return _buildConfiguration; }
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
            Files = new ObservableCollection<ProjectFileViewModel>();
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
    }
}