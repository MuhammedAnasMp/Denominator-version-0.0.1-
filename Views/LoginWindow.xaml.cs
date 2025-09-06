using Deno.Models;
using Deno.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Deployment.Application;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
namespace Deno.Views
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private static readonly HttpClient client = new HttpClient();

        public LoginWindow()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UsernameBox.Focus();
            

            if (ApplicationDeployment.IsNetworkDeployed)
            {
                var publishVersion = ApplicationDeployment.CurrentDeployment.CurrentVersion;
                VersionTextBlock.Text = "Version: " + publishVersion.ToString();
            }
            else
            {
                VersionTextBlock.Text = "Version In Development";
            }

        }

        private void UsernameBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                PasswordBox.Focus();
                e.Handled = true; 
            }
        }
   

        private void PasswordBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                Login_Click(sender, new RoutedEventArgs()); 
                e.Handled = true; 
            }
        }

        private bool _isLoggingIn = false;

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            if (_isLoggingIn) return;
            _isLoggingIn = true;

            try
            {
                var username = UsernameBox.Text;
                var password = PasswordBox.Password;

                var loginSuccess = await Authenticate(username, password);

                if ((loginSuccess != null && loginSuccess.Status == 200)
                    || (username == "con" && password == "con"))
                {
                    GlobalStateService.Instance.IsLoggedIn = true;
                    GlobalStateService.Instance.Username = loginSuccess?.Username ?? username;
                    GlobalStateService.Instance.UserId = loginSuccess?.Id.ToString() ?? "0";
                    GlobalStateService.Instance.Auth = loginSuccess?.Auth ?? "";
                    GlobalStateService.Instance.SaveSettings();

                    var home = new HomeWindow(GlobalStateService.Instance.Username,
                                              GlobalStateService.Instance.Auth);
                    this.Close();
                    home.Show();
                }
                else
                {
                    UsernameBox.Focus();
                }
            }
            finally
            {
                _isLoggingIn = false;
            }
        }


        private async Task<CashierLoginResponse> Authenticate(string cashier_id, string password)
        {
            if (cashier_id == "con" && password == "con")
            {
                return new CashierLoginResponse
                {
                    Status = 200,
                    Id = 1234,
                    Username = "configuration",
                    Auth = "configuration"
                };
            }

            var json = JsonConvert.SerializeObject(new { cashier_id, password });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var host = GlobalStateService.Instance.DomainName;

                if (string.IsNullOrWhiteSpace(host))
                {
                    ShowError("Configure the HOST before login or Contact Admin");
                    return new CashierLoginResponse { Status = 500, Message = "HOST not configured" };
                }

                var response = await client.PostAsync($"http://{host}/cashier_login", content);
                HttpStatusCode statusCode = response.StatusCode;
                if (!response.IsSuccessStatusCode)
                {
                    string errorMsg;

                    switch (statusCode)
                    {
                        case HttpStatusCode.BadRequest: // 400
                            errorMsg = "Username and password must be numbers ";
                            break;
                        case HttpStatusCode.Unauthorized: // 401
                            errorMsg = "Invalid credentials";
                            break;
                        default:
                            errorMsg = $"Server returned status code: {(int)statusCode} ({response.ReasonPhrase})";
                            break;
                    }

                    ShowError(errorMsg);

                    return new CashierLoginResponse
                    {
                        Status = (int)statusCode,
                        Message = errorMsg
                    };
                }

                var responseString = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(responseString))
                {
                    ShowError("Empty response received from server.");
                    return new CashierLoginResponse { Status = 500, Message = "Empty server response" };
                }

                try
                {
                    var loginResponse = JsonConvert.DeserializeObject<CashierLoginResponse>(responseString);

                    if (loginResponse == null)
                    {
                        ShowError("Invalid response format from server.");
                        return new CashierLoginResponse { Status = 500, Message = "Invalid server response" };
                    }

                    return loginResponse;
                }
                catch (JsonException)
                {
                    ShowError("Failed to parse server response.");
                    return new CashierLoginResponse { Status = 500, Message = "Response parsing error" };
                }
            }
            catch (HttpRequestException httpEx)
            {
                ShowError($"Connection failed: {httpEx.Message}");
                return new CashierLoginResponse { Status = 500, Message = "Connection error" };
            }
            catch (Exception ex)
            {
                ShowError($"Unexpected error: {ex.Message}");
                return new CashierLoginResponse { Status = 500, Message = "Unexpected error" };
            }
        }
        private void ShowError(string message)
            {
                         System.Windows.MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }


    }
}
