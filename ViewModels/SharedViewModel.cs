using System.ComponentModel;
using EnvDTE;

namespace ShieldVSExtension.ViewModels
{
    public class SharedViewModel
    {
        private Project _payload;

        public Project Payload
        {
            get => _payload;
            set
            {
                if (_payload == value) return;

                _payload = value;
                OnPropertyChanged(nameof(Payload));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}