using Deno.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
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

        public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register(
            "IsLoading", typeof(bool), typeof(HomePage), new PropertyMetadata(false));

        public static readonly DependencyProperty UpdatingRecordProperty = DependencyProperty.Register(
            "UpdatingRecord", typeof(bool), typeof(HomePage), new PropertyMetadata(false));

        public bool IsLoading
        {
            get => (bool)GetValue(IsLoadingProperty);
            set => SetValue(IsLoadingProperty, value);
        }

        public bool UpdatingRecord
        {
            get => (bool)GetValue(UpdatingRecordProperty);
            set => SetValue(UpdatingRecordProperty, value);
        }

        public HomePage(string username, CurrencyService currencyService)
        {
            InitializeComponent();
            _globalStateService = GlobalStateService.Instance;
            _currencyService = currencyService;
            WelcomeText = $"Welcome, {username}!";
            DataContext = _currencyService;
            this.SetValue(WelcomeTextProperty, WelcomeText);

            _ = LoadDenominationDataAsync();
        }

        public static readonly DependencyProperty WelcomeTextProperty = DependencyProperty.Register(
            "WelcomeText", typeof(string), typeof(HomePage), new PropertyMetadata(string.Empty));

        public ICommand UpdateTotalsCommand => new RelayCommand(UpdateTotals);

        public ICommand LogoutCommand => new RelayCommand(ExecuteLogout);

        private async Task LoadDenominationDataAsync()
        {
            try
            {
                IsLoading = true;
                using (var client = new HttpClient())
                {
                    var host = _globalStateService.DomainName;
                    var userId = _globalStateService.UserId;
                    var storeId = _globalStateService.LocCode;
                    var response = await client.GetAsync($"http://{host}/check_exist_history?Id=0&StoreId={storeId}&UserId={userId}");

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"API Response: {responseContent}");

                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var result = JsonSerializer.Deserialize<CurrencyService.GetResponse>(responseContent, options);

                        if (result?.Data != null && result.Data.Count > 0)
                        {
                            var data = result.Data[0];
                            Console.WriteLine($"API Data: KD_025={data.Kd025}, NoteTotal={data.NoteTotal}");
                            await UpdateQuantitiesFromApi(data);
                            UpdatingRecord = true; // Set to true since API data was loaded and quantities updated
                            Console.WriteLine($"UpdatingRecord set to: {UpdatingRecord}");
                            IsLoading = false;
                            MessageBox.Show("Manager ID is required to update existing records.", "Update Required", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            Console.WriteLine("API returned empty data (tables: []), skipping quantity update.");
                            UpdatingRecord = false; // No data, so not updating a record
                        }
                    }
                    else
                    {
                        Console.WriteLine($"API failed with status: {response.StatusCode}");
                        MessageBox.Show($"Failed to fetch denomination data. Status code: {response.StatusCode}",
                            "API Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        UpdatingRecord = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching denomination data: {ex}");
                MessageBox.Show($"Error fetching denomination data: {ex.Message}",
                    "API Error", MessageBoxButton.OK, MessageBoxImage.Error);
                UpdatingRecord = false;
            }
            finally
            {
                IsLoading = false; // Hide loader and enable UI
            }
        }

        private async Task UpdateQuantitiesFromApi(CurrencyService.DenominationData data)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (_currencyService.SelectedCurrency?.CurrencyCode != "KWD")
                {
                    Console.WriteLine("Setting currency to KWD");
                    _currencyService.SelectedCurrency = _currencyService.AllCurrencies
                        .FirstOrDefault(c => c.CurrencyCode == "KWD");
                }

                foreach (var coin in _currencyService.CoinViewModels)
                {
                    switch (coin.Denomination)
                    {
                        case 0.005m:
                            coin.Quantity = data.Kd0005;
                            break;
                        case 0.01m:
                            coin.Quantity = data.Kd001;
                            break;
                        case 0.02m:
                            coin.Quantity = data.Kd002;
                            break;
                        case 0.05m:
                            coin.Quantity = data.Kd005;
                            break;
                        case 0.1m:
                            coin.Quantity = data.Kd01;
                            break;
                        default:
                            Console.WriteLine($"Unknown coin denomination: {coin.Denomination}");
                            break;
                    }
                    Console.WriteLine($"Updated Coin {coin.Denomination}: Quantity={coin.Quantity}");
                }

                foreach (var note in _currencyService.NoteViewModels)
                {
                    switch (note.Denomination)
                    {
                        case 0.25m:
                            note.Quantity = data.Kd025;
                            break;
                        case 0.5m:
                            note.Quantity = data.Kd05;
                            break;
                        case 1m:
                            note.Quantity = data.Kd1;
                            break;
                        case 5m:
                            note.Quantity = data.Kd5;
                            break;
                        case 10m:
                            note.Quantity = data.Kd10;
                            break;
                        case 20m:
                            note.Quantity = data.Kd20;
                            break;
                        default:
                            Console.WriteLine($"Unknown note denomination: {note.Denomination}");
                            break;
                    }
                    Console.WriteLine($"Updated Note {note.Denomination}: Quantity={note.Quantity}");
                }

                // Force UI refresh
                _currencyService.OnPropertyChanged(nameof(CurrencyService.CoinViewModels));
                _currencyService.OnPropertyChanged(nameof(CurrencyService.NoteViewModels));
                _currencyService.OnPropertyChanged(nameof(CurrencyService.CoinTotal));
                _currencyService.OnPropertyChanged(nameof(CurrencyService.NoteTotal));
                _currencyService.OnPropertyChanged(nameof(CurrencyService.GrandTotal));
            });
        }

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