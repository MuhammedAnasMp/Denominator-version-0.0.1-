using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

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
                    UpdateViewModels();
                    OnPropertyChanged(nameof(SelectedCurrency));
                    OnPropertyChanged(nameof(CoinViewModels));
                    OnPropertyChanged(nameof(NoteViewModels));
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
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

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
                    Coins = new List<decimal>{0.25m,0.5m,1m},
                    Notes = new List<decimal>{5,10,20,50,100,200,500,1000}
                },
                new CurrencyInfo
                {
                    Country = "Kuwait",
                    CurrencyCode = "KWD",
                    CurrencyName = "Kuwaiti Dinar",
                    CurrencySymbol = "د.ك",
                    SubunitName = "Fils",
                    SubunitPerUnit = 1000,
                    Coins = new List<decimal>{0.005m,0.01m,0.02m,0.05m,0.1m},
                    Notes = new List<decimal>{0.25m,0.5m,1,5,10,20}
                }
            };

            // Load previously selected currency
            string savedCode = Properties.Settings.Default["SelectedCurrencyCode"]?.ToString() ?? AllCurrencies.First().CurrencyCode;
            _selectedCurrency = AllCurrencies.FirstOrDefault(c => c.CurrencyCode == savedCode);
            UpdateViewModels();
        }

        private void UpdateViewModels()
        {
            CoinViewModels = SelectedCurrency?.Coins?.Select(c => new CoinViewModel(c)).ToList();
            NoteViewModels = SelectedCurrency?.Notes?.Select(n => new NoteViewModel(n)).ToList();
        }
    }

    public class CoinViewModel
    {
        public decimal Denomination { get; set; }
        public int Quantity { get; set; }
        public decimal Total => Denomination * Quantity;
        public CoinViewModel(decimal denom) { Denomination = denom; Quantity = 0; }
    }

    public class NoteViewModel
    {
        public decimal Denomination { get; set; }
        public int Quantity { get; set; }
        public decimal Total => Denomination * Quantity;
        public NoteViewModel(decimal denom) { Denomination = denom; Quantity = 0; }
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
}
