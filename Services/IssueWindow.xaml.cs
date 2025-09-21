using System;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace Deno.Services
{
    public partial class IssueWindow : Window
    {
        private readonly HttpClient _httpClient;

        public IssueWindow()
        {
            InitializeComponent();
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://your-api-base-url.com/") // Replace with your API base URL

            };
            HideMinimizeAndMaximize();
            cmbCategory.SelectionChanged += CmbCategory_SelectionChanged;
            // Set initial placeholder text based on default selection
            UpdatePlaceholderText();

        }

        private void CmbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdatePlaceholderText();
        }

        private void UpdatePlaceholderText()
        {
            if (cmbCategory.SelectedItem is ComboBoxItem selectedItem)
            {
                string category = selectedItem.Content.ToString();
                switch (category)
                {
                    case "Feature":
                        SetPlaceholderText("Describe the new feature you would like to suggest...");
                        break;
                    case "Requirement":
                        SetPlaceholderText("Detail the requirement or specification needed...");
                        break;
                    case "Bug":
                        SetPlaceholderText("Describe the bug...");
                        break;
                    case "Comment":
                        SetPlaceholderText("Enter your general comments or feedback...");
                        break;
                    default:
                        SetPlaceholderText("Enter your casual message here...");
                        break;
                }
            }
        }

        private void SetPlaceholderText(string placeholderText)
        {
            // Check if TextBox is empty or contains only the placeholder text
            if (string.IsNullOrWhiteSpace(txtMessage.Text) || txtMessage.Tag?.ToString() == "Placeholder")
            {
                txtMessage.Text = placeholderText;
                txtMessage.Foreground = System.Windows.Media.Brushes.Gray; // Placeholder color
                txtMessage.Tag = "Placeholder"; // Mark as placeholder
            }
        }

        // Optional: Handle TextBox focus to clear placeholder
        private void TxtMessage_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtMessage.Tag?.ToString() == "Placeholder")
            {
                txtMessage.Text = "";
                txtMessage.Foreground = System.Windows.Media.Brushes.Black; // Normal text color
                txtMessage.Tag = null;
            }
        }

        // Optional: Handle TextBox losing focus to restore placeholder if empty
        private void TxtMessage_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMessage.Text))
            {
                UpdatePlaceholderText();
            }
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void HideMinimizeAndMaximize()
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            int currentStyle = GetWindowLong(hwnd, GWL_STYLE);
            // Remove minimize and maximize
            SetWindowLong(hwnd, GWL_STYLE, currentStyle & ~WS_MAXIMIZEBOX & ~WS_MINIMIZEBOX);
        }

        private const int GWL_STYLE = -16;
        private const int WS_MAXIMIZEBOX = 0x10000;
        private const int WS_MINIMIZEBOX = 0x20000;

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        
        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            cmbCategory.SelectedIndex = 0; // Reset to first item ("Feature")
            txtMessage.Clear();
        }

        private async void Send_Click(object sender, RoutedEventArgs e)

        {
            if (txtMessage.Tag?.ToString() == "Placeholder")
            {
                txtMessage.Text = "";
                txtMessage.Foreground = System.Windows.Media.Brushes.Black; // Normal text color
                txtMessage.Tag = null;
            }

            if (cmbCategory.SelectedItem == null || string.IsNullOrWhiteSpace(txtMessage.Text))
            {
                MessageBox.Show("Please select a category and enter a message.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string category = (cmbCategory.SelectedItem as ComboBoxItem).Content.ToString();
            string message = txtMessage.Text;

            try
            {
                var issueData = new
                {
                    Type = category,
                    Message = message,
                    LocCode = GlobalStateService.Instance.LocCode,
                    UserName = GlobalStateService.Instance.Username,
                    UserId = GlobalStateService.Instance.UserId,
                    Datetime = DateTime.Now,
                    PosNumber = GlobalStateService.Instance.PosNumber,
                    DevIp = GlobalStateService.Instance.DeveIp
                };
                string host = GlobalStateService.Instance.DomainName;
                var json = JsonSerializer.Serialize(issueData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"http://{host}/helpus", content);

                if (response.IsSuccessStatusCode)
                {
                    string username = GlobalStateService.Instance.Username;
                    string successMessage;

                    switch (category)
                    {
                        case "Feature":
                            successMessage = $"Thank you, {username}! Your feature suggestion has been submitted. We'll review it soon.";
                            break;
                        case "Requirement":
                            successMessage = $"Got it, {username}. Your requirement has been recorded and will be considered in our planning.";
                            break;
                        case "Bug":
                            successMessage = $"Thanks, {username}. We've logged your bug report and our team will work on fixing it.";
                            break;
                        case "Comment":
                            successMessage = $"Thank you for your feedback, {username}! We appreciate your comments.";
                            break;
                        default:
                            successMessage = $"Your issue has been submitted, {username}. We'll work on resolving it.";
                            break;
                    }

                    MessageBox.Show(successMessage, "Submitted", MessageBoxButton.OK, MessageBoxImage.Information);

                    cmbCategory.SelectedIndex = 0;
                    txtMessage.Clear();
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Failed to send issue: {response.StatusCode} - {error}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
