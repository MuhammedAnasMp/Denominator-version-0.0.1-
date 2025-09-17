using Deno.Services;
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

            // -----------------------------
            // Restore size & position if saved, otherwise set full-screen width with height 560
            // -----------------------------
            bool hasSavedSize = Properties.Settings.Default.WindowWidth > 0 &&
                                Properties.Settings.Default.WindowHeight > 0 &&
                                Properties.Settings.Default.WindowTop >= 0 &&
                                Properties.Settings.Default.WindowLeft >= 0;

            if (hasSavedSize)
            {
                // Use saved size/position
                this.WindowState = WindowState.Normal; // Ensure normal state to apply custom dimensions
                this.WindowStyle = WindowStyle.None; // Remove window borders
                this.Width = Properties.Settings.Default.WindowWidth;
                this.Height = Properties.Settings.Default.WindowHeight;
                this.Top = Properties.Settings.Default.WindowTop;
                this.Left = Properties.Settings.Default.WindowLeft;
            }
            else
            {
                // Initial run: set full-screen width, height=560, top=0
                this.WindowState = WindowState.Normal; // Ensure normal state to apply custom dimensions
                this.WindowStyle = WindowStyle.None; // Remove window borders
                var screen = Screen.PrimaryScreen.Bounds;
                this.Width = screen.Width;
                this.Height = 560; // Set fixed height to 560 pixels
                this.Top = 0;
                this.Left = screen.Left;
            }

            // -----------------------------
            // Your existing content setup
            // -----------------------------
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
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);

            // Save current size & position
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