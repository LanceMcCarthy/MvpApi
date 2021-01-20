using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using Windows.Storage;
using Windows.UI.Popups;
using Microsoft.AppCenter.Crashes;
using MvpApi.Common.CustomEventArgs;
using MvpApi.Common.Extensions;
using MvpApi.Services.Apis;
using MvpApi.Services.Utilities;
using MvpApi.Wpf.Helpers;
using Newtonsoft.Json;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.SplashScreen;
using Analytics = Microsoft.AppCenter.Analytics.Analytics;

namespace MvpApi.Wpf
{
    public partial class LoginWindow : Window
    {
        private static readonly string _scope = "wl.emails%20wl.basic%20wl.offline_access%20wl.signin";
        private static readonly string _clientId = "090fa1d9-3d6f-4f6f-a733-a8b8a3fe16ff";
        private readonly string _redirectUrl = "https://login.live.com/oauth20_desktop.srf";
        private readonly string _accessTokenUrl = "https://login.live.com/oauth20_token.srf";
        private readonly Uri _signInUri = new Uri($"https://login.live.com/oauth20_authorize.srf?client_id={_clientId}&redirect_uri=https:%2F%2Flogin.live.com%2Foauth20_desktop.srf&response_type=code&scope={_scope}");
        private readonly Uri _signOutUri = new Uri($"https://login.live.com/oauth20_logout.srf?client_id={_clientId}&redirect_uri=https:%2F%2Flogin.live.com%2Foauth20_desktop.srf");

        private readonly Action _loginCompleted;

        public LoginWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initialize with a callback that invokes when login completes
        /// </summary>
        /// <param name="loginCompleted"></param>
        public LoginWindow(Action loginCompleted)
        {
            InitializeComponent();
            _loginCompleted = loginCompleted;
        }

        private void ToggleBusy(string message, bool isBusy = true)
        {
            StatusTextBlock.Text = message;
            StatusTextBlock.Visibility = isBusy ? Visibility.Visible : Visibility.Collapsed;
        }

        private async Task CompleteSignInAsync(string authorizationHeader)
        {
            await InitializeMvpApiAsync(authorizationHeader);

            _loginCompleted?.Invoke();

            this.Hide();
        }

        private async void WebBrowser_OnNavigated(object sender, NavigationEventArgs e)
        {
            if (e.Uri.AbsoluteUri.Contains("code="))
            {
                var authCode = e.Uri.ExtractQueryValue("code");

                var authorizationHeader = await RequestAuthorizationAsync(authCode);

                if (!string.IsNullOrEmpty(authorizationHeader))
                {
                    await CompleteSignInAsync(authorizationHeader);
                }
            }
            else if (e.Uri.AbsoluteUri.Contains("lc="))
            {
                // If the redirected uri doesn't have a code in query string, redirect with Uri that explicitly requests response_type=code
                AuthWebView.Source = _signInUri;
            }
        }

        public async Task SignInAsync()
        {
            var refreshToken = StorageHelpers.Instance.LoadToken("refresh_token");

            // If refresh token is available, the user has previously been logged in and we can get a refreshed access token immediately
            if (!string.IsNullOrEmpty(refreshToken))
            {
                var authorizationHeader = await RequestAuthorizationAsync(refreshToken, true);

                if (!string.IsNullOrEmpty(authorizationHeader))
                {
                    Analytics.TrackEvent("LoginWindow SignInAsync - Seamless Signin Achieved");
                    await CompleteSignInAsync(authorizationHeader);
                    return;
                }
            }

            // important we let this fall through to avoid multiple else statements
            Analytics.TrackEvent("LoginWindow SignInAsync - Manual Signin Required");

            // Needs fresh login, navigate to sign in page
            this.ShowDialog();
            AuthWebView.Source = _signInUri;
        }

        public async Task SignOutAsync()
        {
            Analytics.TrackEvent("LoginWindow SignOutAsync");

            this.Show();

            try
            {
                // Indicate to user we are signing out
                ToggleBusy("logging out...");

                // Erase cached tokens
                //ViewModel.IsBusyMessage = "deleting cache files...";
                StorageHelpers.Instance.DeleteToken("access_token");
                StorageHelpers.Instance.DeleteToken("refresh_token");

                // Clean up profile objects
                ToggleBusy("resetting profile...");
                App.ApiService.Mvp = null;
                App.ApiService.ProfileImagePath = "";

                // Delete profile photo file
                ToggleBusy("deleting profile photo file...");

                var imageFile = await ApplicationData.Current.LocalFolder.TryGetItemAsync("ProfilePicture.jpg");
                if (imageFile != null) await imageFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);
                await ex.LogExceptionWithUserMessage();
            }
            finally
            {
                // Hide busy indicator
                ToggleBusy("", false);

                // Toggle flag
                App.ApiService.IsLoggedIn = false;

                // Start auth workflow again (the logout Uri redirects to sign in again)
                AuthWebView.Source = _signOutUri;
            }
        }

        public async Task InitializeMvpApiAsync(string authorizationHeader)
        {
            if (App.ApiService != null)
            {
                App.ApiService.AccessTokenExpired -= ApiService_AccessTokenExpired;
                App.ApiService.RequestErrorOccurred -= ApiService_RequestErrorOccurred;
            }

            // New-up the service
            App.ApiService = new MvpApiService(authorizationHeader);

            App.ApiService.AccessTokenExpired += ApiService_AccessTokenExpired;
            App.ApiService.RequestErrorOccurred += ApiService_RequestErrorOccurred;

            App.ApiService.IsLoggedIn = true;

            // Get MVP profile
            ((SplashScreenDataContext)RadSplashScreenManager.SplashScreenDataContext).Content = "downloading profile info...";
            App.ApiService.Mvp = await App.ApiService.GetProfileAsync();

            // Get MVP profile image
            ((SplashScreenDataContext)RadSplashScreenManager.SplashScreenDataContext).Content = "downloading profile image...";
            App.ApiService.ProfileImagePath = await App.ApiService.DownloadAndSaveProfileImage();

            ((SplashScreenDataContext)RadSplashScreenManager.SplashScreenDataContext).Content = "";
        }

        private async void ApiService_AccessTokenExpired(object sender, ApiServiceEventArgs e)
        {
            if (e.IsTokenRefreshNeeded)
            {
                await SignInAsync();
            }
            else
            {
                // Future use
            }
        }

        private async void ApiService_RequestErrorOccurred(object sender, ApiServiceEventArgs e)
        {
            var message = "Unknown Server Error";

            if (e.IsBadRequest)
            {
                message = e.Message;
            }
            else if (e.IsServerError)
            {
                message = e.Message + "\r\n\nIf this continues to happen, please open a GitHub Issue and we'll investigate further (find the GitHub link on the About page).";
            }

            await new MessageDialog(message, "MVP API Request Error").ShowAsync();
        }

        public async Task<string> RequestAuthorizationAsync(string authCode, bool isRefresh = false)
        {
            var authorizationHeader = "";

            try
            {
                using var client = new HttpClient();

                // Construct the Form content
                var postContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("client_id", _clientId),
                    new KeyValuePair<string, string>("grant_type", isRefresh ? "refresh_token" : "authorization_code"),
                    new KeyValuePair<string, string>(isRefresh ? "refresh_token" : "code", authCode.Split('&')[0]),
                    new KeyValuePair<string, string>("redirect_uri", _redirectUrl)
                });

                // Variable to hold the response data
                var responseTxt = "";

                // Post the Form data
                using (var response = await client.PostAsync(new Uri(_accessTokenUrl), postContent))
                {
                    // Read the response
                    responseTxt = await response.Content.ReadAsStringAsync();
                }

                // Deserialize the parameters from the response
                var tokenData = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseTxt);

                // Ensure response has access token
                if (tokenData.ContainsKey("access_token"))
                {
                    // Store the expiration time of the token, currently 3600 seconds (an hour)
                    StorageHelpers.Instance.SaveSetting("expires_in", tokenData["expires_in"]);

                    // Store tokens (NOTE: The tokens are encrypted with Rijindel before storing in LocalFolder)
                    StorageHelpers.Instance.StoreToken("access_token", tokenData["access_token"]);
                    StorageHelpers.Instance.StoreToken("refresh_token", tokenData["refresh_token"]);

                    // We need to prefix the access token with the token type for the auth header. 
                    // Currently this is always "bearer", doing this to be more future proof
                    var tokenType = tokenData["token_type"];
                    var cleanedAccessToken = tokenData["access_token"].Split('&')[0];

                    // Build the Bearer authorization header
                    authorizationHeader = $"{tokenType} {cleanedAccessToken}";
                }
            }
            catch (HttpRequestException ex)
            {
                Crashes.TrackError(ex);

                await ex.LogExceptionWithUserMessage();

                if (ex.Message.Contains("401"))
                {
                    //TODO consider another message HTTP specific errors
                }

                Debug.WriteLine($"LoginWindow HttpRequestException: {ex}");
            }
            catch (Exception ex)
            {

                Crashes.TrackError(ex);

                await ex.LogExceptionWithUserMessage();
            }

            return authorizationHeader;
        }
    }
}
