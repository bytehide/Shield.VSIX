using System;
using System.Windows.Input;

namespace ShieldVSExtension.Commands
{
    internal class RelayCommand(Action<object> execute, Predicate<object> canExecute) : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return canExecute == null || canExecute.Invoke(parameter);
        }

        public void Execute(object parameter)
        {
            execute.Invoke(parameter);
        }

        public event EventHandler CanExecuteChanged;
    }
}