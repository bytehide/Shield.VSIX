using System.ComponentModel.Design;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace ShieldVSExtension.Commands
{
    internal sealed class Enable
    {
        public static OleMenuCommand Command { get; private set; }
        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            IMenuCommandService commandService = await package.GetServiceAsync<IMenuCommandService, IMenuCommandService>();
            Assumes.Present(commandService);

            var cmdId = new CommandID(Guids.GuidShieldVsExtensionPackageCmdSet, (int)Ids.ShieldEnabled);
            Command = new OleMenuCommand((s, e) => Execute(package), cmdId);
            
            commandService.AddCommand(Command);
        }

        public static void Execute(AsyncPackage package)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (ShieldVsExtensionPackage.Configuration is null)
                return;

            ShieldVsExtensionPackage.Configuration.IsEnabled = !ShieldVsExtensionPackage.Configuration.IsEnabled;
            ShieldVsExtensionPackage.UpdateExtensionEnabled();
        }
    }
}
