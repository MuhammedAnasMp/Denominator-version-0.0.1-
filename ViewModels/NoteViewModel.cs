using Deno.Services;
using System.ComponentModel;

public class NoteViewModel : INotifyPropertyChanged
{
    private int _quantity;
    private readonly CurrencyService _currencyService;

    public NoteViewModel(decimal denom, CurrencyService currencyService = null)
    {
        Denomination = denom;
        _currencyService = currencyService;
        Quantity = 0;
    }

    public decimal Denomination { get; set; }
    public int Quantity
    {
        get => _quantity;
        set
        {
            _quantity = value;
            OnPropertyChanged(nameof(Quantity));
            OnPropertyChanged(nameof(Total));
            _currencyService?.OnPropertyChanged(nameof(CurrencyService.NoteTotal));
            _currencyService?.OnPropertyChanged(nameof(CurrencyService.GrandTotal));
        }
    }
    public decimal Total => Denomination * Quantity;

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}