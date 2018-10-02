using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Navigation;
using MvpApi.Uwp.Common;
using MvpApi.Uwp.Extensions;
using MvpApi.Uwp.Helpers;
using MvpApi.Uwp.Services;
using Newtonsoft.Json;
using Template10.Common;

namespace MvpApi.Uwp.ViewModels
{
    public class LoginPageViewModel : PageViewModelBase
    {
        #region Fields

        private static readonly string RedirectUrl = "https://login.live.com/oauth20_desktop.srf";
        public static readonly string AccessTokenUrl = "https://login.live.com/oauth20_token.srf";
        private static readonly Uri SignInUrl = new Uri($"https://login.live.com/oauth20_authorize.srf?client_id={Constants.ClientId}&redirect_uri=https:%2F%2Flogin.live.com%2Foauth20_desktop.srf&response_type=code&scope={Constants.Scope}");
        private static readonly string RefreshTokenUrl = $"https://login.live.com/oauth20_token.srf?client_id={Constants.ClientId}&client_secret={Constants.ClientSecret}&redirect_uri=https://login.live.com/oauth20_desktop.srf&grant_type=refresh_token&refresh_token=";
        
        private Uri browserUri;

        #endregion

        public LoginPageViewModel()
        {
        }

        #region Properties

        public Uri BrowserUri
        {
            get => browserUri;
            set => Set(ref browserUri, value);
        }

        #endregion
        
        public async void BrowserWindow_LoadCompleted(object sender, NavigationEventArgs e)
        {
            if (e.Uri.AbsoluteUri.Contains("code="))
            {
                var authCode = e.Uri.ExtractQueryValue("code");

                // get access token
                var apiAuthorization = await RequestAuthorizationAsync(AccessTokenUrl, authCode);

                if (string.IsNullOrEmpty(apiAuthorization))
                {
                    Debug.WriteLine("BrowserWindow_LoadCompleted - apiAuthorization was null");
                    return;
                }
                
                try
                {
                    // Auth is good, new up the ApiService
                    App.ApiService = new MvpApiService(Constants.SubscriptionKey, apiAuthorization);

                    IsBusy = true;

                    if (App.ShellPage.DataContext is ShellPageViewModel shellVm)
                    {
                        shellVm.IsLoggedIn = true;
                        IsBusyMessage = "downloading profile info...";
                        shellVm.Mvp = await App.ApiService.GetProfileAsync();

                        IsBusyMessage = "downloading profile image...";
                        shellVm.ProfileImagePath = await App.ApiService.DownloadAndSaveProfileImage(ApplicationData.Current.LocalFolder);
                    }
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception);
                }
                finally
                {
                    IsBusyMessage = "";
                    IsBusy = false;
                }

                if (BootStrapper.Current.NavigationService.CanGoBack)
                    BootStrapper.Current.NavigationService.GoBack();

            }
            else if (e.Uri.AbsoluteUri.Contains("lc="))
            {
                BrowserUri = SignInUrl;
            }
        }

        public static async Task<string> RequestAuthorizationAsync(string requestUrl, string authCode, bool refresh = false)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var content = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("client_id", Constants.ClientId),
                        new KeyValuePair<string, string>("grant_type", refresh ? "refresh_token" : "authorization_code"),
                        new KeyValuePair<string, string>(refresh ? "refresh_token" : "code", authCode.Split('&')[0]),
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

        #region Navigation

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            bool loggingOut = false;

            if (parameter is bool b)
                loggingOut = b;

            if (loggingOut)
            {
                // Log out
                BrowserUri = new Uri($"https://login.live.com/oauth20_logout.srf?client_id={Constants.ClientId}&redirect_uri=https:%2F%2Flogin.live.com%2Foauth20_desktop.srf");

                StorageHelpers.DeleteToken("access_token");
                StorageHelpers.DeleteToken("refresh_token");

                // Clean up Mvp info
                if (App.ShellPage.DataContext is ShellPageViewModel shellVm)
                {
                    shellVm.IsLoggedIn = false;
                    shellVm.Mvp = null;
                    shellVm.ProfileImagePath = null;
                }

                // Clean up file
                try
                {
                    var imageFile = await ApplicationData.Current.LocalFolder.TryGetItemAsync("ProfilePicture.jpg");
                    if (imageFile != null)
                        await imageFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
            else
            {
                // TODO Figure out how to use refresh token
                // Login
                BrowserUri = SignInUrl;
            }
        }

        public override Task OnNavigatedFromAsync(IDictionary<string, object> pageState, bool suspending)
        {
            return base.OnNavigatedFromAsync(pageState, suspending);
        }

        #endregion
    }
}
