using Deno.Services;
using Deno.Views;
using System;
using System.Deployment.Application;
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
                // Load settings from GlobalStateService
                GlobalStateService.Instance.LoadSettings();

                // Check for updates before proceeding
                CheckForUpdates();

                // Decide whether to show the login or home window based on login state
                if (GlobalStateService.Instance.IsLoggedIn)
                {
                    // Show HomeWindow if the user is logged in
                    HomeWindow home = new HomeWindow(GlobalStateService.Instance.Username, GlobalStateService.Instance.Auth);
                    home.Show();
                }
                else
                {
                    // Show LoginWindow if the user is not logged in
                    LoginWindow login = new LoginWindow();
                    login.Show();
                }
            }
            catch (Exception ex)
            {
                // Log error and show an error message to the user
                Console.WriteLine($"Startup error: {ex.Message}\nStackTrace: {ex.StackTrace}");
                MessageBox.Show($"Startup error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                // Show the Login window in case of an error
                LoginWindow login = new LoginWindow();
                login.Show();
            }
        }

        private void CheckForUpdates()
        {
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                var deployment = ApplicationDeployment.CurrentDeployment;

                try
                {
                    var updateAvailable = deployment.CheckForDetailedUpdate();

                    if (updateAvailable.UpdateAvailable)
                    {
                        var availableVersion = updateAvailable.AvailableVersion;
                        MessageBox.Show($"Updating to version {availableVersion}...");
                        deployment.UpdateCompleted += (s, e) =>
                        {
                            MessageBox.Show($"The application will now restart with version {availableVersion}.");
                            RestartApplication();
                        };
                        deployment.UpdateAsync();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not check for updates: {ex.Message}");
                    Console.WriteLine($"Update check error: {ex.Message}\nStackTrace: {ex.StackTrace}");
                }
            }
        }

        private void RestartApplication()
        {
            try
            {
                if (ApplicationDeployment.IsNetworkDeployed && ApplicationDeployment.CurrentDeployment?.UpdateLocation != null)
                {
                    // Use ClickOnce to restart the application
                    string updateLocation = ApplicationDeployment.CurrentDeployment.UpdateLocation.ToString();
                    Console.WriteLine($"Restarting with update location: {updateLocation}");
                    System.Diagnostics.Process.Start("rundll32.exe", "dfshim.dll,ShOpenVerbApplication " + updateLocation);
                }
                else
                {
                    // Fallback: Restart using the current executable
                    string executablePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                    Console.WriteLine($"Restarting with executable path: {executablePath}");
                    System.Diagnostics.Process.Start(executablePath);
                }

                // Shut down the current instance
                System.Windows.Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to restart application: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Console.WriteLine($"Restart error: {ex.Message}\nStackTrace: {ex.StackTrace}");
            }
        }
    }
}