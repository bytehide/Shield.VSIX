using System;
using System.ComponentModel.Design;
using System.Windows.Interop;
using EnvDTE;
using EnvDTE80;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using ShieldVSExtension.ToolWindows;
using ShieldVSExtension.UI_Extensions;
using Task = System.Threading.Tasks.Task;

namespace ShieldVSExtension.Commands
{
    internal sealed class ConfigurationWindowCommand
    {
        public static OleMenuCommand Command { get; private set; }
        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            IMenuCommandService commandService = await package.GetServiceAsync<IMenuCommandService, IMenuCommandService>();
            Assumes.Present(commandService);

            var cmdId = new CommandID(Guids.GuidShieldVsExtensionPackageCmdSet, (int)Ids.ShieldConfiguration);
            Command = new OleMenuCommand((s, e) => Execute(package), cmdId);

            commandService.AddCommand(Command);
        }

        public static void Execute(AsyncPackage package)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (ShieldVsExtensionPackage.Configuration is null)
                return;

            //ToolWindowPane window = package.FindToolWindow(typeof(ConfigurationWindow), 0, true);
            //if (window?.Frame == null)
            //{
            //    throw new NotSupportedException("Cannot create tool window");
            //}
            //IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            //Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());

            //var optionsViewModel = new OptionsViewModel(dte, configuration);

            if (!(ServiceProvider.GlobalProvider.GetService(typeof(DTE)) is DTE2 dte)) throw new ArgumentNullException(nameof(dte));

            var optionsViewModel = new ConfigurationViewModel(dte, ShieldVsExtensionPackage.Configuration);
            var optionsView = new ConfigurationWindowControl(optionsViewModel);

            var interop = new WindowInteropHelper(optionsView);
            interop.EnsureHandle();
            interop.Owner = (IntPtr)dte.MainWindow.HWnd;

            optionsView.ShowDialog();

            ShieldVsExtensionPackage.UpdateExtensionEnabled();
        }
    }
}
