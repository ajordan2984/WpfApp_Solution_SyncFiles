using System;
using System.Windows.Input;

namespace WpfApp_Project_SyncFiles.Commands
{
    class ButtonCommands : ICommand
    {
        private Action _function;
        private bool canExecute;

        public ButtonCommands(Action function)
        {
            _function = function;
            canExecute = true;
        }

        #region ICommand Members
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            if (parameter != null)
            {
                canExecute = (bool)parameter;
            }

            return canExecute;
        }

        public void Execute(object parameter)
        {
            _function.Invoke();
        }
        #endregion
    }
}
