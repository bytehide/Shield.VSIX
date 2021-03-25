using Microsoft.VisualStudio.Shell;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using dnlib.DotNet;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using ShieldVSExtension.Commands;
using ShieldVSExtension.Helpers;
using ShieldVSExtension.ToolWindows;
using Task = System.Threading.Tasks.Task;
using ShieldSolutionConfiguration = ShieldVSExtension.Configuration.SolutionConfiguration;
using Microsoft;
using Shield.Client;
using Shield.Client.Extensions;
using Shield.Client.Models;
using Shield.Client.Models.API.Application;
using ShieldVSExtension.Configuration;

namespace ShieldVSExtension
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionOpening_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideToolWindow(typeof(ConfigurationWindow))]
    public sealed class ShieldVsExtensionPackage : AsyncPackage, IVsPersistSolutionOpts
    {
        /// <summary>
        /// ShieldVSExtensionPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "311f3401-ccf0-489a-b402-97528dc6b439";

        private const string ShieldConfiguration = "ShieldConfigurationPkg";

        internal static ShieldSolutionConfiguration Configuration { get; set; }

        private OutputWindowPane Pane { get; set; }

        private vsBuildAction CurrentBuildAction { get; set; }

        private DTE2 Dte { get; set; }

        private BuildEvents buildEvents;

        private SolutionEvents solutionEvents;

        private ErrorListProvider ErrorListProvider { get; set; }

        private ShieldClient ShieldApiClient { get; set; }

        private const string ExtensionConfigurationFile = "ExtensionConfiguration";

        private SecureLocalStorage.SecureLocalStorage LocalStorage { get; set; }

        private ShieldExtensionConfiguration ExtensionConfiguration { get; set; }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await Enable.InitializeAsync(this);
            await ConfigurationWindowCommand.InitializeAsync(this);

            Dte = (DTE2)await GetServiceAsync(typeof(DTE));
            if (Dte == null) throw new ArgumentNullException(nameof(Dte));

            Pane = Dte.ToolWindows.OutputWindow.OutputWindowPanes.Add("Dotnetsafer Shield");

            ErrorListProvider = new ErrorListProvider(this);

            solutionEvents = Dte.Events.SolutionEvents;

            buildEvents = Dte.Events.BuildEvents;
            
            buildEvents.OnBuildBegin += BuildEvents_OnBuildBegin;
            
            buildEvents.OnBuildProjConfigDone += BuildEvents_OnBuildProjConfigDone;

            var isSolutionLoaded = await IsSolutionLoadedAsync();

            solutionEvents.AfterClosing += SolutionEvents_AfterClosing;

            AddOptionKey(ShieldConfiguration);

            var solutionPersistenceService = (IVsSolutionPersistence) await GetServiceAsync(typeof(IVsSolutionPersistence));
            if (solutionPersistenceService == null) throw new ArgumentNullException(nameof(solutionPersistenceService));

            solutionPersistenceService.LoadPackageUserOpts(this, ShieldConfiguration);

            if (isSolutionLoaded)
                SolutionEventsOnOpened();
            else
                solutionEvents.Opened += SolutionEventsOnOpened;

            TryReloadStorage();
        }

        private bool TryReloadStorage()
        {
            LocalStorage = new SecureLocalStorage.SecureLocalStorage(
                new SecureLocalStorage.CustomLocalStorageConfig(null, "DotnetsaferShieldForVisualStudio").WithDefaultKeyBuilder()
            );

            ExtensionConfiguration = LocalStorage.Exists(ExtensionConfigurationFile)
                ? LocalStorage.Get<ShieldExtensionConfiguration>(ExtensionConfigurationFile)
                : null;

            return ExtensionConfiguration != null;
        }

        private bool TryConnectShield()
        {
            if (ShieldApiClient != null)
            {
                //Test connection
                return true;
            }

            if (!TryReloadStorage())
                return false;
            try
            {
                ShieldApiClient = ShieldClient.CreateInstance(ExtensionConfiguration.ApiToken);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> IsSolutionLoadedAsync()
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync();

            if (!(await GetServiceAsync(typeof(SVsSolution)) is IVsSolution solService)) throw new ArgumentNullException(nameof(solService));

            ErrorHandler.ThrowOnFailure(solService.GetProperty((int) __VSPROPID.VSPROPID_IsSolutionOpen,
                    out var value));

            return value is bool isSolOpen && isSolOpen;
        }

        protected override void OnSaveOptions(string key, Stream stream)
        {
            if (key != ShieldConfiguration || Configuration == null)
                return;

            try
            {
                ShieldSolutionConfiguration.Save(Configuration, stream);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cannot save configuration.\r\n{ex.GetType().Name}: {ex.Message}");
            }
        }

        protected override void OnLoadOptions(string key, Stream stream)
        {
            if (key != ShieldConfiguration)
                return;

            try
            {
                Configuration = ShieldSolutionConfiguration.Load(stream);
            }
            catch (Exception e)
            {

                ThreadHelper.ThrowIfNotOnUIThread();

                Configuration = new ShieldSolutionConfiguration();
                WriteLine("An error occurred while loading the Shield configuration.");
                WriteLine($"{e.GetType().Name}: {e.Message}");
                ActivePane();
            }
        }

        private void WriteLine(string message)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            Pane.OutputString(message+Environment.NewLine);
        }

        private void Write(string message)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            Pane.OutputString(message);
        }

        private void ActivePane()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Pane.Activate();
        }

        private void SolutionEvents_AfterClosing()
        {
            Enable.Command.Visible = false;
        }

        internal static void UpdateExtensionEnabled(bool? isEnabled = null)
        {
            if (isEnabled.HasValue)
                Enable.Command.Checked = isEnabled.Value;
            else
                Enable.Command.Checked = Configuration != null && Configuration.IsEnabled;

        }

        private void SolutionEventsOnOpened()
        {
            Enable.Command.Visible = true;
            ConfigurationWindowCommand.Command.Visible = true;

            if (Configuration == null)
                Configuration = new ShieldSolutionConfiguration();

            UpdateExtensionEnabled();
        }

        private void BuildEvents_OnBuildProjConfigDone(string projectName, string projectConfig, string platform, string solutionConfig, bool success)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (!TryConnectShield())
                return;

            if (!success || Configuration == null || !Configuration.IsEnabled)
                return;

            if (CurrentBuildAction != vsBuildAction.vsBuildActionBuild && CurrentBuildAction != vsBuildAction.vsBuildActionRebuildAll)
                return;

            var projectConfiguration = Configuration.Projects.FirstOrDefault(p => p.ProjectName == projectName && p.IsEnabled);
            if (projectConfiguration == null || string.IsNullOrEmpty(projectConfiguration.FileToProtect))
                return;

            //var targetDirectory = !string.IsNullOrEmpty(projectConfiguration.TargetDirectory)
            //    ? projectConfiguration.TargetDirectory
            //    : Configuration.TargetDirectory;

            //if (String.IsNullOrEmpty(targetDirectory))
            //    return;

            //targetDirectory = Environment.ExpandEnvironmentVariables(targetDirectory);

            var project = Dte.Solution.GetProjects().FirstOrDefault(p => p.UniqueName == projectConfiguration.ProjectName);

            var sourceDirectory = project.GetFullOutputPath();

            var file = Path.Combine(sourceDirectory, projectConfiguration.FileToProtect);

            if (!File.Exists(file))
                return;

            var statusBar = (IVsStatusbar)GetService(typeof(SVsStatusbar));
            Assumes.Present(statusBar);
            uint cookie = 0;

            object icon = (short)Microsoft.VisualStudio.Shell.Interop.Constants.SBAI_Deploy;

            try
            {
                //var files = projectConfiguration.Files
                //    .Where(p => !String.IsNullOrWhiteSpace(p))
                //    .Select(Environment.ExpandEnvironmentVariables)
                //    .SelectMany(p => Directory.GetFiles(sourceDirectory, p, projectConfiguration.IncludeSubDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                //    .Distinct()
                //    .OrderBy(p => p)
                //    .Where(File.Exists)
                //    .ToArray();

                //if (files.Length == 0)
                //    return;

                WriteLine($"====={projectName}=====");

                statusBar.Animation(1, ref icon);

                var shieldProject = ShieldApiClient.Project.FindOrCreateExternalProject(Configuration.ShieldProjectName);

                var dependencies = project.GetReferences();

                var moduleCtx = ModuleDef.CreateModuleContext();

                var module = ModuleDefMD.Load(file, moduleCtx);

                var referencies = module.GetAssemblyRefs();

                var requiredReferencies = dependencies.Where(dp => referencies.Any(rf => string.Equals(rf.FullName, dp.reference, StringComparison.InvariantCultureIgnoreCase)));

                var uploadApplicationDirectly = ShieldApiClient.Application.UploadApplicationDirectly(shieldProject.Key, file
                    ,requiredReferencies.Select(dep => dep.path).ToList());

                if (uploadApplicationDirectly.RequiresDependencies ||
                    string.IsNullOrEmpty(uploadApplicationDirectly.ApplicationBlob))
                    throw new Exception("Dependencies required."); //TODO: Inform UI

                var appBlob = uploadApplicationDirectly.ApplicationBlob;

                var connection = ShieldApiClient.Connector.CreateQueueConnection();

                var taskConnection = ShieldApiClient.Connector.InstanceQueueConnector(connection);

                taskConnection.OnLog(connection.OnLogger, (s, s1, arg3) => WriteLine(s));

                taskConnection.StartAsync().Wait();

                ApplicationConfigurationDto config;


                //TODO: Looking for custom configuration

                if (projectConfiguration.InheritFromProject)
                    config =
                        ShieldApiClient.Configuration.MakeApplicationConfiguration(Configuration.ProjectPreset.Name
                            .ToPreset());
                else
                    config = ShieldApiClient.Configuration.MakeApplicationConfiguration(projectConfiguration.ApplicationPreset.Name
                        .ToPreset());

                var protect = ShieldApiClient.Tasks.ProtectSingleFile(
                    shieldProject.Key,
                    appBlob,
                    connection,
                    config,
                    connection.OnLogger);

                protect.OnError(taskConnection, error => throw new Exception("error"));

                protect.OnSuccess(taskConnection, dto =>
                    ShieldApiClient.Application.DownloadApplicationAsArray(dto)
                        .SaveOn(
                            projectConfiguration.ReplaceOriginalFile ? 
                                file : 
                                file.Replace(Path.GetExtension(file),$"_shield{Path.GetExtension(file)}"), true)
                );

                var waitHandle = new AutoResetEvent(false);

                protect.OnClose(taskConnection, delegate { waitHandle.Set(); });

                waitHandle.WaitOne();

                WriteLine($"====={projectName}=====");
                WriteLine("");
            }
            catch (Exception ex)
            {
                WriteLine($"====={projectName}=====");
                WriteLine(ex.Message);
                WriteLine($"====={projectName}=====");
                WriteLine("");

                Pane.Activate();
            }
            finally
            {
                statusBar.Animation(0, ref icon);
                statusBar.Progress(ref cookie, 0, "", 0, 0);
            }
        }

        private void BuildEvents_OnBuildBegin(vsBuildScope scope, vsBuildAction action)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            CurrentBuildAction = action;
            Pane.Clear();
            ErrorListProvider.Tasks.Clear();
        }

        #endregion
    }
}
