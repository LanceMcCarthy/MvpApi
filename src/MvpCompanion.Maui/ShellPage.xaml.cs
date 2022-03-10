using MvpApi.Common.CustomEventArgs;
using MvpApi.Services.Apis;
using MvpApi.Services.Utilities;
using MvpCompanion.Maui.ViewModels;
using MvpCompanion.Maui.Views;
using System.Text.Json;
using MvpCompanion.Maui.Services;

namespace MvpCompanion.Maui;

public partial class ShellPage : Shell
{
    private readonly ShellViewModel _viewModel;

    private readonly INotificationService notificationService;

    public ShellPage()
	{
		InitializeComponent();
        _viewModel = new ShellViewModel();
        BindingContext = _viewModel;

        notificationService = MvpCompanion.Maui.Services.ServiceProvider.Current.GetService<INotificationService>();
        
        if (Device.Idiom == TargetIdiom.Phone)
        {
            CurrentItem = PhoneTabs;
            SelectView("home");
        }

        Routing.RegisterRoute("login", typeof(Login));
        Routing.RegisterRoute("home", typeof(Views.Home));
        Routing.RegisterRoute("home/detail", typeof(Detail));
        Routing.RegisterRoute("upload", typeof(Upload));
        Routing.RegisterRoute("account", typeof(Profile));
        Routing.RegisterRoute("help", typeof(Help));
        Routing.RegisterRoute("settings", typeof(Settings));
        Routing.RegisterRoute("settings/about", typeof(About));
    }

    public void SelectView(string viewName)
    {
        if (Device.Idiom == TargetIdiom.Phone)
        {
            switch (viewName)
            {
                case "home":
                    CurrentItem = HomeTab;
                    break;
                case "upload":
                    CurrentItem = UploadTab;
                    break;
                case "account":
                    CurrentItem = AccountTab;
                    break;
                case "settings":
                    CurrentItem = SettingsTab;
                    break;
            }
        }
        else
        {
            switch (viewName)
            {
                case "home":
                    CurrentItem = HomeFlyoutItem;
                    break;
                case "upload":
                    CurrentItem = UploadFlyoutItem;
                    break;
                case "account":
                    CurrentItem = AccountFlyoutItem;
                    break;
                case "settings":
                    CurrentItem = SettingsFlyoutItem;
                    break;
            }
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var userAuthRefreshed = await SignInAsync();

        if (userAuthRefreshed)
        {
            notificationService.ShowNotification("Logged in...", "Logged in!");

            if (Device.Idiom == TargetIdiom.Phone)
            {
                CurrentItem = HomeTab;
            }
            else
            {
                CurrentItem = HomeFlyoutItem;
            }
        }
        else
        {
            notificationService.ShowNotification("Logging in...", "You need to login.");

            await GoToAsync("login?operation=signin");
        }
    }
    
    private void TapGestureRecognizer_Tapped(Object sender, EventArgs e)
    {
        GoToAsync("///settings");
    }
    
    #region Authentication
    
    public async Task<bool> SignInAsync()
    {
        try
        {
            
            var refreshToken = Preferences.Get("refresh_token", "");
            //var refreshToken = StorageHelpers.Instance.LoadToken("refresh_token");

            // If refresh token is available, the user has previously been logged in and we can get a refreshed access token immediately
            if (!string.IsNullOrEmpty(refreshToken))
            {
                _viewModel.IsBusy = true;
                _viewModel.IsBusyMessage = "refreshing session...";

                var authorizationHeader = await RequestAuthorizationAsync(refreshToken, true);

                if (!string.IsNullOrEmpty(authorizationHeader))
                {
                    await InitializeMvpApiAsync(authorizationHeader);
                    return true;
                }
            }

            return false;
        }
        catch (Exception e)
        {
            await e.LogExceptionAsync();
            throw;
        }
        finally
        {
            _viewModel.IsBusy = false;
            _viewModel.IsBusyMessage = "";
        }
    }

    public async Task SignOutAsync()
    {
        try
        {
            // Indicate to user we are signing out
            _viewModel.IsBusy = true;
            _viewModel.IsBusyMessage = "logging out...";

            // Erase cached tokens
            _viewModel.IsBusyMessage = "deleting cache files...";
            //StorageHelpers.Instance.DeleteToken("access_token");
            //StorageHelpers.Instance.DeleteToken("refresh_token");
            Preferences.Set("access_token", "");
            Preferences.Set("refresh_token", "");

            // Delete profile photo file
            if (File.Exists((Current.BindingContext as ShellViewModel).ProfileImagePath))
            {
                (Current.BindingContext as ShellViewModel).IsBusyMessage = "deleting profile photo file...";
                File.Delete((Current.BindingContext as ShellViewModel).ProfileImagePath);
            }

            // Clean up profile objects
            _viewModel.IsBusyMessage = "resetting profile...";
            (Current.BindingContext as ShellViewModel).Mvp = null;
            (Current.BindingContext as ShellViewModel).ProfileImagePath = "";
        }
        catch (Exception ex)
        {
            await ex.LogExceptionAsync();
            await DisplayAlert("Sign out Error", ex.Message, "ok");
        }
        finally
        {
            // Hide busy indicator
            _viewModel.IsBusy = false;
            _viewModel.IsBusyMessage = "";

            // Toggle flag
            (Current.BindingContext as ShellViewModel).IsLoggedIn = false;

            await GoToAsync("login?operation=signout");
        }
    }
    
    public async Task InitializeMvpApiAsync(string authorizationHeader)
    {
        _viewModel.IsBusy = true;
        _viewModel.IsBusyMessage = "authenticating...";

        // remove any previously wired up event handlers
        if (App.ApiService != null)
        {
            App.ApiService.AccessTokenExpired -= ApiService_AccessTokenExpired;
            App.ApiService.RequestErrorOccurred -= ApiService_RequestErrorOccurred;
        }

        App.ApiService = new MvpApiService(authorizationHeader);

        App.ApiService.AccessTokenExpired += ApiService_AccessTokenExpired;
        App.ApiService.RequestErrorOccurred += ApiService_RequestErrorOccurred;

        (Current.BindingContext as ShellViewModel).IsLoggedIn = true;

        _viewModel.IsBusyMessage = "downloading profile info...";
        (Current.BindingContext as ShellViewModel).Mvp = await App.ApiService.GetProfileAsync();

        _viewModel.IsBusyMessage = "downloading profile image...";
        (Current.BindingContext as ShellViewModel).ProfileImagePath = await App.ApiService.DownloadAndSaveProfileImage();

        // Navigation is now performed where ever the initialization request was called from.
        // This allows for individually determining if navigation is needed
        // await Shell.Current.GoToAsync("home");

        _viewModel.IsBusyMessage = "";
        _viewModel.IsBusy = false;
    }

    // WebView logic


    private readonly string _redirectUrl = "https://login.live.com/oauth20_desktop.srf";
    private readonly string _accessTokenUrl = "https://login.live.com/oauth20_token.srf";
    private static readonly string _clientId = "090fa1d9-3d6f-4f6f-a733-a8b8a3fe16ff";

    public async Task<string> RequestAuthorizationAsync(string authCode, bool isRefresh = false)
    {
        try
        {
            using var client = new HttpClient();

            // Construct the Form content, this is where I add the OAuth token (could be access token or refresh token)
            var postContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
            {
                new("client_id", _clientId),
                new("grant_type", isRefresh ? "refresh_token" : "authorization_code"),
                new(isRefresh ? "refresh_token" : "code", authCode.Split('&')[0]),
                new("redirect_uri", _redirectUrl)
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
            var tokenData = JsonSerializer.Deserialize<Dictionary<string, string>>(responseTxt);

            if (tokenData.ContainsKey("access_token"))
            {
                //StorageHelpers.Instance.StoreToken("access_token", tokenData["access_token"]);
                //StorageHelpers.Instance.StoreToken("refresh_token", tokenData["refresh_token"]);
                Preferences.Set("access_token", tokenData["access_token"]);
                Preferences.Set("refresh_token", tokenData["access_token"]);

                // We need to prefix the access token with the token type for the auth header. 
                // Currently this is always "bearer", doing this to be more future proof
                var tokenType = tokenData["token_type"];
                var cleanedAccessToken = tokenData["access_token"].Split('&')[0];

                // set public property that is "returned"
                return $"{tokenType} {cleanedAccessToken}";
            }
        }
        catch (HttpRequestException e)
        {
            await e.LogExceptionAsync();
            await DisplayAlert("Error", $"Something went wrong signing you in, try again. {e.Message}", "ok");
        }
        catch (Exception e)
        {
            await e.LogExceptionAsync();
            await DisplayAlert("Error", $"Something went wrong signing you in, try again. {e.Message}", "ok");
        }

        return null;
    }

    private async void ApiService_AccessTokenExpired(object sender, ApiServiceEventArgs e)
    {
        if (e.IsTokenRefreshNeeded)
        {
            var userAuthRefreshed = await SignInAsync();

            if (!userAuthRefreshed)
            {
                await GoToAsync("login?signin");
            }
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

        await DisplayAlert("MVP API Request Error", message, "ok");
    }

    #endregion
}
