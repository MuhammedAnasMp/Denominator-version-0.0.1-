using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace Deno.Services
{
    public class CurrencyService : INotifyPropertyChanged
    {
        public List<CurrencyInfo> AllCurrencies { get; set; }
        private CurrencyInfo _selectedCurrency;

        public CurrencyInfo SelectedCurrency
        {
            get => _selectedCurrency;
            set
            {
                if (_selectedCurrency != value)
                {
                    _selectedCurrency = value;
                    // Update GlobalStateService and save to JSON
                    GlobalStateService.Instance.CurrencyCode = value?.CurrencyCode ?? "AED";
                    GlobalStateService.Instance.SaveSettings();
                    UpdateViewModels();
                    OnPropertyChanged(nameof(SelectedCurrency));
                    OnPropertyChanged(nameof(CoinViewModels));
                    OnPropertyChanged(nameof(NoteViewModels));
                    OnPropertyChanged(nameof(CoinTotal));
                    OnPropertyChanged(nameof(NoteTotal));
                    OnPropertyChanged(nameof(GrandTotal));
                    Console.WriteLine($"CurrencyService: SelectedCurrency changed to {value?.CurrencyCode}");
                }
            }
        }

        private List<CoinViewModel> _coinViewModels;
        public List<CoinViewModel> CoinViewModels
        {
            get => _coinViewModels;
            private set
            {
                _coinViewModels = value;
                OnPropertyChanged(nameof(CoinViewModels));
                OnPropertyChanged(nameof(CoinTotal));
                OnPropertyChanged(nameof(GrandTotal));
            }
        }

        private List<NoteViewModel> _noteViewModels;
        public List<NoteViewModel> NoteViewModels
        {
            get => _noteViewModels;
            private set
            {
                _noteViewModels = value;
                OnPropertyChanged(nameof(NoteViewModels));
                OnPropertyChanged(nameof(NoteTotal));
                OnPropertyChanged(nameof(GrandTotal));
            }
        }

        public decimal CoinTotal => CoinViewModels?.Sum(c => c.Total) ?? 0;
        public decimal NoteTotal => NoteViewModels?.Sum(n => n.Total) ?? 0;
        public decimal GrandTotal => CoinTotal + NoteTotal;

        public ICommand ResetCommand => new RelayCommand(Reset);
        public ICommand PostToApiCommand => new RelayCommand(PostToApi);

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public CurrencyService()
        {
            AllCurrencies = new List<CurrencyInfo>
            {
                new CurrencyInfo
                {
                    Country = "United Arab Emirates",
                    CurrencyCode = "AED",
                    CurrencyName = "UAE Dirham",
                    CurrencySymbol = "د.إ",
                    SubunitName = "Fils",
                    SubunitPerUnit = 100,
                    Coins = new List<decimal> { 0.25m, 0.5m, 1m },
                    Notes = new List<decimal> { 5, 10, 20, 50, 100, 200, 500, 1000 }
                },
                new CurrencyInfo
                {
                    Country = "Kuwait",
                    CurrencyCode = "KWD",
                    CurrencyName = "Kuwaiti Dinar",
                    CurrencySymbol = "د.ك",
                    SubunitName = "Fils",
                    SubunitPerUnit = 1000,
                    Coins = new List<decimal> { 0.005m, 0.01m, 0.02m, 0.05m, 0.1m },
                    Notes = new List<decimal> { 0.25m, 0.5m, 1, 5, 10, 20 }
                }
            };

            // Use GlobalStateService instead of Properties.Settings.Default
            string savedCode = GlobalStateService.Instance.CurrencyCode ?? AllCurrencies.First().CurrencyCode;
            _selectedCurrency = AllCurrencies.FirstOrDefault(c => c.CurrencyCode == savedCode) ?? AllCurrencies.First();
            UpdateViewModels();
            Console.WriteLine($"CurrencyService initialized with CurrencyCode: {savedCode}");
        }

        private void UpdateViewModels()
        {
            // Clear existing event subscriptions
            if (CoinViewModels != null)
            {
                foreach (var coin in CoinViewModels)
                {
                    coin.PropertyChanged -= OnCoinPropertyChanged;
                }
            }
            if (NoteViewModels != null)
            {
                foreach (var note in NoteViewModels)
                {
                    note.PropertyChanged -= OnNotePropertyChanged;
                }
            }

            // Create new view models
            CoinViewModels = SelectedCurrency?.Coins?.Select(c =>
            {
                var vm = new CoinViewModel(c, this);
                vm.PropertyChanged += OnCoinPropertyChanged;
                return vm;
            }).ToList();

            NoteViewModels = SelectedCurrency?.Notes?.Select(n =>
            {
                var vm = new NoteViewModel(n, this);
                vm.PropertyChanged += OnNotePropertyChanged;
                return vm;
            }).ToList();
        }
        private void OnNotePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(NoteViewModel.Quantity) ||
                e.PropertyName == nameof(NoteViewModel.Total))
            {
                OnPropertyChanged(nameof(NoteTotal));
                OnPropertyChanged(nameof(GrandTotal));
            }
        }

        private void OnCoinPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CoinViewModel.Quantity) ||
                e.PropertyName == nameof(CoinViewModel.Total))
            {
                OnPropertyChanged(nameof(CoinTotal));
                OnPropertyChanged(nameof(GrandTotal));
            }
        }


        private void Reset(object parameter)
        {
            foreach (var coin in CoinViewModels)
                coin.Quantity = 0;
            foreach (var note in NoteViewModels)
                note.Quantity = 0;
            OnPropertyChanged(nameof(CoinViewModels));
            OnPropertyChanged(nameof(NoteViewModels));
            OnPropertyChanged(nameof(CoinTotal));
            OnPropertyChanged(nameof(NoteTotal));
            OnPropertyChanged(nameof(GrandTotal));
        }

        private void PostToApi(object parameter)
        {
            // Placeholder for API call
        }
    }

    public class CoinViewModel : INotifyPropertyChanged
    {
        private int _quantity;
        public decimal Denomination { get; set; }
        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged(nameof(Quantity));
                OnPropertyChanged(nameof(Total));
            }
        }
        public decimal Total => Denomination * Quantity;

        public CoinViewModel(decimal denom, CurrencyService currencyService) { Denomination = denom; Quantity = 0; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class NoteViewModel : INotifyPropertyChanged
    {
        private int _quantity;
        public decimal Denomination { get; set; }
        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged(nameof(Quantity));
                OnPropertyChanged(nameof(Total));
            }
        }
        public decimal Total => Denomination * Quantity;

        public NoteViewModel(decimal denom, CurrencyService currencyService) { Denomination = denom; Quantity = 0; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class CurrencyInfo
    {
        public string Country { get; set; }
        public string CurrencyCode { get; set; }
        public string CurrencyName { get; set; }
        public string CurrencySymbol { get; set; }
        public string SubunitName { get; set; }
        public int SubunitPerUnit { get; set; }
        public List<decimal> Coins { get; set; }
        public List<decimal> Notes { get; set; }
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