using System;
using System.ComponentModel;
using System.IO;
using System.Text.Json;

namespace Deno.Services
{
    public class GlobalStateService : INotifyPropertyChanged
    {
        private static GlobalStateService _instance = new GlobalStateService();
        public static GlobalStateService Instance => _instance;

        private string _locCode = "";
        private string _posNumber = "";
        private string _printerName = "";
        private string _domainName = "";
        private string _currencyCode = "AED";
        private bool _isLoggedIn;
        private string _username = "";
        private string _userId = "";
        private string _auth = "";
        private string _locName = "";
        private string _deveIp = "";

        public string DeveIp
        {
            get => _deveIp;
            set { _deveIp = value ?? ""; OnPropertyChanged(nameof(DeveIp)); }
        }

        public string LocName
        {
            get => _locName;
            set { _locName = value ?? ""; OnPropertyChanged(nameof(LocName)); }
        }

        public string LocCode
        {
            get => _locCode;
            set { _locCode = value ?? ""; OnPropertyChanged(nameof(LocCode)); }
        }
        public string PrinterName
        {
            get => _printerName;
            set { _printerName = value ?? ""; OnPropertyChanged(nameof(PrinterName)); }
        }

        public string PosNumber
        {
            get => _posNumber;
            set { _posNumber = value ?? ""; OnPropertyChanged(nameof(PosNumber)); }
        }

        public string DomainName
        {
            get => _domainName;
            set { _domainName = value ?? ""; OnPropertyChanged(nameof(DomainName)); }
        }

        public string CurrencyCode
        {
            get => _currencyCode;
            set { _currencyCode = value ?? "AED"; OnPropertyChanged(nameof(CurrencyCode)); }
        }

        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set { _isLoggedIn = value; OnPropertyChanged(nameof(IsLoggedIn)); }
        }

        public string Username
        {
            get => _username;
            set { _username = value ?? ""; OnPropertyChanged(nameof(Username)); }
        }

        public string UserId
        {
            get => _userId;
            set { _userId = value ?? ""; OnPropertyChanged(nameof(UserId)); }
        }

        public string Auth
        {
            get => _auth;
            set { _auth = value ?? ""; OnPropertyChanged(nameof(Auth)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static readonly string SettingsFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Deno",
            "settings.json");

        public void SaveSettings()
        {
            try
            {
                var settings = new AppSettings
                {
                    LocName = LocName,
                    LocCode = LocCode,
                    PrinterName = PrinterName , 
                    PosNumber = PosNumber,
                    DomainName = DomainName,
                    CurrencyCode = CurrencyCode,
                    IsLoggedIn = IsLoggedIn,
                    Username = Username,
                    UserId = UserId,
                    Auth = Auth ,
                    DevIp = DeveIp
                };

                Directory.CreateDirectory(Path.GetDirectoryName(SettingsFilePath));
                string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsFilePath, json);
                Console.WriteLine($"Settings saved to {SettingsFilePath}: {json}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save settings: {ex.Message}\nStackTrace: {ex.StackTrace}");
                throw; // Re-throw to let caller handle
            }
        }

        public void LoadSettings()
        {
            try
            {
                Console.WriteLine($"Attempting to load settings from {SettingsFilePath}");
                if (File.Exists(SettingsFilePath))
                {
                    string json = File.ReadAllText(SettingsFilePath);
                    Console.WriteLine($"Raw JSON: {json}");
                    var settings = JsonSerializer.Deserialize<AppSettings>(json);
                    LocName = settings.LocName ?? "";
                    LocCode = settings.LocCode ?? "";
                    PosNumber = settings.PosNumber ?? "";
                    PrinterName = settings.PrinterName ?? "";
                    DomainName = settings.DomainName ?? "";
                    CurrencyCode = settings.CurrencyCode ?? "AED";
                    IsLoggedIn = settings.IsLoggedIn;
                    Username = settings.Username ?? "";
                    UserId = settings.UserId ?? "";
                    Auth = settings.Auth ?? "";
                    DeveIp = settings.DevIp ?? "";

                    Console.WriteLine($"Settings loaded: LocCode={LocCode}, PosNumber={PosNumber}, DomainName={DomainName}, CurrencyCode={CurrencyCode}, IsLoggedIn={IsLoggedIn}, Username={Username}, UserId={UserId}, Auth={Auth}");
                }
                else
                {
                    Console.WriteLine("No settings file found, using defaults.");
                    LocCode = "";
                    LocName = "";
                    PrinterName = "";
                    PosNumber = "";
                    DomainName = "";
                    CurrencyCode = "KWD";
                    IsLoggedIn = false;
                    Username = "";
                    UserId = "";
                    Auth = "";
                    DeveIp = "";

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load settings: {ex.Message}\nStackTrace: {ex.StackTrace}");
                LocCode = "";
                LocName = "";
                PosNumber = "";
                DomainName = "";
                CurrencyCode = "AED";
                IsLoggedIn = false;
                Username = "";
                UserId = "";
                Auth = "";
                throw; // Re-throw to let caller handle
            }
        }
    }

    public class AppSettings
    {
        public string DevIp { get; set; }
        public string LocName { get; set; }
        public string LocCode { get; set; }
        public string PosNumber { get; set; }
        public string DomainName { get; set; }
        public string CurrencyCode { get; set; }
        public bool IsLoggedIn { get; set; }
        public string Username { get; set; }
        public string UserId { get; set; }
        public string Auth { get; set; }
        public string PrinterName { get; set; }
    }
}