using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Modula.Models;
using Modula.Models.DTO;
using Modula.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modula.Components.Pages
{
    public partial class Login
    {
        [Inject] private NavigationManager Nav { get; set; } = default!;
        [Inject] private IJSRuntime JS { get; set; } = default!;
        [Inject] private IApiService _apiService { get; set; } = default!;
        [Inject] private IAlertService _alertService { get; set; } = default!;
        private string username { get; set; } = "";
        private string password { get; set; } = "";
        private ElementReference usernameInput;
        private ElementReference passwordInput;

        protected override Task OnAfterRenderAsync(bool firstRender)
        {
            _apiService.RemoveToken();
            return Task.CompletedTask;
        }

        public async Task OnUsernameKeyUp(KeyboardEventArgs e)
        {
            if (e.Code == "Enter" || e.Key == "Enter" || e.Code == "NumpadEnter")
            {
                await JS.InvokeVoidAsync("setFocus", passwordInput);
            }
        }

        public async Task OnPasswordKeyUp(KeyboardEventArgs e)
        {
            if (e.Code == "Enter" || e.Key == "Enter" || e.Code == "NumpadEnter")
            {
                await OnSubmit();
            }
        }

        public async Task OnSubmit()
        {
            try
            {
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                    throw new InvalidDataException("Vui lòng nhập tên đăng nhập/mật khẩu.");
                await JS.InvokeVoidAsync("toggleLoading", true);
                var payload = new
                {
                    LoginName = username,
                    PasswordHash = password
                };
                var serialized = JsonConvert.SerializeObject(payload);
                var jsonContent = new StringContent(serialized,
                    Encoding.UTF8,
                    "application/json");
                var response = await _apiService.Client.PostAsync($"home/login", jsonContent);

                var json = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    var error = JsonConvert.DeserializeObject<GenericAPIResponse<LogInInfo>>(json);
                    throw new Exception(
                        string.IsNullOrEmpty(error?.message) ? error?.message : "Đã có lỗi xảy ra");
                }

                var accountInfo = JsonConvert.DeserializeObject<LogInInfo>(json);
                _apiService.SetAuthorizationHeader(accountInfo!.access_token);
                await JS.InvokeVoidAsync("toggleLoading", false);
                Nav.NavigateTo($"/");
            }
            catch (Exception ex)
            {
                await JS.InvokeVoidAsync("toggleLoading", false);
                await _alertService.ShowAsync("Thông báo", ex.Message, "OK");
            }
        }

        public void OnSettingClicked()
        {
            Nav.NavigateTo($"/settings");
        }
        //public async Task OnBeforeInternalNaviagetion(LocationChangingContext context)
        //{
        //    if (context.TargetLocation == "/settings" || context.TargetLocation == "/") return;
        //    var confirm = await _alertService.ShowQuestionAsync("Thông báo", "Bạn có muốn thoát ứng dụng không?", "Yes", "No");
        //    if (confirm)
        //        Environment.Exit(0);
        //    else
        //        context.PreventNavigation();
        //}
    }
}