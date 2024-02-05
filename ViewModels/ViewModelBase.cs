using System.ComponentModel;
using ShieldVSExtension.Commands;
using System;

namespace ShieldVSExtension.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        private ProjectViewModel _payload;

        public ProjectViewModel Payload
        {
            get => _payload;
            set
            {
                if (_payload == value) return;

                _payload = value;
                OnPropertyChanged(nameof(Payload));
            }
        }

        #region Commands

        public RelayCommand CheckProjectCommand { get; set; }

        protected virtual void OnCheckProject(object obj) => throw new NotImplementedException();

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // public event PropertyChangedEventHandler PropertyChanged;
        // 
        // protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        // {
        //     PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        // }
        // 
        // protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        // {
        //     if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        //     field = value;
        //     OnPropertyChanged(propertyName);
        //     return true;
        // }

        #endregion
    }
}