using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WpfAppFireWall_UserVersion.MVVM.viewModel
{
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<object, bool> _canExecute;

        public RelayCommand(Action execute, Func<object, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            //if (Debugger.IsAttached)
            //{
            //    // Если программа находится в режиме отладки, возвращаем false, чтобы предотвратить выполнение команды
            //    return false;
            //}
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute();
        }
    }
}
