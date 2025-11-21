using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Modula.Services
{
    public class MQTTService
    {
        private IMqttClient _client;
        private IMqttClientOptions _options;

        public event Action? OnConnected;

        public event Action<RecPushMessage>? OnRecPushReceived;

        public async Task UnsubscribeTopicAsync(string topic)
        {
            if (_client != null && _client.IsConnected)
            {
                try
                {
                    await _client.UnsubscribeAsync(topic);
                    Console.WriteLine("Unsubscribed from topic: " + topic);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error unsubscribing: " + ex.Message);
                }
            }
            else
            {
                Console.WriteLine("Cannot unsubscribe, MQTT not connected.");
            }
        }

        public async Task SubscribeTopicAsync(string topic)
        {
            if (_client != null && _client.IsConnected)
            {
                // Nếu muốn unsubscribe topic cũ trước
                // await _client.UnsubscribeAsync(oldTopic); // lưu oldTopic nếu cần

                await _client.SubscribeAsync(new MqttTopicFilterBuilder()
                    .WithTopic(topic)
                    .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                    .Build());

                Console.WriteLine("Subscribed to topic: " + topic);
            }
            else
            {
                Console.WriteLine("Cannot subscribe, MQTT not connected.");
            }
        }

        public async void ConnectAsync(string host, int port, string username, string password, string topic)
        {
            var factory = new MqttFactory();
            _client = factory.CreateMqttClient();

            _options = new MqttClientOptionsBuilder()
                .WithTcpServer(host, port)
                .WithCredentials(username, password)
                .WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V311)   // MUST for Apache Apollo
                .WithCleanSession()
                .Build();

            _client.UseConnectedHandler(async e =>
            {
                Console.WriteLine("MQTT Connected!");
                OnConnected?.Invoke();

                await _client.SubscribeAsync(new MqttTopicFilterBuilder()
                    .WithTopic(topic)
                    .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                    .Build());

                Console.WriteLine("Subscribed: " + topic);
            });

            _client.UseDisconnectedHandler(async e =>
            {
                Console.WriteLine("MQTT Disconnected: " + e.Exception?.Message);

                // Apollo hay disconnect nếu timeout → tự động reconnect
                await Task.Delay(2000);

                try { await _client.ConnectAsync(_options); }
                catch { }
            });

            _client.UseApplicationMessageReceivedHandler(e =>
            {
                string json = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

                if (json.Contains("\"operator\": \"RecPush\""))
                {
                    var data = ParseRecPush(json);
                    OnRecPushReceived?.Invoke(data);
                }
            });

            // Connect
            await _client.ConnectAsync(_options);
        }

        private RecPushMessage ParseRecPush(string json)
        {
            var obj = JsonConvert.DeserializeObject<RecPushMessage>(json);

            if (!string.IsNullOrEmpty(obj.info.pic))
                obj.info.SavedImagePath = SaveBase64Image(obj.info.pic, "RecPush");

            return obj;
        }

        private string SaveBase64Image(string base64, string prefix)
        {
            if (string.IsNullOrEmpty(base64))
                return "";

            try
            {
                if (base64.Contains("base64,"))
                    base64 = base64.Substring(base64.IndexOf("base64,") + 7);

                byte[] bytes = Convert.FromBase64String(base64);

                string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
                Directory.CreateDirectory(folder);

                string filePath = Path.Combine(folder, $"{prefix}_{DateTime.Now:yyyyMMdd_HHmmss}.jpg");

                File.WriteAllBytes(filePath, bytes);
                return filePath;
            }
            catch
            {
                return "";
            }
        }
    }

    public class RecPushMessage
    {
        public string operatorName { get; set; }  // "RecPush"
        public RecPushInfo info { get; set; }
    }

    public class RecPushInfo
    {
        public string customId { get; set; }
        public string personId { get; set; }
        public int RecordID { get; set; }
        public int VerifyStatus { get; set; }
        public int PersonType { get; set; }
        public float similarity1 { get; set; }
        public float similarity2 { get; set; }
        public int Sendintime { get; set; }
        public string direction { get; set; }
        public string otype { get; set; }
        public string persionName { get; set; }
        public string facesluiceId { get; set; }
        public string facesluiceName { get; set; }
        public string idCard { get; set; }
        public string telnum { get; set; }
        public string time { get; set; }
        public string PushType { get; set; }
        public string OpendoorWay { get; set; }
        public string cardNum2 { get; set; }
        public string RFIDCard { get; set; }
        public string szQrCodeData { get; set; }
        public string dwFileIndex { get; set; }
        public string dwFilePos { get; set; }
        public string pic { get; set; }

        // Ảnh lưu ra file
        public string SavedImagePath { get; set; }
    }
}
