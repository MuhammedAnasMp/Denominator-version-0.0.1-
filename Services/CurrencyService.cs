using Deno.Models;
using Newtonsoft.Json;
using PrintHTML.Core.Helpers;
using PrintHTML.Core.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace Deno.Services
{
    public class CurrencyService : INotifyPropertyChanged
    {
        public List<CurrencyInfo> AllCurrencies { get; set; }
        private CurrencyInfo _selectedCurrency;

        public CurrencyInfo SelectedCurrency
        {
            get => _selectedCurrency;
            set
            {
                if (_selectedCurrency != value)
                {
                    _selectedCurrency = value;
                    // Update GlobalStateService and save to JSON
                    GlobalStateService.Instance.CurrencyCode = value?.CurrencyCode ?? "AED";
                    GlobalStateService.Instance.SaveSettings();
                    UpdateViewModels();
                    OnPropertyChanged(nameof(SelectedCurrency));
                    OnPropertyChanged(nameof(CoinViewModels));
                    OnPropertyChanged(nameof(NoteViewModels));
                    OnPropertyChanged(nameof(CoinTotal));
                    OnPropertyChanged(nameof(NoteTotal));
                    OnPropertyChanged(nameof(GrandTotal));
                    Console.WriteLine($"CurrencyService: SelectedCurrency changed to {value?.CurrencyCode}");
                }
            }
        }


        private int _updatingRecordId;

        public int UpdatingRecordId
        {
            get => _updatingRecordId;
            set
            {
                _updatingRecordId = value;
                OnPropertyChanged(nameof(UpdatingRecordId));
            }
        }


        private bool _updatingRecord;

        public bool UpdatingRecord
        {
            get => _updatingRecord;
            set
            {
                _updatingRecord = value;
                OnPropertyChanged(nameof(UpdatingRecord));
            }
        }



        private bool _editMod;

        public bool EditMod
        {
            get => _editMod;
            set
            {
                _editMod = value;
                OnPropertyChanged(nameof(EditMod));
            }
        }



        private int _higherUserID;

        public int HigherUserID
        {
            get => _higherUserID;
            set
            {
                _higherUserID = value;
                OnPropertyChanged(nameof(HigherUserID));
            }
        }



        private bool _isPosting = false;
        private List<CoinViewModel> _coinViewModels;
        public List<CoinViewModel> CoinViewModels
        {
            get => _coinViewModels;
            private set
            {
                _coinViewModels = value;
                OnPropertyChanged(nameof(CoinViewModels));
                OnPropertyChanged(nameof(CoinTotal));
                OnPropertyChanged(nameof(GrandTotal));
            }
        }

        private List<NoteViewModel> _noteViewModels;
        public List<NoteViewModel> NoteViewModels
        {
            get => _noteViewModels;
            private set
            {
                _noteViewModels = value;
                OnPropertyChanged(nameof(NoteViewModels));
                OnPropertyChanged(nameof(NoteTotal));
                OnPropertyChanged(nameof(GrandTotal));
            }
        }

        public decimal CoinTotal => CoinViewModels?.Sum(c => c.Total) ?? 0;
        public decimal NoteTotal => NoteViewModels?.Sum(n => n.Total) ?? 0;
        public decimal GrandTotal => CoinTotal + NoteTotal;

        public ICommand ResetCommand => new RelayCommand(Reset);
        //public ICommand PostToApiCommand => new RelayCommand(PostToApi);
        public ICommand PostToApiCommand { get; }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public CurrencyService()
        {
            AllCurrencies = new List<CurrencyInfo>
            {
                //new CurrencyInfo
                //{
                //    Country = "United Arab Emirates",
                //    CurrencyCode = "AED",
                //    CurrencyName = "UAE Dirham",
                //    CurrencySymbol = "د.إ",
                //    SubunitName = "Fils",
                //    SubunitPerUnit = 100,
                //    Coins = new List<decimal> { 0.25m, 0.5m, 1m },
                //    Notes = new List<decimal> { 5, 10, 20, 50, 100, 200, 500, 1000 }
                //},
                new CurrencyInfo
                {
                    Country = "Kuwait",
                    CurrencyCode = "KWD",
                    CurrencyName = "Kuwaiti Dinar",
                    CurrencySymbol = "د.ك",
                    SubunitName = "Fils",
                    SubunitPerUnit = 1000,
                    Coins = new List<decimal> { 0.005m, 0.01m, 0.02m, 0.05m, 0.1m },
                    Notes = new List<decimal> { 0.25m, 0.5m, 1, 5, 10, 20 }
                }
            };

            // Use GlobalStateService instead of Properties.Settings.Default
            string savedCode = GlobalStateService.Instance.CurrencyCode ?? AllCurrencies.First().CurrencyCode;
            _selectedCurrency = AllCurrencies.FirstOrDefault(c => c.CurrencyCode == savedCode) ?? AllCurrencies.First();
            UpdateViewModels();
            PostToApiCommand = new RelayCommand(PostToApi, CanPostToApi);

        }
        private bool CanPostToApi(object parameter)
        {
            return !_isPosting;
        }
        private void UpdateViewModels()
        {
            // Clear existing event subscriptions
            if (CoinViewModels != null)
            {
                foreach (var coin in CoinViewModels)
                {
                    coin.PropertyChanged -= OnCoinPropertyChanged;
                }
            }
            if (NoteViewModels != null)
            {
                foreach (var note in NoteViewModels)
                {
                    note.PropertyChanged -= OnNotePropertyChanged;
                }
            }

            // Create new view models
            CoinViewModels = SelectedCurrency?.Coins?.Select(c =>
            {
                var vm = new CoinViewModel(c, this);
                vm.PropertyChanged += OnCoinPropertyChanged;
                return vm;
            }).ToList();

            NoteViewModels = SelectedCurrency?.Notes?.Select(n =>
            {
                var vm = new NoteViewModel(n, this);
                vm.PropertyChanged += OnNotePropertyChanged;
                return vm;
            }).ToList();
        }
        private void OnNotePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(NoteViewModel.Quantity) ||
                e.PropertyName == nameof(NoteViewModel.Total))
            {
                OnPropertyChanged(nameof(NoteTotal));
                OnPropertyChanged(nameof(GrandTotal));
            }
        }

        private void OnCoinPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CoinViewModel.Quantity) ||
                e.PropertyName == nameof(CoinViewModel.Total))
            {
                OnPropertyChanged(nameof(CoinTotal));
                OnPropertyChanged(nameof(GrandTotal));
            }
        }


        private void Reset(object parameter)
        {
            if (EditMod)
            {
                //MessageBox.Show("Unlock is required");
                return;
            }
            foreach (var coin in CoinViewModels)
                coin.Quantity = 0;
            foreach (var note in NoteViewModels)
                note.Quantity = 0;
            OnPropertyChanged(nameof(CoinViewModels));
            OnPropertyChanged(nameof(NoteViewModels));
            OnPropertyChanged(nameof(CoinTotal));
            OnPropertyChanged(nameof(NoteTotal));
            OnPropertyChanged(nameof(GrandTotal));
        }

        private async void PrintReceipt(int createdId ,bool reset = true)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var host = GlobalStateService.Instance.DomainName;

                    var UserId = GlobalStateService.Instance.UserId;
                    var storeId = GlobalStateService.Instance.LocCode; 
                    var response = await client.GetAsync($"http://{host}/api/denominations?Id={createdId}&StoreId={storeId}&UserId={UserId}");

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();

                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var result = System.Text.Json.JsonSerializer.Deserialize<GetResponse>(responseContent, options);
                        if (response.IsSuccessStatusCode)
                        {
                            // Convert the data to a readable string
                            if (result.Data != null && result.Data.Any())
                            {
                                var data = result.Data.First();
                                var allTransactions  =result.Transaction;

                                string FormatAmount(double amt)
                                {
                                    double truncated = Math.Truncate(amt * 1000) / 1000;
                                    return truncated.ToString("0.000", CultureInfo.InvariantCulture);
                                }
                                string StoreName = GlobalStateService.Instance.LocName;
                                string PrinterName = GlobalStateService.Instance.PrinterName;
                                string htmlContent = $@"
                                    <!DOCTYPE html>
                                    <html lang='en'>
                                    <head>
                                        <meta charset='UTF-8'>
                                        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                                        <title>Denomination Receipt - {StoreName}</title>
                                        <style>
                                            body {{ 
                                                font-family: 'Arial', sans-serif; 
                                                margin: 0;
                                                padding: 10px;

                                            }}

                                            .receipt-container {{
                                                margin: 0 auto;
                                                background: white;
                                             
                                            }}
                                            .header {{
                                                text-align: center;
                                                margin-bottom: 15px;
                                                padding-bottom: 10px;
                                                border-bottom: 1px solid #007bff;
                                            }}
                                            .store-name {{
                                                color: #007bff;
                                                margin: 0 0 5px 0;
                                                font-size: 20px;
                                                font-weight: bold;
                                            }}
                                            .info-row {{
                                                display: flex;
                                                justify-content: center;
                                                gap: 10px;
                                                margin: 5px 0;
                                                flex-wrap: wrap;
                                                font-size: 12px;
                                            }}
                                            .info-item {{
                                                background: #e9ecef;
                                                padding: 5px 10px;
                                                border-radius: 15px;
                                            }}
                                            h3 {{
                                                color: #495057;
                                                margin: 5px 0 5px 0;
                                                text-align: center;
                                                font-size: 14px;
                                            }}
                                            table {{
                                                width: 60%;
                                                margin: 0px auto;
                                                border-collapse: collapse;
                                                font-size: 12px;
                                                        }}
                                            .th, td {{   
                                                text-align: center;
                                            }}
                                            .summery-text-alignment{{
                                                      text-align: left;
                                                       font-weight: normal;}}

                                            .supervisor-name {{font - size: 8px;
                                                                max-width: 100px;
                                                                overflow: hidden;
                                                                text-overflow: ellipsis;
                                                                white-space: nowrap;
                                                                display: inline-block;
                                                                vertical-align: middle;
                                                            }}
                                            }}
                                            .th {{
                                                 text-align: center;
                                                    font-weight: bold;
                                                    border-bottom: 1px solid; 
                                                    background-color:yellow ;

                                                
                                            }}
                                            .total-row td {{
                                                background: #e3f2fd;
                                                text-align: center;
                                                

                                            }}
                                            .footer {{
                                                text-align: center;
                                                border-top: 1px solid #007bff;
                                                color: #6c757d;
                                                font-size: 12px;
                                            }}
                                            .grand-total {{
                                                font-weight: bold;
                                                font-size: 15px;
                                                
                                            }}
                                            .currency-symbol::before {{
                                                content: 'Rp ';
                                            }}
                   
                                          
                                        </style>
                                    </head>
                                    <body>
                                    <div class='receipt-container'>
                                        <div class='header'>
                                            <h1 class='store-name'>{StoreName}</h1>
                                            <div class='info-row'>
                                                <span class='info-item'>Date: <strong>{data.CreatedDt:dd-MMM-yy}</strong></span>
                                                <span class='info-item'>ID: <strong>{data.CreatedById}</strong></span>
                                                <br/>
                                                <span class='info-item'>Name: <strong>{data.CreatedByName}</strong></span>
                                            </div>
                                        </div>
                                        <div>
                                        <h3>Currency Denominations</h3>
                                        <table>
                       

                                            <tr>
                                                <th class='th'>Currency</th>
                                                <th class='th'>Qty</th>
                                                <th class='th'>Total</th>
                                            </tr>
                                             
                                            {$"<tr><td>20.000</td><td>{data.Kd20}</td><td class='currency-symbol'>{FormatAmount(data.Kd20 * 20)}</td></tr>"}

                                                   {$"<tr><td>10.000</td><td>{data.Kd10}</td><td class='currency-symbol'>{FormatAmount(data.Kd10 * 10)}</td></tr>"}
                                                   {$"<tr><td>5.000</td><td>{data.Kd5}</td><td class='currency-symbol'>{FormatAmount(data.Kd5 * 5)}</td></tr>"}
                                                   {$"<tr><td>1.000</td><td>{data.Kd1}</td><td class='currency-symbol'>{FormatAmount(data.Kd1 * 1)}</td></tr>"}
                                                   {$"<tr><td>0.500</td><td>{data.Kd05}</td><td class='currency-symbol'>{FormatAmount(data.Kd05 * 0.5)}</td></tr>"}
                                                   {$"<tr><td>0.250</td><td>{data.Kd025}</td><td class='currency-symbol'>{FormatAmount(data.Kd025 * 0.25)}</td></tr>"}
                                        </table>
                                           <div/>
                                            <div class='total-row'>
                                                <div colspan='3' style='text-align:center;'>
                                                    <strong>Total Currency: <span class='currency-symbol'>{FormatAmount(data.NoteTotal)}</span></strong>
                                                </div>
                                            </div>
                                        <div>
                                        <h3>Coins Denominations</h3>
                                        <table>
                                            <tr>
                                                <th class='th'>Coins</th>
                                                <th class='th'>Qty</th>
                                                <th class='th'>Total</th>
                                            </tr>
                                            {$"<tr><td>0.100</td><td>{data.Kd01}</td><td class='currency-symbol'>{FormatAmount(data.Kd01 * 0.1)}</td></tr>"}
                                            {$"<tr><td>0.050</td><td>{data.Kd005}</td><td class='currency-symbol'>{FormatAmount(data.Kd005 * 0.05)}</td></tr>"}
                                            {$"<tr><td>0.020</td><td>{data.Kd002}</td><td class='currency-symbol'>{FormatAmount(data.Kd002 * 0.02)}</td></tr>"}
                                            {$"<tr><td>0.010</td><td>{data.Kd001}</td><td class='currency-symbol'>{FormatAmount(data.Kd001 * 0.01)}</td></tr>"}
                                            {$"<tr><td>0.005</td><td>{data.Kd0005}</td><td class='currency-symbol'>{FormatAmount(data.Kd0005 * 0.005)}</td></tr>"}
                                        </table>
                                        </div>
                                            <div class='total-row'>
                                                <div style='text-align:center;'>
                                                    <strong>Total Coins: <span class='currency-symbol'>{FormatAmount(data.CoinTotal)}</span></strong>
                                                </div>
                                            </div>


                                      <div class='footer'>
                                            <p><span class='grand-total'> Grand Total : {data.GrandTotal} KD </span> </p> <br/>  <br/>
                                         
                                        </div>
                                        <div>

                                        <h3>Supervisor Bill Summary</h3>
                                                  <table>
                                                    <tr>
                                                      <th class='th'>Supervisor</th>
                                                      <th class='th'>Bills</th>
                                                      <th class='th'>Value</th>
                                                    </tr>
                                                    {string.Join("", allTransactions.SelectMany(kvp =>
                                                        {
                                                            {
                                                         var sectionHeader = $@"
                                                        <tr class='section-header'>
                                                          <td>----------------</td>
                                                          <td>{kvp.Key}</td>
                                                          <td>----------------</td>
                                                        </tr>";

                                                          var rows = string.Join("", kvp.Value.Select(entry => $@"
                                                          <tr>
                                                           <td style='font-size:10px; text-align:left;'><span title='{entry.NAME.ToString() ?? ""}'>{entry.NAME.ToString() ?? ""}</span></td>

                                                            <td>{entry.BILL_COUNT.ToString() ?? ""}  </td>
                                                            <td>{entry.VALUE?.ToString("F2") ?? "0.00"}</td>
                                                          </tr>
                                                        "));

                                                            return sectionHeader + rows;
                                                        }
                                                    }))}
                                                  </table>
                                            <div/>

                                            <table style=""width: 100%; font-size: 12px;"">
                                            <tr>
                                                <td style=""text-align: left;"">
                                                    <strong>Supervisor Sign</strong><br/><br/>
                                                
                                                </td>
                                                <td style=""text-align: right;"">
                                                    <strong>Cash Officer Sign</strong><br/><br/>
                                                   
                                                </td>
                                            </tr>
                                        </table>
                                            
                                            <div class='footer'>
                                            <p>Generated on: {DateTime.Now:dd-MMM-yyyy HH:mm}</p>
                                                </div>
                                             </div>

                                    </body>
                                    </html>";

                             
                                var printerService = new PrinterService();
                                try
                                {
                                    AsyncPrintTask.Exec(
                                        true,
                                        () =>
                                        {
                                            try
                                            {
                                                printerService.DoPrint(htmlContent, PrinterName, 46);
                                            }
                                            catch (Exception ex)
                                            {

                                                MessageBox.Show(
                                                    $"Check if the printer name is in the correct format and uses capital letters appropriately.\r\n" +
                                                    $"Error Details: {ex.Message}",
                                                    "Error: Printer Not Found",
                                                    MessageBoxButton.OK,
                                                    MessageBoxImage.Error
                                                );

                                            }
                                        }
                                    );
                                }
                                catch (PrinterNotFoundException ex)
                                {
                                    MessageBox.Show(
                                                        $"Check if the printer name is in the correct format and uses capital letters appropriately.\r\n" +
                                                        $"",
                                                        ex.Message,
                                                        MessageBoxButton.OK,
                                                        MessageBoxImage.Error
                                                    );
                                }
                                //validate this 
                                if (reset)
                                {

                                Reset(null);
                                }
                               
                            } 
                        else
                        {
                            MessageBox.Show("No data found!", "Receipt Data");
                        }
                        }
                        else
                        {
                            MessageBox.Show($"Error: {result?.Message}", "API Response");
                        }
                    }
                    else
                    {
                        MessageBox.Show($"HTTP Error: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }




        private async void PostToApi(object parameter)
        {
            




         
            try
            {
                if (EditMod)
                {
                    var authDialog = new AuthDialog();
                    if (authDialog.ShowDialog() != true)
                    {
                        //MessageBox.Show("Authentication cancelled.");
                        return;
                    }

                    string password = authDialog.Password;


                    var loginSuccess = await ValidateManagerCredentials(authDialog.Id, password);

                    if (loginSuccess == null || loginSuccess.Status != 200)
                    {
                        MessageBox.Show("Authentication failed");
                        return;
                    }

                    if (loginSuccess.Auth == "CASHIER")
                    {
                        MessageBox.Show("Supervisor ID is required to update denomination");
                        return;
                    }

                    if (loginSuccess.Auth == "INVALID")
                    {
                        MessageBox.Show("Invalid login credentials");
                        return;
                    }
                    HigherUserID = loginSuccess.Id;
                    EditMod = false;
                    return;
                }


                if (_isPosting)
                    return;
                _isPosting = true;
                CommandManager.InvalidateRequerySuggested(); // disables the button

                

                if (parameter is int updatingRecordId && updatingRecordId != 0)
                {

               
                    



                    //if updatingRecordId is avaialble then sent the existing data with flag to post denomination data function 
                    // dont reset the existing quantitys because method is updating 


                    int? updatedId = await PostDenominationData(updatingRecordId , HigherUserID);
                    if (updatedId.HasValue)
                    {

                        PrintReceipt((int)updatedId , false);

                    }


                }

                else
                {

                    int? createdId = await PostDenominationData();
                    if (createdId.HasValue)
                    {

                        PrintReceipt((int)createdId);

                    }

                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error posting data: {ex.Message}");
                Console.WriteLine($"Error in PostToApi: {ex}");
            }
            finally
            {
                _isPosting = false;
                CommandManager.InvalidateRequerySuggested(); // re-enables the button
            }
        }

        private async Task<CashierLoginResponse> ValidateManagerCredentials(string cashier_id, string password)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var authData = new
                    {
                        cashier_id = int.Parse(cashier_id),
                        password = int.Parse(password)
                    };

                    var json = System.Text.Json.JsonSerializer.Serialize(authData);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var host = GlobalStateService.Instance.DomainName;
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
                    catch (System.Text.Json.JsonException)
                    {
                        ShowError("Failed to parse server response.");
                        return new CashierLoginResponse { Status = 500, Message = "Response parsing error" };
                    }
                    
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error : {ex}");
                return new CashierLoginResponse { Status = 500, Message = $"Invalid server response {ex}" };
            }
        }
        private void ShowError(string message)
        {
            System.Windows.MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }


        private async Task<int?> PostDenominationData(int? recordId = null , int? authId = null)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var postData = new
                    {
                        CurrencyCode = SelectedCurrency?.CurrencyCode,
                        Coins = CoinViewModels?.Select(c => new
                        {
                            Denomination = c.Denomination.ToString(),
                            Quantity = c.Quantity,
                            Total = c.Total
                        }).ToList(),
                        Notes = NoteViewModels?.Select(n => new
                        {
                            Denomination = n.Denomination.ToString(),
                            Quantity = n.Quantity,
                            Total = n.Total
                        }).ToList(),
                        CoinTotal = CoinTotal.ToString("F3"),
                        NoteTotal = NoteTotal.ToString("F3"),
                        GrandTotal = GrandTotal.ToString("F3"),
                        Timestamp = DateTime.UtcNow,
                        PosNumber = GlobalStateService.Instance.PosNumber,
                        LocCode = GlobalStateService.Instance.LocCode,
                        UserId = GlobalStateService.Instance.UserId,
                        UserName = GlobalStateService.Instance.Username,


                        RecordId = recordId.HasValue ? recordId.Value : (int?)null,
                        AuthId = authId.HasValue ? authId.Value : (int?)null
                
                    };
                      
                    if (GrandTotal == 0)
                    {
                        MessageBox.Show("Please enter the quantity for each coin and note.",
                                        "Grand Total Cannot Be Zero",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Warning);
                        return null;
                    }

                    var json = System.Text.Json.JsonSerializer.Serialize(postData, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var host = GlobalStateService.Instance.DomainName;

                    var response = await client.PostAsync($"http://{host}/api/denominations", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = await response.Content.ReadAsStringAsync();
                        var responseData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(responseJson);

                        if (responseData != null && responseData.TryGetValue("id", out object idObj))
                        {
                            if (int.TryParse(idObj?.ToString(), out int id))
                            {
                       
                                return id;
                            }
                            else
                            {
                                // "id" exists but is not a valid integer
                                MessageBox.Show("Data posted but ID is not a valid number. Please contact admin");
                                return null;
                            }
                        }
                        else
                        {
                            // "id" key does not exist in the response
                            MessageBox.Show("Data posted but no ID returned from API. Please contact admin");
                            return null;
                        }

                    }
                    else
                    {
                        MessageBox.Show($"Failed to post data. Status code: {response.StatusCode}");
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error posting data: {ex.Message}");
                return null;
            }
        }

        public class GetResponse
        {
            [JsonPropertyName("status")]
            public int Status { get; set; }

            [JsonPropertyName("message")]
            public string Message { get; set; }

            [JsonPropertyName("id")]
            public int Id { get; set; }

            [JsonPropertyName("success")]
            public bool Success { get; set; }

            [JsonPropertyName("data")]
            public List<DenominationData> Data { get; set; }

            [JsonPropertyName("transaction_report")]
            public   Dictionary<string, List<TransactionEntry>> Transaction { get; set; }

            [JsonPropertyName("counted_by")]
            public string CountedBy { get; set; }
        }
        public class TransactionEntry
        {
            public string NAME { get; set; }
            public int BILL_COUNT { get; set; }
            public decimal? VALUE { get; set; }
        }

        public class DenominationData
        {
            [JsonPropertyName("kd_0005")]
            public int Kd0005 { get; set; }

            [JsonPropertyName("kd_001")]
            public int Kd001 { get; set; }

            [JsonPropertyName("kd_002")]
            public int Kd002 { get; set; }

            [JsonPropertyName("kd_005")]
            public int Kd005 { get; set; }

            [JsonPropertyName("kd_01")]
            public int Kd01 { get; set; }

            [JsonPropertyName("kd_025")]
            public int Kd025 { get; set; }

            [JsonPropertyName("kd_05")]
            public int Kd05 { get; set; }

            [JsonPropertyName("kd_1")]
            public int Kd1 { get; set; }

            [JsonPropertyName("kd_5")]
            public int Kd5 { get; set; }

            [JsonPropertyName("kd_10")]
            public int Kd10 { get; set; }

            [JsonPropertyName("kd_20")]
            public int Kd20 { get; set; }

            [JsonPropertyName("coin_total")]
            public double CoinTotal { get; set; }

            [JsonPropertyName("note_total")]
            public double NoteTotal { get; set; }

            [JsonPropertyName("grand_total")]
            public double GrandTotal { get; set; }

            [JsonPropertyName("created_dt")]
            public DateTime CreatedDt { get; set; }

            [JsonPropertyName("pos_number")]
            public string PosNumber { get; set; }

            [JsonPropertyName("loc_code")]
            public string LocCode { get; set; }

            [JsonPropertyName("created_by_name")]
            public string CreatedByName { get; set; }

            [JsonPropertyName("created_by_id")]
            public int CreatedById { get; set; }

            [JsonPropertyName("id")]
            public int Id { get; set; }

            [JsonPropertyName("existing_record_id")]
            public int RecordId { get; set; }

            [JsonPropertyName("approved_by")]
            public int AutId { get; set; }
        }


        public class AuthResponse
        {
            [JsonPropertyName("message")]
            public string Message { get; set; }

            [JsonPropertyName("success")]
            public bool Success { get; set; }

            [JsonPropertyName("status")]
            public int Status { get; set; }

            [JsonPropertyName("counted_by")]
            public string CountedBy { get; set; }

            //[JsonPropertyName("data")]
            //public string Message { get; set; }

        }
    }

    public class CoinViewModel : INotifyPropertyChanged
    {
        private int _quantity;
        public decimal Denomination { get; set; }
        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged(nameof(Quantity));
                OnPropertyChanged(nameof(Total));
            }
        }
        public decimal Total => Denomination * Quantity;

        public CoinViewModel(decimal denom, CurrencyService currencyService) { Denomination = denom; Quantity = 0; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class NoteViewModel : INotifyPropertyChanged
    {
        private int _quantity;
        public decimal Denomination { get; set; }
        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged(nameof(Quantity));
                OnPropertyChanged(nameof(Total));
            }
        }
        public decimal Total => Denomination * Quantity;

        public NoteViewModel(decimal denom, CurrencyService currencyService) { Denomination = denom; Quantity = 0; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class CurrencyInfo
    {
        public string Country { get; set; }
        public string CurrencyCode { get; set; }
        public string CurrencyName { get; set; }
        public string CurrencySymbol { get; set; }
        public string SubunitName { get; set; }
        public int SubunitPerUnit { get; set; }
        public List<decimal> Coins { get; set; }
        public List<decimal> Notes { get; set; }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);
        public void Execute(object parameter) => _execute(parameter);
     
    }
    }