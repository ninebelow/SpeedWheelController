using System;
using System.Windows.Input;

namespace SpeedWheelController.ViewModels
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> execute;
        private readonly Predicate<object> canExecute;

        public event EventHandler? CanExecuteChanged;

        public RelayCommand(Action<object> execute) : this(execute, null!)
        {
        }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            return this.canExecute == null || this.canExecute(parameter!);
        }

        public void Execute(object? parameter)
        {
            this.execute(parameter!);
        }
    }
}
