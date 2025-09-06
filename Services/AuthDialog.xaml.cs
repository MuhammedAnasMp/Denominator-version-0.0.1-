using System.Windows;
using System.Windows.Input;

namespace Deno.Services
{
    public partial class AuthDialog : Window
    {
        public string Id => IdBox.Password;
        public string Password => PasswordBox.Password;

        public bool IsAuthenticated { get; private set; }

        public AuthDialog()
        {
            InitializeComponent();

        
            Loaded += (s, e) => IdBox.Focus();

            IdBox.KeyDown += (s, e) => {
                if (e.Key == Key.Enter)
                {
                    e.Handled = true;
                    PasswordBox.Focus();
                }
            };

            PasswordBox.KeyDown += (s, e) => {
                if (e.Key == Key.Enter)
                {
                    e.Handled = true;
                    Login_Click(this, new RoutedEventArgs());
                }
            };
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Id) && !string.IsNullOrEmpty(Password))
            {
                IsAuthenticated = true;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Please enter both ID and password.",
                                "Authorization Required",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);

            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
