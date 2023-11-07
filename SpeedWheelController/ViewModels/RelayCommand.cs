using System;
using System.Windows.Input;

namespace SpeedWheelController.ViewModels
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> execute;
        private readonly Predicate<object>? canExecute;

        public RelayCommand(Action<object> execute) : this(execute, null!)
        {
        }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            ArgumentNullException.ThrowIfNull(execute);
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public RelayCommand(Action execute)
        {
            ArgumentNullException.ThrowIfNull(execute);
            this.execute = (_) => execute();
            this.canExecute = null!;
        }

        public RelayCommand(Action execute, Func<bool> canExecute)
        {
            ArgumentNullException.ThrowIfNull(execute);
            ArgumentNullException.ThrowIfNull(canExecute);

            this.execute = (_) => execute();
            this.canExecute = (_) => canExecute();
        }

        public bool CanExecute(object? parameter)
        {
            return this.canExecute == null || this.canExecute(parameter!);
        }

        public void Execute(object? parameter)
        {
            this.execute(parameter!);
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}