using Community.VisualStudio.Toolkit;
using dnlib.DotNet;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using ShieldVSExtension.Commands;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft;
using ShieldSolutionConfiguration = ShieldVSExtension.Common.Configuration.SolutionConfiguration;
using Task = System.Threading.Tasks.Task;
using ShieldVSExtension.Common.Configuration;
using ShieldVSExtension.Common.Extensions;
using ShieldVSExtension.Common.Helpers;
using ShieldVSExtension.Storage;
using ShieldVSExtension.UI;

namespace ShieldVSExtension;

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
[ProvideToolWindow(typeof(AppWindow))]
public sealed class ShieldVsExtensionPackage : AsyncPackage
{
    /// <summary>
    /// ShieldVSExtensionPackage GUID string.
    /// </summary>
    public const string PackageGuidString = "311f3401-ccf0-489a-b402-97528dc6b439";

    private const string ShieldConfiguration = "ShieldConfigurationPkg";

    internal static ShieldSolutionConfiguration Configuration { get; set; }

    private OutputWindowPane Pane { get; set; }

    private OutputWindowPane OutputPane { get; set; }

    private vsBuildAction CurrentBuildAction { get; set; }

    private DTE2 Dte { get; set; }

    private BuildEvents buildEvents;

    private SolutionEvents solutionEvents;

    private ErrorListProvider ErrorListProvider { get; set; }


    private const string ExtensionConfigurationFile = "ExtensionConfiguration";

    private SecureLocalStorage LocalStorage { get; set; }

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
        // var tt = AppDomain.CurrentDomain.GetAssemblies().ToList();

        // When initialized asynchronously, the current thread may be a background thread at this point.
        // Do any initialization that requires the UI thread after switching to the UI thread.
        await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
        await Enable.InitializeAsync(this);
        // await ConfigurationWindowCommand.InitializeAsync(this);
        await MainWindowCommand.InitializeAsync(this);

        Dte = (DTE2)await GetServiceAsync(typeof(DTE));
        Assumes.Present(Dte);
        if (Dte == null) throw new ArgumentNullException(nameof(Dte));

        Pane = Dte.ToolWindows.OutputWindow.OutputWindowPanes.Add("Dotnetsafer Shield");

        OutputPane = Dte.ToolWindows.OutputWindow.OutputWindowPanes.Add("Dotnetsafer Shield Output");

        ErrorListProvider = new ErrorListProvider(this);

        solutionEvents = Dte.Events.SolutionEvents;

        buildEvents = Dte.Events.BuildEvents;

        buildEvents.OnBuildBegin += BuildEvents_OnBuildBegin;

        buildEvents.OnBuildProjConfigDone += BuildEvents_OnBuildProjConfigDone;

        //buildEvents.OnBuildDone += (scope, action) => ActivePane();

        var isSolutionLoaded = await IsSolutionLoadedAsync();

        solutionEvents.AfterClosing += SolutionEvents_AfterClosing;

        AddOptionKey(ShieldConfiguration);

        var solutionPersistenceService = (IVsSolutionPersistence)await GetServiceAsync(typeof(IVsSolutionPersistence));
        if (solutionPersistenceService == null) throw new ArgumentNullException(nameof(solutionPersistenceService));

        solutionPersistenceService.LoadPackageUserOpts(this, ShieldConfiguration);

        if (isSolutionLoaded)
        {
            SolutionEventsOnOpened();
        }

        solutionEvents.Opened += SolutionEventsOnOpened;

        TryReloadStorage();
    }

    private bool TryReloadStorage()
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        try
        {
            LocalStorage = new SecureLocalStorage(
                new CustomLocalStorageConfig(null, "DotnetsaferShieldForVisualStudio").WithDefaultKeyBuilder()
            );

            ExtensionConfiguration = LocalStorage.Exists(ExtensionConfigurationFile)
                ? LocalStorage.Get<ShieldExtensionConfiguration>(ExtensionConfigurationFile)
                : null;
        }
        catch (Exception e)
        {
            WriteLine(e.Message);
        }

        return ExtensionConfiguration != null;
    }

    private static bool TryConnectShield()
    {
        return false;
    }

    private async Task<bool> IsSolutionLoadedAsync()
    {
        await JoinableTaskFactory.SwitchToMainThreadAsync();

        if (!(await GetServiceAsync(typeof(SVsSolution)) is IVsSolution solService))
            throw new ArgumentNullException(nameof(solService));

        ErrorHandler.ThrowOnFailure(solService.GetProperty((int)__VSPROPID.VSPROPID_IsSolutionOpen,
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
        Pane.OutputString(message + Environment.NewLine);
    }

    private async Task WriteLineAsync(string message)
    {
        await JoinableTaskFactory.SwitchToMainThreadAsync(DisposalToken);

        Pane.OutputString(message + Environment.NewLine);
    }

    private async Task WriteLineToOutputAsync(string message)
    {
        await JoinableTaskFactory.SwitchToMainThreadAsync(DisposalToken);

        OutputPane.OutputString(message + Environment.NewLine);
    }

    private void WriteLineToOutput(string message)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        OutputPane.OutputString(message + Environment.NewLine);
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

    private void ActiveOutputPane()
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        OutputPane.Activate();
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
        // ConfigurationWindowCommand.Command.Visible = true;
        MainWindowCommand.Command.Visible = true;

        if (Configuration == null)
            Configuration = new ShieldSolutionConfiguration();

        UpdateExtensionEnabled();
    }

    public void BuildEvents_OnBuildProjConfigDone(string projectName, string projectConfig, string platform,
        string solutionConfig, bool success)
    {
        _ = JoinableTaskFactory.RunAsync(async delegate
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(DisposalToken);

            if (Configuration.BuildConfiguration != "*" && !string.Equals(projectConfig,
                    Configuration.BuildConfiguration, StringComparison.CurrentCultureIgnoreCase))
                return;

            if (!success || Configuration == null || !Configuration.IsEnabled)
                return;

            if (CurrentBuildAction != vsBuildAction.vsBuildActionBuild &&
                CurrentBuildAction != vsBuildAction.vsBuildActionRebuildAll)
                return;

            if (!TryConnectShield())
            {
                await WriteLineAsync("[ERROR] The Dotnetsafer Shield Api token is invalid.");
                await WriteLineAsync(
                    "> Go to the extension UI to update it, or read our documentation at https://dotnetsafer.com/docs/product/shield-vs/1.0.");
                await WriteLineAsync("[ERROR] Cannot continue without valid api token.");
                return;
            }

            var overrideEdition = Configuration.ShieldProjectEdition == "Keep my plan"
                ? null
                : Configuration.ShieldProjectEdition ?? null;

            await WriteLineAsync("#");
            await
                WriteLineAsync(
                    "[Reminder] This extension is not intended for deployment, if you need to protect Xamarin, WebApp or Apis applications that have automatic deployment on azure, aws or other servers, use: https://dotnetsafer.com/docs/product/shield-msbuild/1.0.");
            await WriteLineAsync("#");

            var projectConfiguration =
                Configuration.Projects.FirstOrDefault(p => p.ProjectName == projectName && p.IsEnabled);
            if (projectConfiguration == null || string.IsNullOrEmpty(projectConfiguration.FileToProtect))
                return;

            await WriteLineAsync("Protection operation started...");

            var project = Dte.Solution.GetProjects()
                .FirstOrDefault(p => p.UniqueName == projectConfiguration.ProjectName);

            var sourceDirectory = project.GetFullOutputPath();

            var rootDirectory = project?.GetDirectory();

            var file = Path.Combine(sourceDirectory, projectConfiguration.FileToProtect);

            if (!File.Exists(file))
            {
                await WriteLineAsync($"[WARNING] Cannot find compiled file in: {file}");

                var projectInformation = await project.GetEvaluatedPropertiesAsync();

                if (!projectInformation.TryGetValue("TargetPath", out var originalFile))
                {
                    await WriteLineAsync("[ERROR] Could not get compiled file for this project.");
                    return;
                }

                if (!File.Exists(originalFile))
                {
                    await WriteLineAsync($"[ERROR] The original build file: '{originalFile}' cannot be obtained.");
                    return;
                }

                file = originalFile;

                await WriteLineAsync(
                    "[WARNING] The original project file has been obtained, which did not correspond to the configuration file.");
            }

            try
            {
                await WriteLineAsync($"========== {projectName} ==========");

                await WriteLineAsync($"> The {Path.GetFileName(file)} file will be protected...");

                await VS.Notifications.StartStatusbarAnimationAsync(StatusAnimation.Build);

                await VS.Notifications.SetStatusbarTextAsync($"Protecting {Path.GetFileName(file)}...");

                //var regexName = RefactorProjectName(projectName);

                // var shieldProject = await ShieldApiClient.Project.FindOrCreateExternalProjectAsync(Configuration.ShieldProjectName);

                await WriteLineAsync(
                    $"> The project has been linked to your Dotnetsafer account with the name '{Configuration.ShieldProjectName}'.");

                var dependencies = project.GetReferences();

                var moduleCtx = ModuleDef.CreateModuleContext();

                var bytes = File.ReadAllBytes(file);

                var module = ModuleDefMD.Load(bytes, moduleCtx);

                var referencies = module.GetAssemblyRefs().ToList();

                var requiredReferencies = dependencies.Where(
                    dp => referencies.Any(
                              rf => string.Equals(rf.FullName, dp.strongInfo,
                                  StringComparison.InvariantCultureIgnoreCase))
                          || referencies.Any(rf =>
                              !string.IsNullOrEmpty(dp.strongInfo) &&
                              dp.strongInfo.ToLowerInvariant().Contains(".csproj")
                              && dp.strongInfo.ToLowerInvariant().Contains(rf.Version.ToString())
                              && dp.strongInfo.ToLowerInvariant().Contains(rf.Name.ToLowerInvariant())));

                var discoverDependencies = requiredReferencies.ToList();

                await WriteLineAsync(
                    $"> {discoverDependencies.Count} dependencies have been found, they will be used to process the application.");

                await WriteLineAsync("> Analyzing the application...");

                // var connection = ShieldApiClient.Connector.CreateQueueConnection();

                var status = 1;

                if (!string.IsNullOrEmpty(overrideEdition))
                {
                    await WriteLineAsync(
                        $"> This process will be executed under the Shield {overrideEdition} edition, because you configured it in the extension.");
                    await WriteLineAsync($"> *Remember if you do not have enough credits it will give an error.");
                }

#pragma warning disable VSTHRD101 // Evite delegados asincrónicos no compatibles
                    
#pragma warning restore VSTHRD101 // Evite delegados asincrónicos no compatibles

                    

#pragma warning disable VSTHRD101 // Evite delegados asincrónicos no compatibles
                    
#pragma warning restore VSTHRD101 // Evite delegados asincrónicos no compatibles

#pragma warning disable VSTHRD101 // Evite delegados asincrónicos no compatibles
                    
#pragma warning restore VSTHRD101 // Evite delegados asincrónicos no compatibles

#pragma warning disable VSTHRD101 // Evite delegados asincrónicos no compatibles
                    
#pragma warning restore VSTHRD101 // Evite delegados asincrónicos no compatibles

                ActivePane();

                // await taskConnection.ProtectSingleFileAsync(shieldProject.Key, appBlob, config);
                // await taskConnection.ProtectSingleFileAsync(shieldProject.Key, appBlob, null);
            }
            catch (Exception ex)
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                await ex.LogAsync();
                ActivePane();
                await WriteLineAsync($"#");
                await WriteLineAsync($"========= [EXCEPTION] =========");
                await WriteLineAsync($"#");
                await WriteLineAsync($"#  An error occurred while protecting the {projectName} project.");
                await WriteLineAsync($"#");
                await WriteLineAsync($"#  -> {ex.Message}");
                await WriteLineAsync($"#");
                await WriteLineAsync(
                    "#  The process has been terminated due to an exception, check the 'extensions' output window for exception information.");
                await WriteLineAsync($"#");
                await WriteLineAsync($"========== {projectName} ==========");
            }
        });
    }

    private string RefactorProjectName(string projectName)
    {
        if (projectName.Contains("\\"))
        {
            var split = projectName.Split('\\');
            projectName = split[split.Length - 1];
        }

        var rgx = new Regex("[^a-zA-Z0-9 -]");
        var regexName = rgx.Replace(projectName, "");


        return regexName.Replace("csproj", null);
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