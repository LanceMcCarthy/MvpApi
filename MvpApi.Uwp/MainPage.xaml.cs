using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using MvpApi.Services;
using MvpApi.Uwp.Common;
using Newtonsoft.Json;

namespace MvpApi.Uwp
{
    public sealed partial class MainPage : Page
    {
        private readonly ApplicationDataContainer localSettings;
        private MvpApiService mvpApiService;
        private bool isLoggedIn;
        
        private static Uri SignInUrl = new Uri($"https://login.live.com/oauth20_authorize.srf?client_id={Constants.ClientId}&redirect_uri=https:%2F%2Flogin.live.com%2Foauth20_desktop.srf&response_type=code&scope={Constants.Scope}");
        private static string RedirectUrl = "https://login.live.com/oauth20_desktop.srf";
        private static string RefreshTokenUrl = $"https://login.live.com/oauth20_token.srf?client_id={Constants.ClientId}&client_secret={Constants.ClientSecret}&redirect_uri=https://login.live.com/oauth20_desktop.srf&grant_type=refresh_token&refresh_token=";

        private static string AccessTokenUrl = $"https://login.live.com/oauth20_token.srf";

        public MainPage()
        {
            this.InitializeComponent();
            browserWindow.LoadCompleted += BrowserWindow_LoadCompleted;

            localSettings = ApplicationData.Current.LocalSettings;

            Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            //isLoggedIn = !string.IsNullOrEmpty(LoadToken("access_token"));
            
            //if (isLoggedIn)
            //{
            //    browserWindow.Visibility = Visibility.Collapsed;
            //    mvpApiService = new MvpApiService(subscriptionKey, LoadToken("access_token"));
            //}
            //else
            //{
            //    browserWindow.Visibility = Visibility.Visible;
            //    browserWindow.Navigate(signInUrl);
            //}
        }
        
        #region Button event handlers

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (isLoggedIn)
            {
                isLoggedIn = false;
                GetProfileInfoButton.IsEnabled = false;

                var logOutUri = new Uri(String.Format($"https://login.live.com/oauth20_logout.srf?client_id={Constants.ClientId}&redirect_uri=https:%2F%2Flogin.live.com%2Foauth20_desktop.srf"));
                browserWindow.Navigate(logOutUri);

                SaveToken("access_token", "");
                SaveToken("refresh_token", "");
            }
            else
            {
                browserWindow.Navigate(SignInUrl);
            }
        }
        
        private async void GetProfileInfoButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (mvpApiService == null)
                {
                    mvpApiService = new MvpApiService(Constants.SubscriptionKey, LoadToken("access_token"));
                }

                var result = await mvpApiService.GetProfileAsync();
                ProfileContentControl.DataContext = result;

                await LoadProfileImage();
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
            }
        }

        public async Task<BitmapImage> Base64ToImage(string base64String)
        {
            // Convert base 64 string to byte[]
            byte[] imageBytes = Convert.FromBase64String(base64String);
            // Convert byte[] to Image
            using (var ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
            {
                BitmapImage image = new BitmapImage();
                await image.SetSourceAsync(ms.AsRandomAccessStream());
                return image;
            }
        }

        private async Task LoadProfileImage()
        {
            try
            {
                // Checkto see if we've previously saved it
                var imageFile = await ApplicationData.Current.LocalFolder.TryGetItemAsync("ProfilePicture.jpg");

                if (imageFile == null)
                {
                    // download the image
                    var imageBytes = await mvpApiService.GetProfileImageAsync();

                    using (var ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
                    {
                        // Save the image
                        imageFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("ProfilePicture.jpg", CreationCollisionOption.ReplaceExisting);
                        using (var fileStream = await ((StorageFile)imageFile).OpenStreamForWriteAsync())
                        {
                            ms.CopyTo(fileStream);
                        }
                    }
                }
                
                ProfileImageBrush.ImageSource = new BitmapImage(new Uri("ms-appdata:///local/ProfilePicture.jpg"));
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        #endregion

        #region OAuth2 Login

        private void BrowserWindow_LoadCompleted(object sender, NavigationEventArgs e)
        {
            if (e.Uri.AbsoluteUri.Contains("code="))
            {
                var authCode = Regex.Split(e.Uri.AbsoluteUri, "code=")[1];

                SaveToken("auth_code", authCode);

                browserWindow.Visibility = Visibility.Collapsed;

                RequestAccessTokenAsync(AccessTokenUrl, authCode);
            }
            else if (e.Uri.AbsoluteUri.Contains("lc="))
            {
                browserWindow.Navigate(SignInUrl);
            }
        }

        public async void RequestAccessTokenAsync(string requestUrl, string authCode)
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

                        mvpApiService = new MvpApiService(Constants.SubscriptionKey, cleanAccessToken);

                        GetProfileInfoButton.IsEnabled = isLoggedIn = true;
                    }
                }
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
    }
}
