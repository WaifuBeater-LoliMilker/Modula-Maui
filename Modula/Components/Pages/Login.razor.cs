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
        [Inject] private MQTTService _mqttService { get; set; } = default!;
        private string username { get; set; } = "";
        private string password { get; set; } = "";
        private ElementReference usernameInput;
        private ElementReference passwordInput;

        protected override async Task OnInitializedAsync()
        {
            _mqttService.IsLoggedIn = false;
            _apiService.RemoveToken();
            if(!_mqttService.IsConnected) await ConnectMQTT();
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
                    var message = error?.message ?? "";
                    throw new Exception(string.IsNullOrEmpty(message) ? "Đã có lỗi xảy ra" : message);
                }

                var accountInfo = JsonConvert.DeserializeObject<LogInInfo>(json);
                _apiService.SetAuthorizationHeader(accountInfo!.access_token);
                _mqttService.IsLoggedIn = true;
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
            _mqttService.IsLoggedIn = true; // prevent login during setting
            Nav.NavigateTo($"/settings");
        }
        //private void OnConnectedMQTT()
        //{

        //}
        private async void OnMessagePushed(RecPushMessage data)
        {
            if (!_mqttService.IsLoggedIn)
                await CallLoginAPI(data);
        }
        private async Task ConnectMQTT()
        {
            var mqttHost = Preferences.Get("MQTT_HOST", "");
            var mqttPORT = Convert.ToInt32(Preferences.Get("MQTT_PORT", ""));
            var mqttUsername = Preferences.Get("MQTT_USERNAME", "");
            var mqttPassword = Preferences.Get("MQTT_PASSWORD", "");
            var mqttTopic = Preferences.Get("MQTT_TOPIC", "");
            //_mqttService.OnConnected -= OnConnectedMQTT;
            //_mqttService.OnConnected += OnConnectedMQTT;
            _mqttService.OnRecPushReceived -= OnMessagePushed;
            _mqttService.OnRecPushReceived += OnMessagePushed;

            try
            {
                await _mqttService.ConnectAsync(mqttHost, mqttPORT, mqttUsername, mqttPassword, mqttTopic);
            }
            catch (Exception ex)
            {
                await _alertService.ShowAsync("Thông báo", ex.Message, "OK");
            }
        }
        private async Task CallLoginAPI(RecPushMessage data)
        {
            try
            {
                var payload = new
                {
                    LoginName = data.info.persionName,
                };
                var serialized = JsonConvert.SerializeObject(payload);
                var jsonContent = new StringContent(serialized,
                    Encoding.UTF8,
                    "application/json");
                if (_apiService.Client.DefaultRequestHeaders.Contains("x-api-key"))
                    _apiService.Client.DefaultRequestHeaders.Remove("x-api-key");
                _apiService.Client.DefaultRequestHeaders.Add("x-api-key", AppEnvironment.APIKey);
                await JS.InvokeVoidAsync("toggleLoading", true);
                var response = await _apiService.Client.PostAsync($"home/loginiden", jsonContent);
                await JS.InvokeVoidAsync("toggleLoading", false);
                if (!response.IsSuccessStatusCode) return;
                var json = await response.Content.ReadAsStringAsync();
                var accountInfo = JsonConvert.DeserializeObject<LogInInfo>(json);
                _apiService.SetAuthorizationHeader(accountInfo!.access_token);
                _mqttService.IsLoggedIn = true;
                Nav.NavigateTo($"/");
            }
            catch (Exception ex)
            {
                await JS.InvokeVoidAsync("toggleLoading", false);
                await _alertService.ShowAsync("Thông báo", ex.Message, "OK");
            }
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