using Deno.Services;
using Deno.Views;
using System;
using System.Windows;

namespace Deno
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                // Load settings into GlobalStateService
                GlobalStateService.Instance.LoadSettings();

                // Log the IsLoggedIn state for debugging
                Console.WriteLine($"IsLoggedIn at startup: {GlobalStateService.Instance.IsLoggedIn}, Auth: {GlobalStateService.Instance.Auth}");

                // Check if user is logged in
                if (GlobalStateService.Instance.IsLoggedIn)
                {
                    HomeWindow home = new HomeWindow(GlobalStateService.Instance.Username, GlobalStateService.Instance.Auth);
                    home.Show();
                }
                else
                {
                    LoginWindow login = new LoginWindow();
                    login.Show();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Startup error: {ex.Message}\nStackTrace: {ex.StackTrace}");
                MessageBox.Show($"Startup error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                // Optionally open LoginWindow on error
                LoginWindow login = new LoginWindow();
                login.Show();
            }
        }
    }
}