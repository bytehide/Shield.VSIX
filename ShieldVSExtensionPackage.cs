﻿using Microsoft.VisualStudio.Shell;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using ShieldVSExtension.Commands;
using ShieldVSExtension.ToolWindows;
using Task = System.Threading.Tasks.Task;
using ShieldSolutionConfiguration = ShieldVSExtension.Configuration.SolutionConfiguration;
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

            var dte = (DTE2)await GetServiceAsync(typeof(DTE));
            if (dte == null) throw new ArgumentNullException(nameof(dte));

            Pane = dte.ToolWindows.OutputWindow.OutputWindowPanes.Add("Dotnetsafer Shield");

            var solutionEvents = dte.Events.SolutionEvents;

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

        #endregion
    }
}