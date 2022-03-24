using MvpApi.Services.Utilities;
using System.Text.Json;

namespace MvpCompanion.Maui.Models.Authentication;

public static class AuthHelpers
{
    private static string _redirectUrl = "https://login.live.com/oauth20_desktop.srf";
    private static string _accessTokenUrl = "https://login.live.com/oauth20_token.srf";
    private static readonly string _clientId = "090fa1d9-3d6f-4f6f-a733-a8b8a3fe16ff";

    public static async Task<string> RequestAuthorizationAsync(string authCode, bool isRefresh = false)
    {
        try
        {
            using var client = new HttpClient();

            var postData = new List<KeyValuePair<string, string>>
            {
                new("client_id", _clientId),
                new("grant_type", isRefresh ? "refresh_token" : "authorization_code"),
                new(isRefresh ? "refresh_token" : "code", authCode.Split('&')[0]),
                new("redirect_uri", _redirectUrl)
            };

            // Construct the Form content, this is where I add the OAuth token (could be access token or refresh token)
            var postContent = new FormUrlEncodedContent(postData);

            // post the Form data
            using var response = await client.PostAsync(new Uri(_accessTokenUrl), postContent);

            // Read the response
            var responseTxt = await response.Content.ReadAsStringAsync();

            // Deserialize the parameters from the response
            var tokenData = JsonSerializer.Deserialize<AuthResponse>(responseTxt, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (!string.IsNullOrEmpty(tokenData.AccessToken))
            {
                //StorageHelpers.Instance.StoreToken("access_token", tokenData["access_token"]);
                //StorageHelpers.Instance.StoreToken("refresh_token", tokenData["refresh_token"]);
                Preferences.Set("access_token", tokenData.AccessToken);
                Preferences.Set("refresh_token", tokenData.RefreshToken);

                // We need to prefix the access token with the token type for the auth header. 
                // Currently this is always "bearer", doing this to be more future proof
                var tokenType = tokenData.TokenType;
                var cleanedAccessToken = tokenData.AccessToken.Split('&')[0];

                // set public property that is "returned"
                return $"{tokenType} {cleanedAccessToken}";
            }
            else
            {
                await Shell.Current.DisplayAlert("Unauthorized", "The account you signed in with did not provide an authorization code.", "ok");
            }
        }
        catch (HttpRequestException e)
        {
            await e.LogExceptionAsync();
            await Shell.Current.DisplayAlert("Error", $"Something went wrong signing you in, try again. {e.Message}", "ok");
        }
        catch (Exception e)
        {
            await e.LogExceptionAsync();
            await Shell.Current.DisplayAlert("Error", $"Something went wrong signing you in, try again. {e.Message}", "ok");
        }

        return null;
    }

    public static async Task<bool> ClearAuthorizationAsync()
    {
        try
        {
            // Erase cached tokens
            //StorageHelpers.Instance.DeleteToken("access_token");
            //StorageHelpers.Instance.DeleteToken("refresh_token");
            Preferences.Set("access_token", "");
            Preferences.Set("refresh_token", "");

            return true;
        }
        catch (Exception ex)
        {
            await ex.LogExceptionAsync();
            await Shell.Current.DisplayAlert("Sign out Error", ex.Message, "ok");

            return false;
        }
    }
}

