using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Popups;
using MvpApi.Common.CustomEventArgs;
using MvpApi.Common.Extensions;
using MvpApi.Services.Apis;
using MvpApi.Services.Utilities;
using Newtonsoft.Json;
using Microsoft.UI.Xaml;
using MvpCompanion.UI.WinUI.Helpers;

namespace MvpCompanion.UI.WinUI
{
    public partial class LoginWindow : Window
    {
        private static readonly string Scope = "wl.emails%20wl.basic%20wl.offline_access%20wl.signin";
        private static readonly string ClientId = "090fa1d9-3d6f-4f6f-a733-a8b8a3fe16ff";
        private const string RedirectUrl = "https://login.live.com/oauth20_desktop.srf";
        private const string AccessTokenUrl = "https://login.live.com/oauth20_token.srf";
        private readonly Uri _signInUri = new($"https://login.live.com/oauth20_authorize.srf?client_id={ClientId}&redirect_uri=https:%2F%2Flogin.live.com%2Foauth20_desktop.srf&response_type=code&scope={Scope}");
        private readonly Uri _signOutUri = new($"https://login.live.com/oauth20_logout.srf?client_id={ClientId}&redirect_uri=https:%2F%2Flogin.live.com%2Foauth20_desktop.srf");

        private readonly Action _loginCompleted;

        public LoginWindow()
        {
            InitializeComponent();
        }

        public LoginWindow(Action loginCompleted)
        {
            InitializeComponent();
            _loginCompleted = loginCompleted;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
        private void UpdateBusyIndicatorMessage(string message)
        {
            //((SplashScreenDataContext)RadSplashScreenManager.SplashScreenDataContext).Content = message;
        }

        private async Task CompleteSignInAsync(string authorizationHeader)
        {
            await InitializeMvpApiAsync(authorizationHeader);

            _loginCompleted?.Invoke();

            this.Close();
            //Hide();
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
                    //Microsoft.AppCenter.Analytics.Analytics.TrackEvent("LoginWindow SignInAsync - Seamless Signin Achieved");

                    await CompleteSignInAsync(authorizationHeader);

                    return;
                }
            }

            // important we let this fall through to avoid multiple else statements
           // Microsoft.AppCenter.Analytics.Analytics.TrackEvent("LoginWindow SignInAsync - Manual Signin Required");

            AuthWebView.Source = _signInUri;

            // Needs fresh login, navigate to sign in page
            //ShowDialog();
            this.Activate();
        }

        public async Task SignOutAsync()
        {
            //Microsoft.AppCenter.Analytics.Analytics.TrackEvent("LoginWindow SignOutAsync");

            this.Activate();
            //Show();

            try
            {
                // Indicate to user we are signing out
                UpdateBusyIndicatorMessage("logging out...");

                // Erase cached tokens
                StorageHelpers.Instance.DeleteToken("access_token");
                StorageHelpers.Instance.DeleteToken("refresh_token");

                // Clean up profile objects
                UpdateBusyIndicatorMessage("resetting profile...");
                App.ApiService.Mvp = null;
                App.ApiService.ProfileImagePath = "";

                // Delete profile photo file
                UpdateBusyIndicatorMessage("deleting profile photo file...");

                var imageFile = await ApplicationData.Current.LocalFolder.TryGetItemAsync("ProfilePicture.jpg");
                if (imageFile != null) await imageFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
            catch (Exception ex)
            {
                //Crashes.TrackError(ex);
                await ex.LogExceptionWithUserMessage();
            }
            finally
            {
                // Hide busy indicator
                UpdateBusyIndicatorMessage("");

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
            UpdateBusyIndicatorMessage("downloading profile info...");
            App.ApiService.Mvp = await App.ApiService.GetProfileAsync();

            // Get MVP profile image
            UpdateBusyIndicatorMessage("downloading profile image...");
            App.ApiService.ProfileImagePath = await App.ApiService.DownloadAndSaveProfileImage();
        }

        private async void ApiService_AccessTokenExpired(object sender, ApiServiceEventArgs e)
        {
            if (e.IsTokenRefreshNeeded)
            {
                UpdateBusyIndicatorMessage("TOKEN EXPIRED! Refreshing...");
                await SignInAsync();
            }
            else
            {
                // Future use
            }
        }

        private static async void ApiService_RequestErrorOccurred(object sender, ApiServiceEventArgs e)
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

        public static async Task<string> RequestAuthorizationAsync(string authCode, bool isRefresh = false)
        {
            var authorizationHeader = "";

            try
            {
                using var client = new HttpClient();

                // Construct the Form content
                var postContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
                {
                    new ("client_id", ClientId),
                    new ("grant_type", isRefresh ? "refresh_token" : "authorization_code"),
                    new (isRefresh ? "refresh_token" : "code", authCode.Split('&')[0]),
                    new ("redirect_uri", RedirectUrl)
                });

                // Variable to hold the response data
                string responseTxt;

                // Post the Form data
                using (var response = await client.PostAsync(new Uri(AccessTokenUrl), postContent))
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
                //Crashes.TrackError(ex);

                await ex.LogExceptionWithUserMessage();

                if (ex.Message.Contains("401"))
                {
                    //TODO consider another message for HTTP specific errors
                }

                Debug.WriteLine($"LoginWindow HttpRequestException: {ex}");
            }
            catch (Exception ex)
            {
                //Crashes.TrackError(ex);

                await ex.LogExceptionWithUserMessage();
            }

            return authorizationHeader;
        }


        private async void AuthWebView_NavigationStarting(Microsoft.UI.Xaml.Controls.WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
        {
            
            if (e.Uri.Contains("code="))
            {
                // original
                //var authCode = e.Uri.ExtractQueryValue("code");

                // hack
                var queryString = e.Uri.Split('?')[1];
                var queryDictionary = System.Web.HttpUtility.ParseQueryString(queryString);
                var authCode = queryDictionary["code"];

                var authorizationHeader = await RequestAuthorizationAsync(authCode);

                if (!string.IsNullOrEmpty(authorizationHeader))
                {
                    await CompleteSignInAsync(authorizationHeader);
                }
            }
            else if (e.Uri.Contains("lc="))
            {
                // If the redirected uri doesn't have a code in query string, redirect with Uri that explicitly requests response_type=code
                AuthWebView.Source = _signInUri;
            }
        }


        private async void AuthWebView_NavigationCompleted(Microsoft.UI.Xaml.Controls.WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {

        }
    }
}
