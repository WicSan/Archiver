using System;
using System.Diagnostics;
using System.Windows.Input;

namespace ArchivePlanner.Util
{
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Predicate<object?>? _canExecute;

        public RelayCommand(Action execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        [DebuggerStepThrough]
        public bool CanExecute(object? parameter)
        {
            return _canExecute?.Invoke(parameter) ?? true;
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public void Execute(object? parameter)
        {
            _execute();
        }
    }
}
