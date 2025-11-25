using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modula.Components.Pages
{
    public partial class Settings
    {
        [Inject] private IJSRuntime JS { get; set; } = default!;
        private string apiURL { get; set; } = "";
        private string mqttHost { get; set; } = "";
        private string mqttPORT { get; set; } = "";
        private string mqttUsername { get; set; } = "";
        private string mqttPassword { get; set; } = "";
        private string mqttTopic { get; set; } = "";
        public Settings()
        {
            apiURL = Preferences.Get("API_URL", "http://10.20.29.65:8088/rerpapi/api/");
            mqttHost = Preferences.Get("MQTT_HOST", "192.168.1.176");
            mqttPORT = Preferences.Get("MQTT_PORT", "61613");
            mqttUsername = Preferences.Get("MQTT_USERNAME", "admin");
            mqttPassword = Preferences.Get("MQTT_PASSWORD", "password");
            mqttTopic = Preferences.Get("MQTT_TOPIC", "mqtt/face/2491236/Rec");
        }
        private async Task OnSave()
        {
            Preferences.Set("API_URL", apiURL);
            Preferences.Set("MQTT_HOST", mqttHost);
            Preferences.Set("MQTT_PORT", mqttPORT);
            Preferences.Set("MQTT_USERNAME", mqttUsername);
            Preferences.Set("MQTT_PASSWORD", mqttPassword);
            Preferences.Set("MQTT_TOPIC", mqttTopic);
            await JS.InvokeVoidAsync("history.back");
        }
    }
}
