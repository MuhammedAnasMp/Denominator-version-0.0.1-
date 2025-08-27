using Deno.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Deno.Views
{
    /// <summary>
    /// Interaction logic for HomeWindow.xaml
    /// </summary>
    public partial class HomeWindow : Window
    {
        private string _auth;

        public HomeWindow(string username, string auth)
        {
            InitializeComponent();
            _auth = auth;
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

        private void SetTabVisibility(string auth)
        {
            switch (auth?.ToLower())
            {
                case "cashier":
                    HomeTab.Visibility = Visibility.Visible;
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
                    HomeTab.Visibility = Visibility.Visible;
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
