using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
//using Windows.Data.Json;
using Windows.Storage;
using Windows.UI.Popups;
using Microsoft.UI.Xaml;
//using Microsoft.AppCenter.Analytics;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using MvpApi.Common.CustomEventArgs;
using MvpApi.Services.Apis;
using MvpApi.Services.Utilities;
using MvpCompanion.UI.WinUI.ViewModels;
using MvpCompanion.UI.WinUI.Views;
using Newtonsoft.Json;

namespace MvpCompanion.UI.WinUI.Dialogs
{
    public sealed partial class LoginDialog : ContentDialog
    {
        private static readonly string Scope = "wl.emails%20wl.basic%20wl.offline_access%20wl.signin";
        private static readonly string ClientId = "090fa1d9-3d6f-4f6f-a733-a8b8a3fe16ff";
        private const string RedirectUrl = "https://login.live.com/oauth20_desktop.srf";
        private const string AccessTokenUrl = "https://login.live.com/oauth20_token.srf";
        private readonly Uri signInUri = new($"https://login.live.com/oauth20_authorize.srf?client_id={ClientId}&redirect_uri=https:%2F%2Flogin.live.com%2Foauth20_desktop.srf&response_type=code&scope={Scope}");
        private readonly Uri signOutUri = new($"https://login.live.com/oauth20_logout.srf?client_id={ClientId}&redirect_uri=https:%2F%2Flogin.live.com%2Foauth20_desktop.srf");
        private readonly Action loginCompleted;

        public LoginDialog()
        {
            InitializeComponent();
        }

        public LoginDialog(Action loginCompleted)
        {
            InitializeComponent();
            this.loginCompleted = loginCompleted;
        }

        protected override void OnBringIntoViewRequested(BringIntoViewRequestedEventArgs e)
        {
            base.OnBringIntoViewRequested(e);
        }

        private void UpdateBusyIndicatorMessage(string message)
        {
            ((ShellViewModel)ShellView.Instance.DataContext).IsBusyMessage = message;
        }

        private async Task CompleteSignInAsync(string authorizationHeader)
        {
            await InitializeMvpApiAsync(authorizationHeader);

            loginCompleted?.Invoke();

            Hide();
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
                    //Analytics.TrackEvent("LoginWindow SignInAsync - Seamless Signin Achieved");

                    await CompleteSignInAsync(authorizationHeader);

                    return;
                }
            }

            // important we let this fall through to avoid multiple else statements
            //Analytics.TrackEvent("LoginWindow SignInAsync - Manual Signin Required");

            AuthWebView.Source = signInUri;

            // Needs fresh login, navigate to sign in page
            await ShowAsync();
        }

        public async Task SignOutAsync()
        {
            //Analytics.TrackEvent("LoginWindow SignOutAsync");

            await ShowAsync();

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
                await ex.LogExceptionAsync();
                Trace.TraceError($"[LoginDialog] {ex}");
            }
            finally
            {
                // Hide busy indicator
                UpdateBusyIndicatorMessage("");

                // Toggle flag
                App.ApiService.IsLoggedIn = false;

                // Start auth workflow again (the logout Uri redirects to sign in again)
                AuthWebView.Source = signOutUri;
            }
        }

        public async Task InitializeMvpApiAsync(string authorizationHeader)
        {
            if (App.ApiService != null)
            {
                App.ApiService.AccessTokenExpired -= ApiService_AccessTokenExpired;
                App.ApiService.RequestErrorOccurred -= ApiService_RequestErrorOccurred;
            }

            //New-up the service
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

            // Force value changed for the GUI bound properties
            ((ShellViewModel)ShellView.Instance.DataContext).RefreshProperties();
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
            Trace.TraceError($"[LoginDialog] ApiService_RequestErrorOccurred: {e.Message}");

            var message = "Unknown Server Error";

            if (e.IsBadRequest)
            {
                message = e.Message;
            }
            else if (e.IsServerError)
            {
                message = e.Message + "\r\n\nIf this continues to happen, please open a GitHub Issue and we'll investigate further (find the GitHub link on the About page).";
            }
            
            await App.ShowMessageAsync(message, "MVP API Request Error");
        }

        public async Task<string> RequestAuthorizationAsync(string authCode, bool isRefresh = false)
        {
            Trace.WriteLine($"[LoginDialog] Request Authorization - Is Refresh: {isRefresh}.");
            
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

                //Deserialize the parameters from the response
                var tokenData = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseTxt);

                //Ensure response has access token
                if (tokenData != null && tokenData.ContainsKey("access_token"))
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
                await ex.LogExceptionAsync();

                if (ex.Message.Contains("401"))
                {
                    //TODO consider another message for HTTP specific errors
                }
            }
            catch (Exception ex)
            {
                await ex.LogExceptionAsync();
                await App.ShowMessageAsync($"There was a problem signing you in: {ex.Message}", "Sign in Error");
            }

            return authorizationHeader;
        }

        private async void AuthWebView_NavigationStarting(WebView2 sender, CoreWebView2NavigationStartingEventArgs e)
        {
            Trace.WriteLine($"[WinUI LoginDialog] Navigation Starting - Is redirected: {e.IsRedirected}, Uri: {e.Uri}");

            if (e.Uri.Contains("code="))
            {
                // WebView2 query parsing
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
                AuthWebView.Source = signInUri;
            }
        }

        private void AuthWebView_NavigationCompleted(WebView2 sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                Trace.WriteLine($"[WinUI LoginDialog] WebView Navigation Completed: {e.IsSuccess}");
            }
            else
            {
                Trace.TraceError($"[WinUI LoginDialog] WebView Navigation Error: {e.WebErrorStatus}");
            }

        }
    }
}
