using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Modula.Models;
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
        [Inject] private IApiService apiService { get; set; } = default!;
        [Inject] private IAlertService alertService { get; set; } = default!;
        private string username { get; set; } = "";
        private string password { get; set; } = "";
        private ElementReference usernameInput;
        private ElementReference passwordInput;

        protected override Task OnAfterRenderAsync(bool firstRender)
        {
            Preferences.Set("Token", "");
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
                var response = await apiService.Client.PostAsync($"api/home/login", jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception();
                }

                var json = await response.Content.ReadAsStringAsync();
                var accountInfo = JsonConvert.DeserializeObject<LogInInfo>(json);
                Preferences.Set("Token", accountInfo!.access_token);
                await JS.InvokeVoidAsync("toggleLoading", false);
                Nav.NavigateTo($"/");
            }
            catch (InvalidDataException ex)
            {
                await JS.InvokeVoidAsync("toggleLoading", false);
                await alertService.ShowAsync("Thông báo", ex.Message, "OK");
            }
            catch
            {
                await JS.InvokeVoidAsync("toggleLoading", false);
                await alertService.ShowAsync("Thông báo", "Tên đăng nhập hoặc mật khẩu không chính xác.", "OK");
            }
        }
        public void OnSettingClicked()
        {
            Nav.NavigateTo($"/settings");
        }
    }
}
