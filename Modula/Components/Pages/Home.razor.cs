using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Modula.Models;
using Modula.Models.DTO;
using Modula.Services;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Modula.Components.Pages
{
    public partial class Home : IAsyncDisposable
    {
        [Inject] private NavigationManager Nav { get; set; } = default!;
        [Inject] private IJSRuntime JS { get; set; } = default!;
        [Inject] private IApiService _apiService { get; set; } = default!;
        [Inject] private IAlertService _alertService { get; set; } = default!;
        [Inject] private MQTTService _mqttService { get; set; } = default!;
        private DotNetObjectReference<Home>? _dotNetRef;
        private bool IsSelectAll;
        private BorrowTicket? FocusedRow { get; set; }
        public List<BorrowTicket> SelectedRows => BorrowTickets.Where(p => p.IsSelected).ToList();
        public List<BorrowTicket> BorrowTickets { get; set; } = [];
        private int SelectedCount => BorrowTickets?.Count(x => x.IsSelected) ?? 0;
        private int SelectedMuonCount => SelectedMuonItems.Count();
        private int SelectedTraCount => SelectedTraItems.Count();

        private IEnumerable<BorrowTicket> SelectedMuonItems =>
        BorrowTickets?.Where(x => x.IsSelected && x.IsBorrow) ?? [];

        private IEnumerable<BorrowTicket> SelectedTraItems =>
            BorrowTickets?.Where(x => x.IsSelected && !x.IsBorrow) ?? [];

        public string currentUserEmployeeCode = "";
        public string currentUserFullname = "";

        public Home()
        {
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                _dotNetRef = DotNetObjectReference.Create(this);
                await JS.InvokeVoidAsync("initIdleLogout", _dotNetRef);
            }
        }

        protected override async Task OnInitializedAsync()
        {
            var token = _apiService.GetAccessToken();
            var handler = new JwtSecurityTokenHandler();
            var decoded = handler.ReadJwtToken(token);
            currentUserEmployeeCode = decoded.Claims
                .FirstOrDefault(c => c.Type == "loginname")
                ?.Value ?? "";
            currentUserFullname = decoded.Claims
                .FirstOrDefault(c => c.Type == "fullname")
                ?.Value ?? "";
            await LoadData();
        }

        private void OnRowClick(BorrowTicket item)
        {
            FocusedRow = item;
        }

        private void ToggleSelectAll(ChangeEventArgs e)
        {
            bool check = (bool)(e.Value ?? false);

            foreach (var p in BorrowTickets)
                p.IsSelected = check;

            FocusedRow = null;
        }

        private void ToggleSingle(BorrowTicket item, ChangeEventArgs e)
        {
            item.IsSelected = (bool)(e.Value ?? false);
            IsSelectAll = SelectedRows.Count >= BorrowTickets.Count;
        }

        private async Task LoadData()
        {
            try
            {
                await JS.InvokeVoidAsync("toggleLoading", true);
                var response = await _apiService.Client.GetAsync("historyproductrtc/get-all");
                var json = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    var error = JsonConvert.DeserializeObject<GenericAPIResponse<ModulaHistories>>(json);
                    throw new Exception(
                        string.IsNullOrEmpty(error?.message) ?
                        "Load dữ liệu không thành công, vui lòng thử lại" : error?.message);
                }
                var modulaHistories = JsonConvert.DeserializeObject<ModulaHistories>(json);
                var borrows = modulaHistories!.data.borrows;
                var returns = modulaHistories!.data.returns;
                foreach (var item in borrows) item.IsBorrow = true;
                foreach (var item in returns) item.IsBorrow = false;
                BorrowTickets = [.. borrows, .. returns];
                await JS.InvokeVoidAsync("toggleLoading", false);
            }
            catch (Exception ex)
            {
                await JS.InvokeVoidAsync("toggleLoading", false);
                await _alertService.ShowAsync("Thông báo", ex.Message, "OK");
            }
        }

        private async Task OnRefresh()
        {
            BorrowTickets = [];
            await LoadData();
        }

        private async Task OnCall()
        {
            if (FocusedRow == null)
            {
                await _alertService.ShowAsync("Thông báo", "Vui lòng chọn sản phẩm trước khi gọi khay", "OK");
                return;
            }
            try
            {
                await JS.InvokeVoidAsync("toggleLoading", true);
                var data = new TrayInfo
                {
                    Code = FocusedRow.ModulaLocationCode,
                    Name = FocusedRow.ProductCode,
                    AxisX = FocusedRow.AxisX,
                    AxisY = FocusedRow.AxisY
                };
                var jsonContent = new StringContent(
                    JsonConvert.SerializeObject(data),
                    Encoding.UTF8,
                    "application/json");
                var response = await _apiService.Client.PostAsync("modulalocation/call-modula", jsonContent);
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<TrayRespond>(json);
                if (result!.status == 0 || !response.IsSuccessStatusCode) throw new Exception(result.message);
                await JS.InvokeVoidAsync("toggleLoading", false);
            }
            catch (Exception ex)
            {
                await JS.InvokeVoidAsync("toggleLoading", false);
                await _alertService.ShowAsync("Thông báo", ex.Message, "OK");
            }
        }

        private async Task OnReturn()
        {
            try
            {
                await JS.InvokeVoidAsync("toggleLoading", false);
                var response = await _apiService.Client.GetAsync("modulalocation/return-modula");
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<TrayRespond>(json);
                if (result!.status == 0 || !response.IsSuccessStatusCode) throw new Exception(result.message);
                await JS.InvokeVoidAsync("toggleLoading", false);
            }
            catch (Exception ex)
            {
                await JS.InvokeVoidAsync("toggleLoading", false);
                await _alertService.ShowAsync("Thông báo", ex.Message, "OK");
            }
        }

        private async Task OnDone()
        {
            try
            {
                if (SelectedRows.Any(s =>
                    (s.IsBorrow && s.ProductQRCode != s.ProductQRCodeConfirm) ||
                    (!s.IsBorrow && (s.ModulaLocationDetailCode != s.ModulaLocationCodeConfirm || s.ProductQRCode != s.ProductQRCodeConfirmReturn))
                    )
                )
                {
                    await _alertService.ShowAsync("Thông báo", "Vui lòng nhập mã QR/Vị trí chính xác", "OK");
                    return;
                }
                var rows = new List<ModulaStatusUpdate>();
                if (SelectedRows != null && SelectedRows.Count > 0)
                {
                    await JS.InvokeVoidAsync("toggleLoading", true);
                    foreach (var item in SelectedRows)
                    {
                        var row = new ModulaStatusUpdate
                        {
                            ID = item.ID,
                            PeopleID = item.PeopleID,
                            StatusPerson = item.IsBorrow ? 1 : 2
                        };
                        rows.Add(row);
                    }
                    var jsonContent = new StringContent(
                        JsonConvert.SerializeObject(rows),
                        Encoding.UTF8,
                        "application/json");
                    var response = await _apiService.Client.PostAsync("historyproductrtc/save-data", jsonContent);
                    await JS.InvokeVoidAsync("showToast", "success", "Thông báo", "Lưu dữ liệu thành công", 200, 3000);
                    await JS.InvokeVoidAsync("toggleLoading", false);
                    foreach (var item in BorrowTickets)
                    {
                        item.ProductQRCodeConfirm = item.ModulaLocationCodeConfirm = "";
                    }
                }
            }
            catch (Exception ex)
            {
                await JS.InvokeVoidAsync("toggleLoading", false);
                await _alertService.ShowAsync("Thông báo", ex.Message, "OK");
            }
        }

        private async Task OnDoneAndLogOut()
        {
            await OnDone();
            await OnLogOut();
        }

        private string GetStatusBadgeClass(string statusText)
        {
            return statusText switch
            {
                "Đăng kí mượn" => "status-badge status-1",
                //"Đăng kí trả" => "status-badge status-2",
                _ => "status-badge status-2"
            };
        }

        private async Task HandleKeyDown(KeyboardEventArgs e, BorrowTicket item)
        {
            if (e.Key == "Enter" || e.Key == "NumpadEnter")
            {
                if (string.IsNullOrEmpty(item.ProductQRCodeConfirm) || item.ProductQRCodeConfirm != item.ProductQRCode)
                {
                    await JS.InvokeVoidAsync("selectAllTextBySelector",
                        $"#qr-input-{item.ProductQRCode.Replace(" ", "-")}");
                    await JS.InvokeVoidAsync("addWarningClass",
                        $"#qr-input-{item.ProductQRCode.Replace(" ", "-")}");
                    return;
                }
                int currentIndex;
                var list = SelectedMuonItems.ToList();
                currentIndex = list.IndexOf(item);

                if (currentIndex < list.Count - 1)
                {
                    await JS.InvokeVoidAsync("removeWarningClass",
                        $"#qr-input-{item.ProductQRCode.Replace(" ", "-")}");
                    var nextItem = list[currentIndex + 1];
                    var inputId = $"qr-input-{nextItem.ProductQRCode.Replace(" ", "-")}";
                    await JS.InvokeVoidAsync("setFocusById", inputId);
                }
            }
        }

        private void OpenModal()
        {

        }

        private async Task ResetInput(BorrowTicket item)
        {
            item.ProductQRCodeConfirm = "";
            StateHasChanged();
            await JS.InvokeVoidAsync("setFocusById", $"qr-input-{item.ProductQRCode.Replace(" ", "-")}");
        }

        private async Task HandleQRCodeKeyDown(KeyboardEventArgs e, BorrowTicket item)
        {
            if (e.Key == "Enter" || e.Key == "NumpadEnter")
            {
                if (string.IsNullOrEmpty(item.ProductQRCodeConfirmReturn) || item.ProductQRCodeConfirmReturn != item.ProductQRCode)
                {
                    await JS.InvokeVoidAsync("selectAllTextBySelector",
                        $"#qr-input-return-{item.ProductQRCode.Replace(" ", "-")}");
                    await JS.InvokeVoidAsync("addWarningClass",
                        $"#qr-input-return-{item.ProductQRCode.Replace(" ", "-")}");
                    return;
                }
                await JS.InvokeVoidAsync("removeWarningClass",
                    $"#qr-input-return-{item.ProductQRCode.Replace(" ", "-")}");
                var locationId = $"location-input-{item.ProductQRCode.Replace(" ", "-")}";
                await JS.InvokeVoidAsync("setFocusById", locationId);
            }
        }

        private async Task HandleLocationKeyDown(KeyboardEventArgs e, BorrowTicket item)
        {
            if (e.Key == "Enter" || e.Key == "NumpadEnter")
            {
                if (string.IsNullOrEmpty(item.ModulaLocationCodeConfirm) || item.ModulaLocationCodeConfirm != item.ModulaLocationDetailCode)
                {
                    await JS.InvokeVoidAsync("selectAllTextBySelector",
                        $"#location-input-{item.ProductQRCode.Replace(" ", "-")}");
                    await JS.InvokeVoidAsync("addWarningClass",
                        $"#location-input-{item.ProductQRCode.Replace(" ", "-")}");
                    return;
                }
                await JS.InvokeVoidAsync("removeWarningClass",
                    $"#location-input-{item.ProductQRCode.Replace(" ", "-")}");
                var list = SelectedTraItems.ToList();
                int currentIndex = list.IndexOf(item);

                if (currentIndex < list.Count - 1)
                {
                    var nextItem = list[currentIndex + 1];

                    var nextQrId = $"qr-input-return-{nextItem.ProductQRCode.Replace(" ", "-")}";
                    await JS.InvokeVoidAsync("setFocusById", nextQrId);
                }
            }
        }

        private async Task ResetQRCodeInput(BorrowTicket item)
        {
            item.ProductQRCodeConfirmReturn = "";
            StateHasChanged();
            await JS.InvokeVoidAsync("setFocusById", $"qr-input-return-{item.ProductQRCode.Replace(" ", "-")}");
        }

        private async Task ResetLocationInput(BorrowTicket item)
        {
            item.ModulaLocationCodeConfirm = "";
            StateHasChanged();
            await JS.InvokeVoidAsync("setFocusById", $"location-input-{item.ProductQRCode.Replace(" ", "-")}");
        }

        private async Task OnLogOut()
        {
            _apiService.RemoveToken();
            _mqttService.IsLoggedIn = false;
            await JS.InvokeVoidAsync("eval", "$('#selectedItemsModal').modal('hide')");
            await JS.InvokeVoidAsync("removeElement", ".offcanvas-backdrop");
            await JS.InvokeVoidAsync("history.back");
        }

        [JSInvokable]
        public async Task OnIdleLogout()
        {
            await JS.InvokeVoidAsync("showToast", "info", "Thông báo", "Tự động đăng xuất", 200, 20000);
            await OnLogOut();
        }

        public async ValueTask DisposeAsync()
        {
            await JS.InvokeVoidAsync("disposeIdleLogout");
            _dotNetRef?.Dispose();
        }
    }
}