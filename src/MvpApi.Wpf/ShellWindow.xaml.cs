using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Windows.Storage;
using Windows.UI.Popups;
using MvpApi.Common.CustomEventArgs;
using MvpApi.Common.Extensions;
using MvpApi.Services.Apis;
using MvpApi.Services.Utilities;
using MvpApi.Wpf.Helpers;
using MvpApi.Wpf.Models;
using Newtonsoft.Json;

namespace MvpApi.Wpf
{
    public partial class ShellWindow : Window
    {
        public ShellWindow()
        {
            if (!string.IsNullOrEmpty(Properties.Settings.Default.PreferredTheme))
            {
                UpdateTheme(Properties.Settings.Default.PreferredTheme);
            }

            InitializeComponent();

            Loaded += ShellWindow_Loaded;
        }

        private async void ShellWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (NetworkHelper.Current.CheckInternetConnection())
            {
                await SignInAsync();
            }
            else
            {
                await new MessageDialog("This application requires an internet connection. Please check your connection and launch the app again.", "No Internet").ShowAsync();
            }
        }

        private void RootNavigationView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (e.AddedItems != null && e.AddedItems[0] is RadNavigationViewItem menuItem)
            //{
            //    ChangeView(menuItem.Tag.ToString());
            //}
        }

        private void ChangeView(ViewName viewName)
        {
            RootNavigationView.SelectedIndex = (int)viewName;
        }

        private async void LogoutButton_OnClick(object sender, RoutedEventArgs e)
        {
            var md = new MessageDialog("Do you wish to sign out?");
            md.Commands.Add(new UICommand("Logout"));
            md.Commands.Add(new UICommand("Cancel"));

            var result = await md.ShowAsync();

            if (result.Label == "Logout")
            {
                await SignOutAsync();
            }
        }

        #region Authentication and WebView

        private static readonly string _scope = "wl.emails%20wl.basic%20wl.offline_access%20wl.signin";
        private static readonly string _clientId = "090fa1d9-3d6f-4f6f-a733-a8b8a3fe16ff";
        private readonly string _redirectUrl = "https://login.live.com/oauth20_desktop.srf";
        private readonly string _accessTokenUrl = "https://login.live.com/oauth20_token.srf";
        private readonly Uri _signInUri = new Uri($"https://login.live.com/oauth20_authorize.srf?client_id={_clientId}&redirect_uri=https:%2F%2Flogin.live.com%2Foauth20_desktop.srf&response_type=code&scope={_scope}");
        private readonly Uri _signOutUri = new Uri($"https://login.live.com/oauth20_logout.srf?client_id={_clientId}&redirect_uri=https:%2F%2Flogin.live.com%2Foauth20_desktop.srf");


        private async void WebBrowser_OnNavigated(object sender, NavigationEventArgs e)
        {
            if (e.Uri.AbsoluteUri.Contains("code="))
            {
                // get token
                var authCode = e.Uri.ExtractQueryValue("code");

                var authorizationHeader = await RequestAuthorizationAsync(authCode);

                await InitializeMvpApiAsync(authorizationHeader);
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
                ViewModel.IsBusy = true;
                ViewModel.IsBusyMessage = "refreshing session...";

                var authorizationHeader = await RequestAuthorizationAsync(refreshToken, true);

                if (string.IsNullOrEmpty(authorizationHeader))
                {
                    // something went wrong using refresh token, use WebView to login
                    LoginUsingWebView();
                }
                else
                {
                    await InitializeMvpApiAsync(authorizationHeader);
                }

                ViewModel.IsBusy = false;
                ViewModel.IsBusyMessage = "";
            }
            else
            {
                // No stored credentials, use WebView workflow
                LoginUsingWebView();
            }
        }

        private void LoginUsingWebView()
        {
            // Make sure the overlay is visible
            if (LoginOverlay.Visibility == Visibility.Collapsed)
            {
                LoginOverlay.Visibility = Visibility.Visible;
            }

            // Start OAuth flow
            AuthWebView.Source = _signInUri;
        }

        private async Task InitializeMvpApiAsync(string authorizationHeader)
        {
            ViewModel.IsBusy = true;
            ViewModel.IsBusyMessage = "refreshing session...";

            // Hide overlay if it's still visible
            if (LoginOverlay.Visibility == Visibility.Visible)
            {
                LoginOverlay.Visibility = Visibility.Collapsed;
            }

            // remove any previously wired up event handlers
            if (App.ApiService != null)
            {
                App.ApiService.AccessTokenExpired -= ApiService_AccessTokenExpired;
                App.ApiService.RequestErrorOccurred -= ApiService_RequestErrorOccurred;
            }

            // New-up the service
            App.ApiService = new MvpApiService(authorizationHeader);

            App.ApiService.AccessTokenExpired += ApiService_AccessTokenExpired;
            App.ApiService.RequestErrorOccurred += ApiService_RequestErrorOccurred;

            // Trigger UI changes (e.g. hide the overlay)
            ViewModel.IsLoggedIn = true;

            // Get MVP profile
            ViewModel.IsBusyMessage = "downloading profile info...";
            ViewModel.Mvp = await App.ApiService.GetProfileAsync();

            // Get MVP profile image
            ViewModel.IsBusyMessage = "downloading profile image...";
            ViewModel.ProfileImagePath = await App.ApiService.DownloadAndSaveProfileImage();

            //Navigate to the home page
            ChangeView(ViewName.Home);

            ViewModel.IsBusy = false;
            ViewModel.IsBusyMessage = "";
        }

        public async Task SignOutAsync()
        {
            try
            {
                // Indicate to user we are signing out
                ViewModel.IsBusy = true;
                ViewModel.IsBusyMessage = "logging out...";

                // Erase cached tokens
                ViewModel.IsBusyMessage = "deleting cache files...";
                StorageHelpers.Instance.DeleteToken("access_token");
                StorageHelpers.Instance.DeleteToken("refresh_token");

                // Clean up profile objects
                ViewModel.IsBusyMessage = "resetting profile...";
                ViewModel.Mvp = null;
                ViewModel.ProfileImagePath = "";

                // Delete profile photo file
                ViewModel.IsBusyMessage = "deleting profile photo file...";
                var imageFile = await ApplicationData.Current.LocalFolder.TryGetItemAsync("ProfilePicture.jpg");
                if (imageFile != null) await imageFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
            catch (Exception ex)
            {
                await ex.LogExceptionWithUserMessage();
            }
            finally
            {
                // Hide busy indicator
                ViewModel.IsBusy = false;
                ViewModel.IsBusyMessage = "";

                // Toggle flag
                ViewModel.IsLoggedIn = false;

                // Make sure the overlay is visible
                if (LoginOverlay.Visibility == Visibility.Collapsed)
                {
                    LoginOverlay.Visibility = Visibility.Visible;
                }

                // Start auth workflow again (the logout Uri redirects to sign in again)
                AuthWebView.Source = _signOutUri;
            }
        }

        private async Task<string> RequestAuthorizationAsync(string authCode, bool isRefresh = false)
        {
            var authorizationHeader = "";

            try
            {
                using (var client = new HttpClient())
                {
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
            }
            catch (HttpRequestException e)
            {
                await e.LogExceptionWithUserMessage();

                if (e.Message.Contains("401"))
                {
                    //TODO consider another message HTTP specific errors
                }

                Debug.WriteLine($"LoginDialog HttpRequestException: {e}");
            }
            catch (Exception e)
            {
                await e.LogExceptionWithUserMessage();
            }

            return authorizationHeader;
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

        #endregion

        public void UpdateTheme(string assemblyName)
        {
            Application.Current.Resources.MergedDictionaries.Clear();

            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri($"/Telerik.Windows.Themes.{assemblyName};component/Themes/System.Windows.xaml", UriKind.RelativeOrAbsolute)
            });

            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri($"/Telerik.Windows.Themes.{assemblyName};component/Themes/Telerik.Windows.Controls.xaml", UriKind.RelativeOrAbsolute)
            });

            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri($"/Telerik.Windows.Themes.{assemblyName};component/Themes/Telerik.Windows.Controls.Data.xaml", UriKind.RelativeOrAbsolute)
            });

            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri($"/Telerik.Windows.Themes.{assemblyName};component/Themes/Telerik.Windows.Controls.GridView.xaml", UriKind.RelativeOrAbsolute)
            });

            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri($"/Telerik.Windows.Themes.{assemblyName};component/Themes/Telerik.Windows.Controls.Input.xaml", UriKind.RelativeOrAbsolute)
            });

            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri($"/Telerik.Windows.Themes.{assemblyName};component/Themes/Telerik.Windows.Controls.Navigation.xaml", UriKind.RelativeOrAbsolute)
            });
            
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("Styles/GeneralStyles.xaml", UriKind.RelativeOrAbsolute)
            });
        }
    }
}
