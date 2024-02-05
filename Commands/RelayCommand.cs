﻿using System;
using System.Windows.Input;

namespace ShieldVSExtension.Commands
{
    public class RelayCommand(Action<object> execute, Predicate<object> canExecute = null) : ICommand
    {
        private readonly Action<object> _execute = execute ?? throw new ArgumentNullException(nameof(execute));

        public bool CanExecute(object parameter)
        {
            return canExecute == null || canExecute.Invoke(parameter);
        }

        public void Execute(object parameter)
        {
            _execute?.Invoke(parameter);
        }

        public event EventHandler CanExecuteChanged;
    }
}