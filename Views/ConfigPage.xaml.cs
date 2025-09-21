using Deno.Services;
using PrintHTML.Core.Helpers;
using PrintHTML.Core.Services;
using System;
using System.Drawing.Printing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;
namespace Deno.Views
{
    public partial class ConfigPage : UserControl
    {
        private string localIP;
        private string printerName;

        public ConfigPage()
        {
            InitializeComponent();

            // Populate currency picker
            //CurrencyPicker.ItemsSource = new[] { "AED", "KWD", "USD" };
            CurrencyPicker.ItemsSource = new[] { "KWD" }; 

            // Set selected item based on loaded CurrencyCode
            CurrencyPicker.SelectedItem = GlobalStateService.Instance.CurrencyCode ?? "KWD";

             localIP = NetworkInterface.GetAllNetworkInterfaces()
             .Where(nic => nic.OperationalStatus == OperationalStatus.Up &&
                           nic.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                           nic.NetworkInterfaceType != NetworkInterfaceType.Tunnel)
             .SelectMany(nic => nic.GetIPProperties().UnicastAddresses)
             .Where(ip => ip.Address.AddressFamily == AddressFamily.InterNetwork)
             .Select(ip => ip.Address.ToString())
             .FirstOrDefault();

            DeviceIpTextBox.Text = localIP ?? "Not found";

            DeviceIpTextBox.Text = localIP ?? "Not found";
        }

        private void CurrencyPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CurrencyPicker.SelectedItem != null)
            {
                GlobalStateService.Instance.CurrencyCode = CurrencyPicker.SelectedItem.ToString();

                Console.WriteLine($"CurrencyPicker changed: CurrencyCode={GlobalStateService.Instance.CurrencyCode}");
            }
        }

        private async void OnTestDomainClicked(object sender, RoutedEventArgs e)
        {
            string domain = DomainNameTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(domain))
            {
                MessageBox.Show("Please enter a valid domain.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                string url = domain.StartsWith("http") ? domain : $"http://{domain}/";

                using (HttpClient client = new HttpClient())
                {
                   
                    var content = new StringContent("{}", Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(url, content);

                    response.EnsureSuccessStatusCode();

                    string responseContent = await response.Content.ReadAsStringAsync();

                
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var result = JsonSerializer.Deserialize<ApiResponse>(responseContent, options);

                    MessageBox.Show($"App Name: {result.AppName}\nApp Version: {result.AppVersion}",
                                    "API Response", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"API call failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public class ApiResponse
        {
            [JsonPropertyName("app_name")]
            public string AppName { get; set; }

            [JsonPropertyName("app_version")]
            public string AppVersion { get; set; }
        }




        private void PrinterComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            // Populate ComboBox with installed printers
            PrinterComboBox.Items.Clear();
            foreach (string printer in PrinterSettings.InstalledPrinters)
            {
                //if (printer.IndexOf("Microsoft", StringComparison.OrdinalIgnoreCase) >= 0) continue;
                //if (printer.IndexOf("OneNote", StringComparison.OrdinalIgnoreCase) >= 0) continue;
                //if (printer.IndexOf("XPS", StringComparison.OrdinalIgnoreCase) >= 0) continue;
                //if (printer.IndexOf("Fax", StringComparison.OrdinalIgnoreCase) >= 0) continue;
                //if (printer.IndexOf("PDF", StringComparison.OrdinalIgnoreCase) >= 0) continue;
                //if (printer.IndexOf("P o s", StringComparison.OrdinalIgnoreCase) >= 0) continue;
                PrinterComboBox.Items.Add(printer);
            }

            // Try to select saved printer from GlobalStateService
            string savedPrinter = GlobalStateService.Instance.PrinterName;

            if (!string.IsNullOrWhiteSpace(savedPrinter) && PrinterComboBox.Items.Contains(savedPrinter))
            {
                PrinterComboBox.SelectedItem = savedPrinter;
            }
            else if (PrinterComboBox.Items.Count > 0)
            {
                // Fallback: select first available printer
                PrinterComboBox.SelectedIndex = 0;
                GlobalStateService.Instance.PrinterName = PrinterComboBox.SelectedItem.ToString();
                GlobalStateService.Instance.SaveSettings();
            }
        }

        private void OnTestPrinterClicked(object sender, RoutedEventArgs e)
        {
            string printerName = PrinterComboBox.SelectedItem as string;
            PrinterComboBox.IsDropDownOpen = false;
            if (string.IsNullOrWhiteSpace(printerName) || printerName.Length == 0)
            {
                MessageBox.Show("Please select a valid printer.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                string currentDate = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");

                // Prepare HTML content with dynamic printer name and date
                string htmlContent = @"
                    <!DOCTYPE html>
                    <html lang=""en"">
                    <head>
                        <meta charset=""UTF-8"">
                        <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                        <title>Thermal Printer Test</title>
                        <style>
                            body {
                                font-family: ""Courier New"", monospace;
                                margin: 0;
                                padding: 10px;
                                width: 100%;
                                height: 100%;
                            }
                            .test-page {
                                text-align: center;
                                padding: 10px;
                                border: 1px solid #000;
                                width: 90%;
                                margin: auto;
                            }
                            .barcode {
                                margin-top: 20px;
                                font-size: 30px;
                            }
                            .footer {
                                margin-top: 30px;
                                font-size: 12px;
                            }
                        </style>
                    </head>
                    <body>
                        <div class=""test-page"">
                            <h1>PRINTER TEST OK</h1>
                            <p><strong>Printer Name:</strong> " + printerName + @"</p> <br/>
                            <p><strong>Date:</strong> " + currentDate + @"</p><br/>
                            <p><strong>Test Type:</strong> Print Test Page</p><br/>
                            <hr>
                       
                          
                        </div>
                    </body>
                    </html>";

                var printerService = new PrinterService();
                AsyncPrintTask.Exec(
                    true,
                    () => printerService.DoPrint(htmlContent, printerName, 46)
                );
                MessageBox.Show($"Printer test for '{printerName}' initiated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error printing demo page: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void OnSaveClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(printerName))


                {

                    if (PrinterComboBox.Items.Count == 0)
                    {
                        MessageBox.Show("Not found any installed printers . Please contact admin ", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        GlobalStateService.Instance.PrinterName = "";
                    return;
                    }

                   
                }

                GlobalStateService.Instance.SaveSettings();

                if (GlobalStateService.Instance.Auth == "configuration")
                {

                    GlobalStateService.Instance.PrinterName = PrinterComboBox.SelectedItem as string; 
                    GlobalStateService.Instance.IsLoggedIn = false;
                    GlobalStateService.Instance.Username = "";
                    GlobalStateService.Instance.UserId = "";
                    GlobalStateService.Instance.Auth = "";
                    GlobalStateService.Instance.DeveIp = localIP;
                    GlobalStateService.Instance.SaveSettings();
                    LoginWindow login = new LoginWindow();
                    login.Show();
                    Window.GetWindow(this)?.Close();

                     MessageBox.Show("Configuration set successfully! Please login as a user .", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    GlobalStateService.Instance.PrinterName = PrinterComboBox.SelectedItem as string;
                    GlobalStateService.Instance.IsLoggedIn = false;
                    GlobalStateService.Instance.Username = "";
                    GlobalStateService.Instance.UserId = "";
                    GlobalStateService.Instance.Auth = "";
                    GlobalStateService.Instance.DeveIp = localIP;
                    GlobalStateService.Instance.SaveSettings();
                    MessageBox.Show("Configuration saved successfully!", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoginWindow login = new LoginWindow();
                    login.Show();
                    Window.GetWindow(this)?.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save configuration: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}