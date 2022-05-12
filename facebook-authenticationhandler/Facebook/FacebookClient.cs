using FacebookAuthenticationhandler.Facebook.Models;
using Newtonsoft.Json;

namespace FacebookAuthenticationhandler.Facebook
{
    public interface IFacebookClient
    {
        Task<App> GetApp(string accessToken);
        Task<User> GetUser(string accessToken, string fields);
    }

    public class FacebookClient : IFacebookClient
    {
        private readonly HttpClient _httpClient;

        public FacebookClient(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<User> GetUser(string accessToken, string fields)
        {
            using var response = await _httpClient.GetAsync($"me?access_token={accessToken}&fields={fields}");
            response.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<User>(await response.Content.ReadAsStringAsync());
        }

        public async Task<App> GetApp(string accessToken)
        {
            using var response = await _httpClient.GetAsync($"app?access_token={accessToken}");
            response.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<App>(await response.Content.ReadAsStringAsync());
        }
    }
}
