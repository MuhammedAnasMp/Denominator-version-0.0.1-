using Deno.Services;
using Deno.Views;
using System;
using System.Windows;

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
                GlobalStateService.Instance.LoadSettings();

             
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
                LoginWindow login = new LoginWindow();
                login.Show();
            }
        }
    }
}