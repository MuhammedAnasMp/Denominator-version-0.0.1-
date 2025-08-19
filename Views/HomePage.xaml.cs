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
using System.Windows.Shapes;
namespace Deno.Views
{
    public partial class HomePage : Page
    {
        public string WelcomeText { get; set; }

        public HomePage(string username)
        {
            InitializeComponent();
            WelcomeText = $"Welcome, {username}!";
            DataContext = this;
        }
    }
}
