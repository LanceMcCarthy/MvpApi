using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using MvpApi.Uwp.Common;
using Newtonsoft.Json;

namespace MvpApi.Uwp.Helpers
{
    public class OauthHelpers
    {
        private const string RedirectUrl = "https://login.live.com/oauth20_desktop.srf";
        private const string AccessTokenUrl = "https://login.live.com/oauth20_token.srf";
        private static readonly Uri SignInUrl = new Uri($"https://login.live.com/oauth20_authorize.srf?client_id={Constants.ClientId}&redirect_uri=https:%2F%2Flogin.live.com%2Foauth20_desktop.srf&response_type=code&scope={Constants.Scope}");
        private static string refreshTokenUrl = $"https://login.live.com/oauth20_token.srf?client_id={Constants.ClientId}&client_secret={Constants.ClientSecret}&redirect_uri=https://login.live.com/oauth20_desktop.srf&grant_type=refresh_token&refresh_token=";
        
        public static async Task<string> RequestAuthorizationAsync(string requestUrl, string authCode)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var content = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("client_id", Constants.ClientId),
                        new KeyValuePair<string, string>("grant_type", "authorization_code"),
                        new KeyValuePair<string, string>("code", authCode.Split('&')[0]),
                        new KeyValuePair<string, string>("redirect_uri", RedirectUrl)
                    };

                    var postContent = new FormUrlEncodedContent(content);

                    var responseTxt = "";
                    var authHeader = "";

                    using (var response = await client.PostAsync(new Uri(requestUrl), postContent))
                    {
                        responseTxt = await response.Content.ReadAsStringAsync();
                    }

                    var tokenData = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseTxt);

                    if (tokenData.ContainsKey("access_token"))
                    {
                        // Store the expiration time of the token, currently 3600 seconds (an hour)
                        StorageHelpers.SaveLocalSetting("expires_in", tokenData["expires_in"]);

                        StorageHelpers.StoreToken("access_token", tokenData["access_token"]);
                        StorageHelpers.StoreToken("refresh_token", tokenData["refresh_token"]);

                        // We need to prefix the access token with the token type for the auth header. 
                        // Currently this is always "bearer", doing this to be more future proof
                        var tokenType = tokenData["token_type"];
                        var cleanedAccessToken = tokenData["access_token"].Split('&')[0];

                        authHeader = $"{tokenType} {cleanedAccessToken}";
                    }

                    return authHeader;
                }
            }
            catch (HttpRequestException e)
            {
                if (e.Message.Contains("401"))
                {
                    //TODO Try refresh token, access token is only valid for 60 minutes
                }

                Debug.WriteLine($"RequestAuthorizationAsync HttpRequestException: {e}");
                return null;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"RequestAuthorizationAsync Exception: {e}");
                return null;
            }
        }
    }
}
