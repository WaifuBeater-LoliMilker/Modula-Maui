using System;
using System.Net.Http;

namespace Modula.Services
{
    public interface IApiService
    {
        public HttpClient Client { get; set; }
        public void SetAuthorizationHeader(string token);
        public void RemoveToken();
        public void SetBaseUrl(string newBaseUrl);
    }
    public class ApiService : IApiService
    {
        public HttpClient Client { get; set; }
        private string _token { get; set; } = "";

        public ApiService()
        {
            var baseURL = Preferences.Get("BaseURL", null) ?? "http://10.20.29.65:8088/rerpapi/api/";
            Client = new HttpClient
            {
                BaseAddress = new Uri(baseURL)
            };
            Client.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public void SetAuthorizationHeader(string token)
        {
            if (Client.DefaultRequestHeaders.Contains("Authorization"))
                Client.DefaultRequestHeaders.Remove("Authorization");
            Client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            _token = token;
        }
        public void RemoveToken()
        {
            Client.DefaultRequestHeaders.Remove("Authorization");
        }
        public void SetBaseUrl(string newBaseUrl)
        {
            if (string.IsNullOrWhiteSpace(newBaseUrl))
                throw new ArgumentException("Base URL cannot be empty.", nameof(newBaseUrl));
            Preferences.Set("BaseURL", newBaseUrl);
            Client = new HttpClient
            {
                BaseAddress = new Uri(newBaseUrl)
            };
            Client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_token}");
        }
    }
}