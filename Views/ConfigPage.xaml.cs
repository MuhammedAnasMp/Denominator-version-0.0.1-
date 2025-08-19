using Deno.Services;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Deno.Views
{
    public partial class ConfigPage : UserControl
    {
        public ConfigPage()
        {
            InitializeComponent();

            // Populate currency picker
            CurrencyPicker.ItemsSource = new[] { "AED", "KWD", "USD" };

            // Set selected item based on loaded CurrencyCode
            CurrencyPicker.SelectedItem = GlobalStateService.Instance.CurrencyCode ?? "AED";

            // Log initial values for debugging
            Console.WriteLine($"ConfigPage initialized: LocCode={GlobalStateService.Instance.LocCode}, " +
                              $"PosNumber={GlobalStateService.Instance.PosNumber}, " +
                              $"DomainName={GlobalStateService.Instance.DomainName}, " +
                              $"CurrencyCode={GlobalStateService.Instance.CurrencyCode}");
        }

        private void CurrencyPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CurrencyPicker.SelectedItem != null)
            {
                GlobalStateService.Instance.CurrencyCode = CurrencyPicker.SelectedItem.ToString();
                Console.WriteLine($"CurrencyPicker changed: CurrencyCode={GlobalStateService.Instance.CurrencyCode}");
            }
        }

        private void OnSaveClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                // Log values before saving
                Console.WriteLine($"Saving: LocCode={GlobalStateService.Instance.LocCode}, " +
                                  $"PosNumber={GlobalStateService.Instance.PosNumber}, " +
                                  $"DomainName={GlobalStateService.Instance.DomainName}, " +
                                  $"CurrencyCode={GlobalStateService.Instance.CurrencyCode}");

                // Save settings to JSON file
                GlobalStateService.Instance.SaveSettings();
                MessageBox.Show("Configuration saved successfully!", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save configuration: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}