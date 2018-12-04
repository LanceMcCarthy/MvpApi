using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Microsoft.Toolkit.Uwp.Connectivity;
using MvpApi.Common.Extensions;
using MvpApi.Common.Models;
using MvpApi.Services.Apis;
using MvpApi.Services.Utilities;
using MvpApi.Uwp.Dialogs;
using MvpApi.Uwp.Helpers;
using Newtonsoft.Json;
using Template10.Common;

namespace MvpApi.Uwp.ViewModels
{
    public class ShellPageViewModel : PageViewModelBase
    {
        private static readonly string _scope = "wl.emails%20wl.basic%20wl.offline_access%20wl.signin";
        private static readonly string _clientId = "090fa1d9-3d6f-4f6f-a733-a8b8a3fe16ff";
        private readonly string _redirectUrl = "https://login.live.com/oauth20_desktop.srf";
        private readonly string _accessTokenUrl = "https://login.live.com/oauth20_token.srf";
        private readonly Uri _signInUrl = new Uri($"https://login.live.com/oauth20_authorize.srf?client_id={_clientId}&redirect_uri=https:%2F%2Flogin.live.com%2Foauth20_desktop.srf&response_type=code&scope={_scope}");

        private Uri _webViewUri;
        private ProfileViewModel _mvp;
        private string _profileImagePath;
        private bool _isLoggedIn;
        
        public ShellPageViewModel()
        {
            if (DesignMode.DesignModeEnabled)
            {
                Mvp = DesignTimeHelpers.GenerateSampleMvp();
                IsLoggedIn = true;
                ProfileImagePath = "/Images/MvpIcon.png";
                return;
            }
        }

        /// <summary>
        /// File path to locally saved MVP profile image
        /// </summary>
        public string ProfileImagePath
        {
            get => _profileImagePath;
            set
            {
                //enforcing propChanged
                _profileImagePath = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Currently signed in MVP profile
        /// </summary>
        public ProfileViewModel Mvp
        {
            get => _mvp;
            set => Set(ref _mvp, value);
        }
        
        /// <summary>
        /// Denotes whether the user is currently logged in and able to make successful requests to the API
        /// </summary>
        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set => Set(ref _isLoggedIn, value);
        }

        public Uri WebViewUri
        {
            get => _webViewUri;
            set => Set(ref _webViewUri, value);
        }

        /// <summary>
        /// Logs in the user by showing a ContentDialog for authentication.
        /// Silent Operation if the user has previously logged in, a silent login will be attempted first (using the refresh token).
        /// </summary>
        /// <returns></returns>
        public async Task<bool> SignInAsync()
        {
            if (IsLoggedIn)
            {
                return IsLoggedIn;
            }
            else
            {
                await SignOutAsync();
                return IsLoggedIn;
            }
            // Initialize LoginDialog. This is a special ContentDialog that will handle authentication lifecycle
            //if(_loginManager == null)
            //{
            //    _loginManager = new LoginDialog();
            //}

            //try
            //{
            //    // Invoke sign in procedure (this handles token storage internally)
            //    await _loginManager.AttemptSilentRefreshAsync();

            //    if (!string.IsNullOrEmpty(_loginManager.AuthorizationCode))
            //    {
            //        App.ApiService = new MvpApiService(_loginManager.AuthorizationCode);

            //        IsBusyMessage = "downloading profile info...";
            //        Mvp = await App.ApiService.GetProfileAsync();

            //        IsBusyMessage = "downloading profile image...";
            //        ProfileImagePath = await App.ApiService.DownloadAndSaveProfileImage();

            //        IsLoggedIn = true;
            //    }
            //}
            //catch (Exception e)
            //{
            //    await e.LogExceptionAsync();
            //}

            //return !string.IsNullOrEmpty(_loginManager.AuthorizationCode);
        }

        public async Task SignOutAsync()
        {
            // Invoke sign out procedure (this will handle token storage internally), returns Tuple with success and message
            //var result = await _loginManager.SignOutAsync();

            try
            {
                IsLoggedIn = false;

                var builder = new UriBuilder("https://login.live.com/oauth20_logout.srf");

                var query = HttpUtility.ParseQueryString(builder.Query);
                query["client_id"] = _clientId;
                query["redirect_uri"] = _redirectUrl;
                builder.Query = query.ToString();

                WebViewUri = new Uri(builder.ToString());

                StorageHelpers.Instance.DeleteToken("access_token");
                StorageHelpers.Instance.DeleteToken("refresh_token");

                // Clean up profile objects
                IsLoggedIn = false;
                Mvp = null;
                ProfileImagePath = "";

                // Clean up files
                try
                {
                    // Profile photo file
                    var imageFile = await ApplicationData.Current.LocalFolder.TryGetItemAsync("ProfilePicture.jpg");
                    if (imageFile != null)
                    {
                        await imageFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
                    }

                    // TODO Clean up Log files (there are automatically kept to a limit of 5 by the logging system).
                    //var query = ApplicationData.Current.LocalFolder.CreateFileQuery();
                    //var files = await query.GetFilesAsync();

                    //foreach (var file in files)
                    //{
                    //    if (file.FileType == "log")
                    //    {
                    //        await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
                    //    }
                    //}
                }
                catch (Exception e)
                {
                    await e.LogExceptionWithUserMessage();
                    Debug.WriteLine(e);
                }
            }
            catch (Exception ex)
            {
                await ex.LogExceptionWithUserMessage();
            }

            // If sign out was successful, handle app-specific files and resources
            //if (result.Item1)
            //{
            //    // Clean up profile objects
            //    IsLoggedIn = false;
            //    Mvp = null;
            //    ProfileImagePath = "";
                
            //    // Clean up files
            //    try
            //    {
            //        // Profile photo file
            //        var imageFile = await ApplicationData.Current.LocalFolder.TryGetItemAsync("ProfilePicture.jpg");
            //        if (imageFile != null)
            //        {
            //            await imageFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
            //        }

            //        // TODO Clean up Log files (there are automatically kept to a limit of 5 by the logging system).
            //        //var query = ApplicationData.Current.LocalFolder.CreateFileQuery();
            //        //var files = await query.GetFilesAsync();
                    
            //        //foreach (var file in files)
            //        //{
            //        //    if (file.FileType == "log")
            //        //    {
            //        //        await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
            //        //    }
            //        //}
            //    }
            //    catch (Exception e)
            //    {
            //        await ExceptionLogger.LogExceptionAsync(e);
            //        Debug.WriteLine(e);
            //    }
            //}

            //// show user result of sign out attempt
            //await new MessageDialog($"{result.Item2}", result.Item1 ? "Success" : "Error").ShowAsync();
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (!NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
            {
                await new MessageDialog("This application requires an internet connection. Please check your connection and launch the app again.", "No Internet").ShowAsync();
                return;
            }
        }

        public override Task OnNavigatedFromAsync(IDictionary<string, object> pageState, bool suspending)
        {
            return base.OnNavigatedFromAsync(pageState, suspending);
        }

        #region Authentication

        
        //private readonly Uri _signOutUri = new Uri($"https://login.live.com/oauth20_logout.srf?client_id={_clientId}&redirect_uri=https:%2F%2Flogin.live.com%2Foauth20_desktop.srf");

        public async void LoginMenuFlyoutItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (IsLoggedIn)
            {
                var md = new MessageDialog("You are already signed in. In case you're having trouble, you can force a refresh by logging out.");

                md.Commands.Add(new UICommand("Logout"));
                md.Commands.Add(new UICommand("Cancel"));

                var result = await md.ShowAsync();

                if (result.Label == "Logout")
                {
                    await SignOutAsync();
                }
            }
            else
            {
                // Is already logged out, try reloading the login url
                WebViewUri = _signInUrl;
            }
        }

        public async void LogoutMenuFlyoutItem_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                await SignOutAsync();
                //IsLoggedIn = false;

                //var builder = new UriBuilder("https://login.live.com/oauth20_logout.srf");

                //var query = HttpUtility.ParseQueryString(builder.Query);
                //query["client_id"] = _clientId;
                //query["redirect_uri"] = _redirectUrl;
                //builder.Query = query.ToString();

                //WebViewUri = new Uri(builder.ToString());

                //StorageHelpers.Instance.DeleteToken("access_token");
                //StorageHelpers.Instance.DeleteToken("refresh_token");
            }
            catch (Exception ex)
            {
                await ex.LogExceptionWithUserMessage();
            }
        }

        public async void WebView_OnNavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs e)
        {
            if (e.Uri == null || e.Uri.AbsoluteUri == null)
            {
                WebViewUri = _signInUrl;
                return;
            }

            // get token
            if (e.Uri.AbsoluteUri.Contains("code="))
            {
                var authCode = e.Uri.ExtractQueryValue("code");
                await RequestAuthorizationAsync(authCode);
            }
            else if (e.Uri.AbsoluteUri.Contains("lc="))
            {
                // Redirect to signin page if there's a bounce
                WebViewUri = _signInUrl;
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
                        var authorization = $"{tokenType} {cleanedAccessToken}";

                        App.ApiService = new MvpApiService(authorization);

                        IsLoggedIn = true;

                        IsBusyMessage = "downloading profile info...";
                        Mvp = await App.ApiService.GetProfileAsync();

                        IsBusyMessage = "downloading profile image...";
                        ProfileImagePath = await App.ApiService.DownloadAndSaveProfileImage();

                        // The current page should be HomePage, so we can just call refresh
                        BootStrapper.Current.NavigationService.Refresh();
                        IsLoggedIn = true;
                    }
                }
            }
            catch (HttpRequestException e)
            {
                await e.LogExceptionWithUserMessage();

                if (e.Message.Contains("401"))
                {
                    //TODO Try refresh token, access token is only valid for 60 minutes
                }

                Debug.WriteLine($"LoginDialog HttpRequestException: {e}");
                
            }
            catch (Exception e)
            {
                await e.LogExceptionWithUserMessage();

                Debug.WriteLine($"LoginDialog Exception: {e}");
            }
        }

        #endregion
    }
}
