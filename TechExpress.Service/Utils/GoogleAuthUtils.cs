using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using TechExpress.Service.Dtos;

namespace TechExpress.Service.Utils
{
    public class GoogleAuthUtils
    {
        private readonly IConfiguration _config;

        public GoogleAuthUtils(IConfiguration config)
        {
            _config = config;
        }

        public string GetGoogleAuthUrl(string redirectUri)
        {
            var clientId = _config["Authentication:Google:ClientId"];
            var scope = "openid profile email";
            var responseType = "code";
            var accessType = "offline";
            var prompt = "consent";

            return $"https://accounts.google.com/o/oauth2/v2/auth?client_id={clientId}&redirect_uri={Uri.EscapeDataString(redirectUri)}&response_type={responseType}&scope={Uri.EscapeDataString(scope)}&access_type={accessType}&prompt={prompt}";
        }

        public async Task<string?> ExchangeCodeForTokenAsync(string code, string redirectUri)
        {
            var clientId = _config["Authentication:Google:ClientId"]!;
            var clientSecret = _config["Authentication:Google:ClientSecret"]!;

            using var client = new HttpClient();

            var requestContent = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "code", code },
                { "grant_type", "authorization_code" },
                { "redirect_uri", redirectUri }
            });

            try
            {
                var response = await client.PostAsync("https://oauth2.googleapis.com/token", requestContent);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var json = System.Text.Json.JsonDocument.Parse(content);
                    return json.RootElement.GetProperty("id_token").GetString();
                }
            }
            catch (Exception)
            {
                return null;
            }

            return null;
        }

        public async Task<GoogleUserInfo?> GetUserInfoFromIdTokenAsync(string idToken)
        {
            try
            {
                var clientId = _config["Authentication:Google:ClientId"];
                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new[] { clientId }
                });

                return new GoogleUserInfo
                {
                    Id = payload.Subject,
                    Email = payload.Email,
                    Name = payload.Name,
                    Picture = payload.Picture,
                    GivenName = payload.GivenName,
                    FamilyName = payload.FamilyName
                };
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
