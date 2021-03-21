using System;
using System.Windows.Input;

namespace FileDB2Browser.ViewModel
{
    public class CommandHandler : ICommand
    {
        private readonly Action<object> action;
        private readonly Func<bool> canExecute;

        public CommandHandler(Action<object> action)
        {
            this.action = action;
            this.canExecute = () => { return true; };
        }

        public CommandHandler(Action<object> action, Func<bool> canExecute)
        {
            this.action = action;
            this.canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return canExecute.Invoke();
        }

        public void Execute(object parameter)
        {
            action(parameter);
        }
    }
}
