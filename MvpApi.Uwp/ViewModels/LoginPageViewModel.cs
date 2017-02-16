using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using MvpApi.Services;
using MvpApi.Uwp.Common;
using Newtonsoft.Json;
using Template10.Common;
using Template10.Mvvm;

namespace MvpApi.Uwp.ViewModels
{
    public class LoginPageViewModel : ViewModelBase
    {
        #region Fields

        private const string RedirectUrl = "https://login.live.com/oauth20_desktop.srf";
        private const string AccessTokenUrl = "https://login.live.com/oauth20_token.srf";
        private static readonly Uri SignInUrl = new Uri($"https://login.live.com/oauth20_authorize.srf?client_id={Constants.ClientId}&redirect_uri=https:%2F%2Flogin.live.com%2Foauth20_desktop.srf&response_type=code&scope={Constants.Scope}");
        private static string refreshTokenUrl = $"https://login.live.com/oauth20_token.srf?client_id={Constants.ClientId}&client_secret={Constants.ClientSecret}&redirect_uri=https://login.live.com/oauth20_desktop.srf&grant_type=refresh_token&refresh_token=";
        
        private readonly ApplicationDataContainer localSettings;
        

        // backing fields
        private Uri browserUri;
        private bool isBusy;
        private string isBusyMessage;

        #endregion

        public LoginPageViewModel()
        {
            if (!DesignMode.DesignModeEnabled)
            {
                localSettings = ApplicationData.Current.LocalSettings;
            }
        }

        #region Properties

        public Uri BrowserUri
        {
            get { return browserUri; }
            set { Set(ref browserUri, value); }
        }
        public bool IsBusy
        {
            get { return isBusy; }
            set { Set(ref isBusy, value); }
        }

        public string IsBusyMessage
        {
            get { return isBusyMessage; }
            set { Set(ref isBusyMessage, value); }
        }

        #endregion

        #region OAuth2 Login

        public void BrowserWindow_LoadCompleted(object sender, NavigationEventArgs e)
        {
            if (e.Uri.AbsoluteUri.Contains("code="))
            {
                var authCode = Regex.Split(e.Uri.AbsoluteUri, "code=")[1];

                SaveToken("auth_code", authCode);
                
                RequestAccessTokenAsync(AccessTokenUrl, authCode);
            }
            else if (e.Uri.AbsoluteUri.Contains("lc="))
            {
                BrowserUri = SignInUrl;
            }
        }

        private async void RequestAccessTokenAsync(string requestUrl, string authCode)
        {
            using (var client = new HttpClient())
            {
                var content = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("client_id", Constants.ClientId),
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("code", authCode.Split('&')[0]),
                    new KeyValuePair<string, string>("redirect_uri", RedirectUrl),
                };

                var postContent = new FormUrlEncodedContent(content);

                using (var response = await client.PostAsync(new Uri(requestUrl), postContent))
                {
                    var responseTxt = await response.Content.ReadAsStringAsync();

                    var tokenData = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseTxt);

                    if (tokenData.ContainsKey("access_token"))
                    {
                        SaveToken("access_token", tokenData["access_token"]);
                        SaveToken("refresh_token", tokenData["refresh_token"]);

                        var cleanAccessToken = tokenData["access_token"].Split('&')[0];

                        App.ApiService = new MvpApiService(Constants.SubscriptionKey, cleanAccessToken);
                        
                        await LoadProfileInfoAsync();
                    }
                }
            }
        }
        
        #endregion

        #region Methods

        private async Task LoadProfileInfoAsync()
        {
            try
            {
                IsBusy = true;

                var shellVm = App.ShellPage.DataContext as ShellPageViewModel;
                if (shellVm != null)
                {
                    shellVm.IsLoggedIn = true;
                    IsBusyMessage = "downloading profile info...";
                    shellVm.Mvp = await App.ApiService.GetProfileAsync();

                    IsBusyMessage = "downloading profile image...";
                    shellVm.ProfileImagePath = await LoadProfileImageAsync();
                }

                if (BootStrapper.Current.NavigationService.CanGoBack)
                    BootStrapper.Current.NavigationService.GoBack();
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
        }

        private async Task<string> LoadProfileImageAsync()
        {
            try
            {
                // download the image
                var imageBytes = await App.ApiService.GetProfileImageAsync();

                using (var ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
                {
                    // Save the image
                    var imageFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("ProfilePicture.jpg", CreationCollisionOption.ReplaceExisting);

                    using (var fileStream = await imageFile.OpenStreamForWriteAsync())
                    {
                        ms.CopyTo(fileStream);
                    }

                    return imageFile.Path;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }

        #endregion

        //TODO Will be updated to use Password Vault or Windows Cryptograpy https://msdn.microsoft.com/en-us/windows/uwp/security/data-protection
        #region Key Storage

        private void SaveToken(string key, string value)
        {
            localSettings.Values["key"] = value;
            Debug.WriteLine($"SaveToken - Key:{key}, Value: {value}");
        }

        private string LoadToken(string key)
        {
            var value = localSettings.Values["key"] as string;
            Debug.WriteLine($"LoadToken - Key:{key}, Value: {value}");
            return value;
        }

        #endregion


        #region Navigation

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            bool loggingOut = false;

            if (parameter is bool)
                loggingOut = (bool)parameter;

            if (loggingOut)
            {
                // Log out
                BrowserUri = new Uri($"https://login.live.com/oauth20_logout.srf?client_id={Constants.ClientId}&redirect_uri=https:%2F%2Flogin.live.com%2Foauth20_desktop.srf");

                SaveToken("access_token", "");
                SaveToken("refresh_token", "");

                // Clean up Mvp info
                var shellVm = App.ShellPage.DataContext as ShellPageViewModel;
                if (shellVm != null)
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
