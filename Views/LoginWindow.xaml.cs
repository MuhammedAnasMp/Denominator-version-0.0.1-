using Deno.Models;
using Deno.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
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

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            var username = UsernameBox.Text;
            var password = PasswordBox.Password;

            var loginSuccess = await Authenticate(username, password);

            if (loginSuccess != null && loginSuccess.Status == 200 || username=="config" && password =="config")
            {
                GlobalStateService.Instance.IsLoggedIn = true;
                GlobalStateService.Instance.Username = loginSuccess.Username;
            
                GlobalStateService.Instance.UserId = loginSuccess.Id.ToString();
                GlobalStateService.Instance.Auth = loginSuccess.Auth;
                GlobalStateService.Instance.SaveSettings();

                var home = new HomeWindow(loginSuccess.Username, loginSuccess.Auth);
                home.Show();
                this.Close();
            }
          
            //else
            //{
            //    System.Windows.MessageBox.Show("Authentication Failed!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //}
        }

        private async Task<LoginResponse> Authenticate(string username, string password)
        {
            // ✅ Bypass API when in config mode
            if (username == "config" && password == "config")
            {
                return new LoginResponse
                {
                    Status = 200,
                    Username = "Admin configuration",
                    Id = 0, // or some dummy value
                    Auth = "config"
                };
            }

            var json = JsonConvert.SerializeObject(new { username, password });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var host = GlobalStateService.Instance.DomainName;
                var response = await client.PostAsync($"http://{host}/cashier_login", content);
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<LoginResponse>(responseString);
                }
            }
            catch (Exception ex)
            {
                if (GlobalStateService.Instance.DomainName == "")
                {
                    System.Windows.MessageBox.Show("Configure the HOST before login or Contact Admin ", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {

                    System.Windows.MessageBox.Show($"{ex.Message}{GlobalStateService.Instance.DomainName} ", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            return null;
        }
    }
}
