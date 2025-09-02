using Deno.Services;
using PrintHTML.Core.Helpers;
using PrintHTML.Core.Services;
using System;
using System.Drawing.Printing;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
namespace Deno.Views
{
    public partial class ConfigPage : UserControl
    {
        private string localIP;

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

        private void OnTestPrinterClicked(object sender, RoutedEventArgs e)
        {
            // Get the printer name from the PrinterNameTextBox
            string printerName = PrinterNameTextBox.Text;
            

            if (string.IsNullOrWhiteSpace(printerName))
            {
                MessageBox.Show("Please enter a valid printer name.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                // Validate if the printer is available
                bool isPrinterValid = false;
                foreach (string installedPrinter in PrinterSettings.InstalledPrinters)
                {
                    if (installedPrinter.Equals(printerName, StringComparison.OrdinalIgnoreCase))
                    {
                        isPrinterValid = true;
                        break;
                    }
                }

                if (!isPrinterValid)
                {
                    MessageBox.Show($"Printer '{printerName}' is not installed or available.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Get current date in a suitable format (e.g., "MM/dd/yyyy HH:mm:ss")
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
                            <p><strong>Printer Model:</strong> " + printerName + @"</p>
                            <p><strong>Date:</strong> " + currentDate + @"</p>
                            <p><strong>Test Type:</strong> Print Test Page</p>
                            <hr>
                       
                          
                        </div>
                    </body>
                    </html>";

                var printerService = new PrinterService();
                AsyncPrintTask.Exec(
                    true,
                    () => printerService.DoPrint(htmlContent, PrinterNameTextBox.Text, 46)
                );
                MessageBox.Show($"Printer test for '{PrinterNameTextBox.Text}' initiated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Check if the printer name is in the correct format and uses capital letters appropriately.\r\n ", $"Error {ex}", MessageBoxButton.OK, MessageBoxImage.Error);
            }
       

        }
        private void OnSaveClicked(object sender, RoutedEventArgs e)
        {
            try
            {
              
                GlobalStateService.Instance.SaveSettings();

                if (GlobalStateService.Instance.Auth == "configuration")
                {
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
                    MessageBox.Show("Configuration saved successfully!", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
                    GlobalStateService.Instance.IsLoggedIn = false;
                    GlobalStateService.Instance.Username = "";
                    GlobalStateService.Instance.UserId = "";
                    GlobalStateService.Instance.Auth = "";
                    GlobalStateService.Instance.DeveIp = localIP;
                    GlobalStateService.Instance.SaveSettings();
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