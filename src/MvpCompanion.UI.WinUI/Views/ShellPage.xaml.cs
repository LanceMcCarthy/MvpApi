using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Popups;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.Toolkit.Uwp.Connectivity;
using Microsoft.UI.Xaml.Navigation;
using MvpApi.Common.CustomEventArgs;
using MvpApi.Common.Extensions;
using MvpApi.Services.Apis;
using MvpApi.Services.Utilities;
using MvpCompanion.UI.Common.Helpers;
using Newtonsoft.Json;
//using NavigationView = Microsoft.UI.Xaml.Controls.NavigationView;
//using NavigationViewBackRequestedEventArgs = Microsoft.UI.Xaml.Controls.NavigationViewBackRequestedEventArgs;
//using NavigationViewItem = Microsoft.UI.Xaml.Controls.NavigationViewItem;
//using NavigationViewItemInvokedEventArgs = Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs;
//using NavigationViewItemSeparator = Microsoft.UI.Xaml.Controls.NavigationViewItemSeparator;
//using Page = Microsoft.UI.Xaml.Controls.Page;
//using Symbol = Microsoft.UI.Xaml.Controls.Symbol;
//using SymbolIcon = Microsoft.UI.Xaml.Controls.SymbolIcon;
using Microsoft.UI.Xaml.Controls;

namespace MvpCompanion.UI.WinUI.Views
{
    public sealed partial class ShellPage : Page
    {
        private static readonly string _scope = "wl.emails%20wl.basic%20wl.offline_access%20wl.signin";
        private static readonly string _clientId = "090fa1d9-3d6f-4f6f-a733-a8b8a3fe16ff";
        private readonly string _redirectUrl = "https://login.live.com/oauth20_desktop.srf";
        private readonly string _accessTokenUrl = "https://login.live.com/oauth20_token.srf";

        private readonly Uri _signInUri =
            new Uri($"https://login.live.com/oauth20_authorize.srf?client_id={_clientId}&redirect_uri=https:%2F%2Flogin.live.com%2Foauth20_desktop.srf&response_type=code&scope={_scope}");

        private readonly Uri _signOutUri = new Uri($"https://login.live.com/oauth20_logout.srf?client_id={_clientId}&redirect_uri=https:%2F%2Flogin.live.com%2Foauth20_desktop.srf");

        public static ShellPage Instance { get; set; }

        //public static HamburgerMenu HamburgerMenu => Instance.Menu;
        public static NavigationView Menu = Instance.NavView;

        public ShellPage()
        {
            Instance = this;
            InitializeComponent();
            this.Loaded += ShellPage_Loaded;
        }
        
        private async void ShellPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
            {
                await SignInAsync();
            }
            else
            {
                await new MessageDialog("This application requires an internet connection. Please check your connection and launch the app again.", "No Internet").ShowAsync();
            }
        }

        public async void LogoutButton_OnClick(object sender, RoutedEventArgs e)
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

        public async void WebView_OnNavigationCompleted(Windows.UI.Xaml.Controls.WebView sender, Windows.UI.Xaml.Controls.WebViewNavigationCompletedEventArgs e)
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

        #region Authentication

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
            await BootStrapper.Current.NavigationService.NavigateAsync(typeof(HomePage), null, new SuppressNavigationTransitionInfo());

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

        //private double NavViewCompactModeThresholdWidth
        //{
        //    get { return NavView.CompactModeThresholdWidth; }
        //}

        //private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        //{
        //    throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        //}

        //// List of ValueTuple holding the Navigation Tag and the relative Navigation Page
        //private readonly List<(string Tag, Type Page)> _pages = new List<(string Tag, Type Page)>
        //{
        //    ("Home", typeof(HomePage)),
        //    ("Profile", typeof(ProfilePage)),
        //    ("Kudos", typeof(KudosPage)),
        //    ("Setting", typeof(SettingsPage)),
        //    ("About", typeof(AboutPage)),
        //};

        //private void NavView_Loaded(object sender, RoutedEventArgs e)
        //{

        //}

        //private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        //{
        //    if (args.IsSettingsInvoked == true)
        //    {
        //        NavView_Navigate("settings", args.RecommendedNavigationTransitionInfo);
        //    }
        //    else if (args.InvokedItemContainer != null)
        //    {
        //        var navItemTag = args.InvokedItemContainer.Tag.ToString();
        //        NavView_Navigate(navItemTag, args.RecommendedNavigationTransitionInfo);
        //    }
        //}

        //// NavView_SelectionChanged is not used in this example, but is shown for completeness.
        //// You will typically handle either ItemInvoked or SelectionChanged to perform navigation,
        //// but not both.
        //private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        //{
        //    if (args.IsSettingsSelected == true)
        //    {
        //        NavView_Navigate("settings", args.RecommendedNavigationTransitionInfo);
        //    }
        //    else if (args.SelectedItemContainer != null)
        //    {
        //        var navItemTag = args.SelectedItemContainer.Tag.ToString();
        //        NavView_Navigate(navItemTag, args.RecommendedNavigationTransitionInfo);
        //    }
        //}

        //private void NavView_Navigate(string navItemTag, NavigationTransitionInfo transitionInfo)
        //{
        //    Type _page = null;
        //    if (navItemTag == "settings")
        //    {
        //        _page = typeof(SettingsPage);
        //    }
        //    else
        //    {
        //        var item = _pages.FirstOrDefault(p => p.Tag.Equals(navItemTag));
        //        _page = item.Page;
        //    }

        //    // Get the page type before navigation so you can prevent duplicate
        //    // entries in the backstack.
        //    var preNavPageType = ContentFrame.CurrentSourcePageType;

        //    // Only navigate if the selected page isn't currently loaded.
        //    if (!(_page is null) && !Type.Equals(preNavPageType, _page))
        //    {
        //        ContentFrame.Navigate(_page, null, transitionInfo);
        //    }
        //}

        //private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        //{
        //    TryGoBack();
        //}

        //private void CoreDispatcher_AcceleratorKeyActivated(CoreDispatcher sender, AcceleratorKeyEventArgs e)
        //{
        //    // When Alt+Left are pressed navigate back
        //    if (e.EventType == CoreAcceleratorKeyEventType.SystemKeyDown
        //        && e.VirtualKey == VirtualKey.Left
        //        && e.KeyStatus.IsMenuKeyDown == true
        //        && !e.Handled)
        //    {
        //        e.Handled = TryGoBack();
        //    }
        //}

        //private void System_BackRequested(object sender, BackRequestedEventArgs e)
        //{
        //    if (!e.Handled)
        //    {
        //        e.Handled = TryGoBack();
        //    }
        //}

        //private void CoreWindow_PointerPressed(CoreWindow sender, PointerEventArgs e)
        //{
        //    // Handle mouse back button.
        //    if (e.CurrentPoint.Properties.IsXButton1Pressed)
        //    {
        //        e.Handled = TryGoBack();
        //    }
        //}

        private bool TryGoBack()
        {
            if (!ContentFrame.CanGoBack)
                return false;

            // Don't go back if the nav pane is overlayed.
            if (NavView.IsPaneOpen &&
                (NavView.DisplayMode == Microsoft.UI.Xaml.Controls.NavigationViewDisplayMode.Compact ||
                 NavView.DisplayMode == Microsoft.UI.Xaml.Controls.NavigationViewDisplayMode.Minimal))
                return false;

            ContentFrame.GoBack();
            return true;
        }

        //private void On_Navigated(object sender, NavigationEventArgs e)
        //{
        //    NavView.IsBackEnabled = ContentFrame.CanGoBack;

        //    if (ContentFrame.SourcePageType == typeof(SettingsPage))
        //    {
        //        // SettingsItem is not part of NavView.MenuItems, and doesn't have a Tag.
        //        NavView.SelectedItem = (NavigationViewItem) NavView.SettingsItem;
        //        NavView.Header = "Settings";
        //    }
        //    else if (ContentFrame.SourcePageType != null)
        //    {
        //        var item = _pages.FirstOrDefault(p => p.Page == e.SourcePageType);

        //        NavView.SelectedItem = NavView.MenuItems
        //            .OfType<NavigationViewItem>()
        //            .First(n => n.Tag.Equals(item.Tag));

        //        NavView.Header =
        //            ((NavigationViewItem) NavView.SelectedItem)?.Content?.ToString();
        //    }
        //}

        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            // You can also add items in code.
            NavView.MenuItems.Add(new NavigationViewItemSeparator());
            NavView.MenuItems.Add(new NavigationViewItem
            {
                Content = "My content",
                Icon = new SymbolIcon((Symbol)0xF1AD),
                Tag = "content"
            });
            //_pages.Add(("content", typeof(MyContentPage)));

            // Add handler for ContentFrame navigation.
            ContentFrame.Navigated += Frame_OnNavigated;

            // NavView doesn't load any page by default, so load home page.
            NavView.SelectedItem = NavView.MenuItems[0];
            // If navigation occurs on SelectionChanged, this isn't needed.
            // Because we use ItemInvoked to navigate, we need to call Navigate
            // here to load the home page.
            NavView_Navigate("home", new EntranceNavigationTransitionInfo());

            // Listen to the window directly so the app responds
            // to accelerator keys regardless of which element has focus.
            Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated += CoreDispatcher_AcceleratorKeyActivated;

            Window.Current.CoreWindow.PointerPressed += CoreWindow_PointerPressed;

            SystemNavigationManager.GetForCurrentView().BackRequested += System_BackRequested;
        }

        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            
        }

        private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            
        }

        private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            
        }
    }
}