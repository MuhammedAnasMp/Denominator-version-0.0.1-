// Add this class to your project
using System.Windows;

namespace Deno.Services
{
    // Update the AuthDialog class
public partial class AuthDialog : Window
{
    public string Username { get; set; }
    public string Password => PasswordBox.Password; // Expose the password
    public bool IsAuthenticated { get; private set; }

    public AuthDialog()
    {
        InitializeComponent();
        DataContext = this;
    }

    private void Login_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password))
        {
            IsAuthenticated = true;
            DialogResult = true;
        }
        else
        {
            MessageBox.Show("Please enter both username and password.");
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
}