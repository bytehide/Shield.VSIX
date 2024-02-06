using System.Windows;
using ShieldVSExtension.Commands;

namespace ShieldVSExtension.ViewModels
{
    public class SharedViewModel : ViewModelBase
    {
        public SharedViewModel()
        {
            CheckProjectCommand = new RelayCommand(OnCheckProject);
        }

        protected override void OnCheckProject(object _)
        {
            if (Payload == null) return;
        
            MessageBox.Show($"settings for {Payload.Name} project");
        }
    }
}