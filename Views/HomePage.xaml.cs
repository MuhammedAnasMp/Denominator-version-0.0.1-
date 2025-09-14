using Deno.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace Deno.Views
{
    public partial class HomePage : Page
    {
        public string WelcomeText
        {
            get => (string)GetValue(WelcomeTextProperty);
            set => SetValue(WelcomeTextProperty, value);
        }
        private readonly CurrencyService _currencyService;
        private readonly GlobalStateService _globalStateService;
        private readonly string _username;

        public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register(
            "IsLoading", typeof(bool), typeof(HomePage), new PropertyMetadata(false));


     
        public bool IsLoading
        {
            get => (bool)GetValue(IsLoadingProperty);
            set => SetValue(IsLoadingProperty, value);
        }

       
        

        public HomePage(string username, CurrencyService currencyService)
        {
            InitializeComponent();
            Loaded += HomePage_Loaded;
            _globalStateService = GlobalStateService.Instance;
            _currencyService = currencyService;
            _username = username;
            WelcomeText = $"Welcome, {username}!";
            DataContext = _currencyService;
            this.SetValue(WelcomeTextProperty, WelcomeText);

            _ = LoadDenominationDataAsync();
        
        }

        private void HomePage_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new System.Action(() =>
            {
                if (CoinsItemsControl.Items.Count > 0)
                {
                    var firstItemContainer = CoinsItemsControl.ItemContainerGenerator.ContainerFromIndex(0) as FrameworkElement;
                    if (firstItemContainer != null)
                    {
                        var textBox = FindVisualChild<TextBox>(firstItemContainer, "CoinQuantityTextBox");
                        if (textBox != null && textBox.IsEnabled)
                        {
                            textBox.Focus();
                            textBox.SelectAll(); 
                        }
                    }
                }
            }));
        }

        private T FindVisualChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T childType && child is FrameworkElement frameworkElement && frameworkElement.Name == childName)
                {
                    return childType;
                }
                var result = FindVisualChild<T>(child, childName);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        public static readonly DependencyProperty WelcomeTextProperty = DependencyProperty.Register(
            "WelcomeText", typeof(string), typeof(HomePage), new PropertyMetadata(string.Empty));

        public ICommand UpdateTotalsCommand => new RelayCommand(UpdateTotals);

        public ICommand LogoutCommand => new RelayCommand(ExecuteLogout);


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        private async Task LoadDenominationDataAsync()
        {
            try
            {
                IsLoading = true;
                using (var client = new HttpClient())
                {
                    var host = _globalStateService.DomainName;
                    var userId = _globalStateService.UserId;
                    var locCode = _globalStateService.LocCode;
                    var response = await client.GetAsync($"http://{host}/existing_history?Id=0&LocCode={locCode}&UserId={userId}");
                   
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"API Response: {responseContent}");

                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var result = JsonSerializer.Deserialize<CurrencyService.GetResponse>(responseContent, options);

                        await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            if (result?.Data != null && result.Data.Count > 0)
                            {
                                var data = result.Data[0];

                                UpdateQuantitiesFromApi(data);
                                _currencyService.UpdatingRecord = true;
                                _currencyService.EditMod = true;
                                _currencyService.InitialAmount = data.OpeningAmount;

                                _currencyService.UpdatingRecordId = result.Id;
                                WelcomeText = $"Wellcome {_globalStateService.Username} : You can update your record.";
                                IsLoading = false;
                                //MessageBox.Show($"Manager ID is required to update existing records. {result.Id}", "Update Required", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            else
                            {
                                Console.WriteLine("API returned empty data (tables: []), skipping quantity update.");
                                _currencyService.UpdatingRecord = false;
                                WelcomeText = $"Welcome, {_username}!";
                                Console.WriteLine($"UpdatingRecord set to: {_currencyService.UpdatingRecord }, WelcomeText: {WelcomeText}");
                            }
                        });
                    }
                    else
                    {
                        Console.WriteLine($"API failed with status: {response.StatusCode}");
                        await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            _currencyService.UpdatingRecord = false;
                            WelcomeText = $"Welcome, {_username}!";
                            MessageBox.Show($"Failed to fetch denomination data. Status code: {response.StatusCode}",
                                "API Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    _currencyService.UpdatingRecord = false;
                    WelcomeText = $"Welcome, {_username}!";
                    MessageBox.Show($"Error fetching denomination data: {ex.Message}",
                        "API Error", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
            finally
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    IsLoading = false;
                    Console.WriteLine($"Final state: UpdatingRecord={_currencyService.UpdatingRecord}");
                });
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