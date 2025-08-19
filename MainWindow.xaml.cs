using Deno.Views;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Deno
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            if (Properties.Settings.Default.IsLoggedIn)
            {
                // Already logged in → show Home window
                var home = new HomeWindow(Properties.Settings.Default.Username , Properties.Settings.Default.Auth);
                home.Show();
            }
            else
            {
                // Not logged in → show Login window
                var login = new LoginWindow();
                login.Show();
            }

            // Close MainWindow since it just acts as a launcher
            this.Close();
        }

    }
}
