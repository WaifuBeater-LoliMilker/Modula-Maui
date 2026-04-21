namespace Modula.Services
{
    public interface IModulaApiService
    {
        public HttpClient Client { get; set; }
        public void SetAuthorizationHeader(string token);
        public string GetAccessToken();
        public void RemoveToken();
        public void SetBaseUrl(string newBaseUrl);
    }
}
