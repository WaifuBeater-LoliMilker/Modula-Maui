namespace Modula.Services
{
    public class ModulaApiService : IModulaApiService
    {
        public HttpClient Client { get; set; }
        private string _token { get; set; } = "";

        public ModulaApiService()
        {
            var baseURL = Preferences.Get("MODULA_API_URL", "http://10.20.29.65:8088/rerpapi/api/");
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
        public string GetAccessToken()
        {
            return _token;
        }
        public void RemoveToken()
        {
            Client.DefaultRequestHeaders.Remove("Authorization");
        }
        public void SetBaseUrl(string newBaseUrl)
        {
            if (string.IsNullOrWhiteSpace(newBaseUrl))
                throw new ArgumentException("Base URL cannot be empty.", nameof(newBaseUrl));
            Preferences.Set("MODULA_API_URL", newBaseUrl);
            Client = new HttpClient
            {
                BaseAddress = new Uri(newBaseUrl)
            };
            Client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_token}");
        }
    }
}
