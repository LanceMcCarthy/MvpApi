using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using MvpApi.Uwp.Common;
using Newtonsoft.Json;

namespace MvpApi.Uwp
{
    public sealed partial class MainPage : Page
    {
        private readonly ApplicationDataContainer localSettings;
        private MvpApiService mvpApiService;
        private bool isLoggedIn;

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

                var logOutUri = new Uri(string.Format($"https://login.live.com/oauth20_logout.srf?client_id={Constants.ClientId}&redirect_uri=https:%2F%2Flogin.live.com%2Foauth20_desktop.srf"));
                browserWindow.Navigate(logOutUri);

                SaveToken("access_token", "");
                SaveToken("refresh_token", "");
            }
            else
            {
                browserWindow.Navigate(Constants.SignInUrl);
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
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
            }
        }

        #endregion
        
        #region OAuth2 Login

        private async void BrowserWindow_LoadCompleted(object sender, NavigationEventArgs e)
        {
            if (e.Uri.AbsoluteUri.Contains("code="))
            {
                var authCode = Regex.Split(e.Uri.AbsoluteUri, "code=")[1];

                SaveToken("auth_code", authCode);

                browserWindow.Visibility = Visibility.Collapsed;

                await RequestAccessTokenAsync(Constants.AccessTokenUrl, authCode);
            }
            else if (e.Uri.AbsoluteUri.Contains("lc="))
            {
                browserWindow.Navigate(Constants.SignInUrl);
            }
        }

        public async Task RequestAccessTokenAsync(string requestUrl, string authCode)
        {
            using (var client = new HttpClient())
            {
                var content = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("client_id", Constants.ClientId),
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("code", authCode.Split('&')[0]),
                    new KeyValuePair<string, string>("redirect_uri", Constants.RedirectUrl),
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

                        mvpApiService = new MvpApiService(Constants.SubscriptionKey, tokenData["access_token"]);

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
