using Deno.Services;
using System.Deployment.Application;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
namespace Deno.Views
{
    public partial class HomeWindow : Window
    {
        private string _auth;

        public HomeWindow(string username, string auth)
        {
            InitializeComponent();

            _auth = auth;

            bool hasSavedSize = Properties.Settings.Default.WindowWidth > 0 &&
                                Properties.Settings.Default.WindowHeight > 0 &&
                                Properties.Settings.Default.WindowTop >= 0 &&
                                Properties.Settings.Default.WindowLeft >= 0;

            if (hasSavedSize)
            {
            
                this.WindowState = WindowState.Normal; 
                this.WindowStyle = WindowStyle.None; 
                this.Width = Properties.Settings.Default.WindowWidth;
                this.Height = Properties.Settings.Default.WindowHeight;
                this.Top = Properties.Settings.Default.WindowTop;
                this.Left = Properties.Settings.Default.WindowLeft;
            }
            else
            {
           
                this.WindowState = WindowState.Normal; 
                this.WindowStyle = WindowStyle.None; 
                var screen = Screen.PrimaryScreen.Bounds;
                this.Width = screen.Width;
                this.Height = 560; 
                this.Top = 0;
                this.Left = screen.Left;
            }

        
            var currencyService = new CurrencyService();

            if (auth == "configuration")
            {
                HomeFrame.Content = new ConfigPage();
            }
            else
            {
                HomeFrame.Content = new HomePage(username, currencyService);
                ManageFrame.Content = new ManagePage();
                ConfigFrame.Content = new ConfigPage();
            }

            SetTabVisibility(GlobalStateService.Instance.Auth);
            if (auth != "configuration")
                SentVersionInfoAsync();
        }

        private void SentVersionInfoAsync()
        {
            Task.Run(async () =>
            {
                try
                {
                    using (var client = new HttpClient())
                    {
                        var host = GlobalStateService.Instance.DomainName;
                        string publishVersion;
                        if (ApplicationDeployment.IsNetworkDeployed)
                        {
                            publishVersion = ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
                        }
                        else
                        {
                            publishVersion = "0.0.0.0";
                        }
                        var payload = new
                        {
                            loc_code = GlobalStateService.Instance.LocCode,
                            loc_name = GlobalStateService.Instance.LocName,
                            pos_number = GlobalStateService.Instance.PosNumber,
                            dev_ip = GlobalStateService.Instance.DeveIp,
                            current_version = publishVersion
                        };
                        var json = JsonSerializer.Serialize(payload);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        var response = await client.PostAsync($"http://{host}/version", content);
                        _ = response;
                    }
                }
                catch
                {
                }
            });
        }
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);

            Properties.Settings.Default.WindowWidth = (int)this.Width;
            Properties.Settings.Default.WindowHeight = (int)this.Height;
            Properties.Settings.Default.WindowTop = (int)this.Top;
            Properties.Settings.Default.WindowLeft = (int)this.Left;
            Properties.Settings.Default.Save();
        }

        private void SetTabVisibility(string auth)
        {
            switch (auth?.ToLower())
            {
                case "cashier":
                    HomeTab.Visibility = Visibility.Collapsed;
                    ManageTab.Visibility = Visibility.Collapsed;
                    ConfigTab.Visibility = Visibility.Collapsed;
                    break;
                case "supervisor":
                    HomeTab.Visibility = Visibility.Visible;
                    ManageTab.Visibility = Visibility.Visible;
                    ConfigTab.Visibility = Visibility.Collapsed;
                    break;
                case "admin":
                    HomeTab.Visibility = Visibility.Visible;
                    ManageTab.Visibility = Visibility.Visible;
                    ConfigTab.Visibility = Visibility.Visible;
                    break;
                case "configuration":
                    HomeTab.Visibility = Visibility.Collapsed;
                    ManageTab.Visibility = Visibility.Collapsed;
                    ConfigTab.Visibility = Visibility.Collapsed;
                    break;
                default:
                    HomeTab.Visibility = Visibility.Visible;
                    ManageTab.Visibility = Visibility.Collapsed;
                    ConfigTab.Visibility = Visibility.Collapsed;
                    break;
            }
        }
    }
}