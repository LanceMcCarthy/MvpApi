using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using MvpApi.Common.Extensions;
using MvpApi.Services.Utilities;
using Newtonsoft.Json;

namespace MvpApi.Uwp.Dialogs
{
    public sealed partial class LoginDialog : ContentDialog
    {
        // IMPORTANT Get your own ClientID key here https://mvpapi.portal.azure-api.net/
        // This clientID will be changed frequently and your app built with this will stop working
        private static readonly string _scope = "wl.emails%20wl.basic%20wl.offline_access%20wl.signin";
        private static readonly string _clientId = "090fa1d9-3d6f-4f6f-a733-a8b8a3fe16ff";
        private readonly string _redirectUrl = "https://login.live.com/oauth20_desktop.srf";
        private readonly string _accessTokenUrl = "https://login.live.com/oauth20_token.srf";
        private readonly Uri _signInUrl = new Uri($"https://login.live.com/oauth20_authorize.srf?client_id={_clientId}&redirect_uri=https:%2F%2Flogin.live.com%2Foauth20_desktop.srf&response_type=code&scope={_scope}");
        private readonly Uri _signOutUri = new Uri($"https://login.live.com/oauth20_logout.srf?client_id={_clientId}&redirect_uri=https:%2F%2Flogin.live.com%2Foauth20_desktop.srf");

        public string AuthorizationCode { get; set; }

        public LoginDialog()
        {
            InitializeComponent();
        }

        private void LoginDialog_OnSecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            AuthorizationCode = "";
            Hide();
        }
        
        private async void WebView_OnLoadCompleted(object sender, NavigationEventArgs e)
        {
            var url = e.Uri.AbsoluteUri;

            // get token
            if (url.Contains("code="))
            {
                var authCode = e.Uri.ExtractQueryValue("code");
                await RequestAuthorizationAsync(authCode);
            }
            else if (url.Contains("lc="))
            {
                // Redirect to signin page if there's a bounce
                LoginWebView.Source = _signInUrl;
                LoginWebView.Refresh();
            }
        }

        public async Task AttemptSilentRefreshAsync()
        {
            var refreshToken = StorageHelpers.Instance.LoadToken("refresh_token");

            if (!string.IsNullOrEmpty(refreshToken))
            {
                // there is a token stored, let's try to use it and not even have to show UI
                await RequestAuthorizationAsync(refreshToken, true);
            }
            else
            {
                // no token available, show dialog to get user to signin and accept
                await ShowAsync();
                LoginWebView.Source = _signInUrl;
                LoginWebView.Refresh();
            }
        }

        /// <summary>
        /// Shows the ContentDialog for manual sign in
        /// </summary>
        /// <returns></returns>
        public async Task SignInAsync()
        {
            await ShowAsync();
            LoginWebView.Source = _signInUrl;
        }

        // Shows ContentDialog to sign out
        public async Task<Tuple<bool, string>> SignOutAsync()
        {
            try
            {
                await ShowAsync();

                LoginWebView.Source = _signOutUri;

                StorageHelpers.Instance.DeleteToken("access_token");
                StorageHelpers.Instance.DeleteToken("refresh_token");

                return new Tuple<bool, string>(true, "You have signed out");
            }
            catch (Exception e)
            {
                await e.LogExceptionAsync();
                return new Tuple<bool, string>(false, $"Error signing out: {e.Message}");
            }
            finally
            {
                Hide();
            }
        }

        private async Task RequestAuthorizationAsync(string authCode, bool isRefresh = false)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    // Construct the Form content, this is where I add the OAuth token (could be access token or refresh token)
                    var postContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("client_id", _clientId),
                        new KeyValuePair<string, string>("grant_type", isRefresh ? "refresh_token" : "authorization_code"),
                        new KeyValuePair<string, string>(isRefresh ? "refresh_token" : "code", authCode.Split('&')[0]),
                        new KeyValuePair<string, string>("redirect_uri", _redirectUrl)
                    });

                    // Variable to hold the response data
                    var responseTxt = "";

                    // post the Form data
                    using (var response = await client.PostAsync(new Uri(_accessTokenUrl), postContent))
                    {
                        // Read the response
                        responseTxt = await response.Content.ReadAsStringAsync();
                    }

                    // Deserialize the parameters from the response
                    var tokenData = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseTxt);

                    if (tokenData.ContainsKey("access_token"))
                    {
                        // Store the expiration time of the token, currently 3600 seconds (an hour)
                        StorageHelpers.Instance.SaveSetting("expires_in", tokenData["expires_in"]);

                        StorageHelpers.Instance.StoreToken("access_token", tokenData["access_token"]);
                        StorageHelpers.Instance.StoreToken("refresh_token", tokenData["refresh_token"]);

                        // We need to prefix the access token with the token type for the auth header. 
                        // Currently this is always "bearer", doing this to be more future proof
                        var tokenType = tokenData["token_type"];
                        var cleanedAccessToken = tokenData["access_token"].Split('&')[0];

                        // set public property that is "returned"
                        AuthorizationCode = $"{tokenType} {cleanedAccessToken}";
                    }
                    else
                    {
                        // Always set the Authorization code to null if there was a problem.
                        AuthorizationCode = null;
                    }
                }
            }
            catch (HttpRequestException e)
            {
                await e.LogExceptionAsync();

                if (e.Message.Contains("401"))
                {
                    //TODO Try refresh token, access token is only valid for 60 minutes
                }

                Debug.WriteLine($"LoginDialog HttpRequestException: {e}");

                // Always set the Authorization code to null if there was a problem.
                AuthorizationCode = null;
            }
            catch (Exception e)
            {
                await e.LogExceptionAsync();
                Debug.WriteLine($"LoginDialog Exception: {e}");

                // Always set the Authorization code to null if there was a problem.
                AuthorizationCode = null;
            }
            finally
            {
                Hide();
            }
        }
    }
}
