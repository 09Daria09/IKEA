using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace IKEA.Commands
{
    public class DelegateCommand : ICommand
    {
        public DelegateCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");
            _execute = execute;
            _canExecute = canExecute;
        }

        public DelegateCommand(ICommand? showAverageQuantityByTypeCommand, Func<object, bool> value)
        {
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute != null)
            {
                return _canExecute(parameter);
            }
            return true;
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        private EventHandler canExecuteChanged;
        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
                canExecuteChanged += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
                canExecuteChanged -= value;
            }
        }

        public void RaiseCanExecuteChanged()
        {
            canExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        Action<object> _execute;
        Predicate<object> _canExecute;
    }
}
