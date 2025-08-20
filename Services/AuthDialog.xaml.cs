using System.Windows;

namespace Deno.Services
{
    public partial class AuthDialog : Window
    {
        // Expose the hidden ID and password
        public string Id => IdBox.Password;
        public string Password => PasswordBox.Password;

        public bool IsAuthenticated { get; private set; }

        public AuthDialog()
        {
            InitializeComponent();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Id) && !string.IsNullOrEmpty(Password))
            {
                // Optionally: add your authentication logic here
                IsAuthenticated = true;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Please enter both ID and password.", "Authorization Required", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
