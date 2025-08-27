using Deno.Services;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace Deno.Views
{
    public partial class HomePage : Page
    {
        public string WelcomeText { get; set; }
        private readonly CurrencyService _currencyService;
        private readonly GlobalStateService _globalStateService;

        public HomePage(string username, CurrencyService currencyService)
        {
            InitializeComponent();
            _globalStateService = GlobalStateService.Instance;
            _currencyService = currencyService;
            WelcomeText = $"Welcome, {username}!";
            DataContext = _currencyService;
            this.SetValue(WelcomeTextProperty, WelcomeText);
        }

        public static readonly DependencyProperty WelcomeTextProperty =
            DependencyProperty.Register("WelcomeText", typeof(string), typeof(HomePage), new PropertyMetadata(string.Empty));

        public ICommand UpdateTotalsCommand => new RelayCommand(UpdateTotals);

        public ICommand LogoutCommand => new RelayCommand(ExecuteLogout);

        private void UpdateTotals(object parameter)
        {
            _currencyService.OnPropertyChanged(nameof(CurrencyService.CoinViewModels));
            _currencyService.OnPropertyChanged(nameof(CurrencyService.NoteViewModels));
        }


        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                textBox.SelectAll();

            }
        }



        private void ExecuteLogout(object parameter)
        {
          
            _globalStateService.Auth = "";
            _globalStateService.Username = "";
            _globalStateService.UserId = "";
            _globalStateService.IsLoggedIn = false;

          
            _globalStateService.SaveSettings();

          
            LoginWindow login = new LoginWindow();
            Window.GetWindow(this)?.Close();
            MessageBox.Show("You have been logged out.", "Logout", MessageBoxButton.OK, MessageBoxImage.Information);
            login.Show();
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);
        public void Execute(object parameter) => _execute(parameter);
    }
}