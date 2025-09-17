using Deno.Services;
using System;
using System.Windows.Controls;

namespace Deno.Views
{
    public partial class ManagePage : Page
    {
        public ManagePage()
        {
            InitializeComponent();
            InitializeWebView();
        }

        private async void InitializeWebView()
        {
            try
            {
                // Ensure WebView2 is ready
                await MyWebView.EnsureCoreWebView2Async();
                string url = GlobalStateService.Instance.DomainName;
       
                MyWebView.Source = new Uri($"http://{url}");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("WebView2 failed: " + ex.Message);
            }
        }
    }
}
