using System.ComponentModel;

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

    public CoinViewModel(decimal denom)
    {
        Denomination = denom;
        Quantity = 0;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}