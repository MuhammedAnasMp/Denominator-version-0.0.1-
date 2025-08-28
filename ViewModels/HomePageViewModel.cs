using Deno.Services;
using System;
using System.ComponentModel;
using System.Windows.Input;

namespace Deno.ViewModels
{
    public class HomePageViewModel : INotifyPropertyChanged
    {
        private readonly GlobalStateService _globalStateService;

        public ICommand LogoutCommand { get; }

        public HomePageViewModel()
        {
            _globalStateService = GlobalStateService.Instance;
            LogoutCommand = new RelayCommand(ExecuteLogout);
        }

        private void ExecuteLogout(object parameter)
        {
          
            _globalStateService.Auth = "";
            _globalStateService.Username = "";
            _globalStateService.UserId = "";
            _globalStateService.IsLoggedIn = false;

         
            _globalStateService.SaveSettings();

  
        }

  
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;
        public void Execute(object parameter) => _execute(parameter);
    }
}