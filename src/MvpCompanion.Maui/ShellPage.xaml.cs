using MvpApi.Services.Utilities;
using MvpCompanion.Maui.Models.Authentication;
using MvpCompanion.Maui.Services;
using MvpCompanion.Maui.ViewModels;
using MvpCompanion.Maui.Views;
using System.Text.Json;

namespace MvpCompanion.Maui;

public partial class ShellPage : Shell
{
    private readonly ShellViewModel _viewModel;
    private readonly INotificationService notificationService;
    private readonly string _redirectUrl = "https://login.live.com/oauth20_desktop.srf";
    private readonly string _accessTokenUrl = "https://login.live.com/oauth20_token.srf";
    private static readonly string _clientId = "090fa1d9-3d6f-4f6f-a733-a8b8a3fe16ff";

    public ShellPage()
    {
        InitializeComponent();
        RegisterRoutes();

        _viewModel = new ShellViewModel();
        BindingContext = _viewModel;

        notificationService = Services.ServiceProvider.Current.GetService<INotificationService>();

        if (DeviceInfo.Idiom == DeviceIdiom.Phone || DeviceInfo.Idiom == DeviceIdiom.Tablet)
        {
            CurrentItem = PhoneTabs;
        }
    }

    private void RegisterRoutes()
    {
        Routing.RegisterRoute("login", typeof(Login));
        Routing.RegisterRoute("home", typeof(Home));
        Routing.RegisterRoute("home/login", typeof(Login));
        Routing.RegisterRoute("home/detail", typeof(Detail));
        Routing.RegisterRoute("upload", typeof(Upload));
        Routing.RegisterRoute("account", typeof(Profile));
        Routing.RegisterRoute("help", typeof(Help));
        Routing.RegisterRoute("settings", typeof(Settings));
        Routing.RegisterRoute("settings/about", typeof(About));
    }

    public void SelectView(string viewName)
    {
        //DeviceIdiom.Phone – Phone
        //DeviceIdiom.Tablet – Tablet
        //DeviceIdiom.Desktop – Desktop
        //DeviceIdiom.TV – TV
        //DeviceIdiom.Watch – Watch
        //DeviceIdiom.Unknown – Unknown

        if (DeviceInfo.Idiom == DeviceIdiom.Phone || DeviceInfo.Idiom == DeviceIdiom.Tablet)
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

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (DeviceInfo.Idiom == DeviceIdiom.Phone || DeviceInfo.Idiom == DeviceIdiom.Tablet)
        {
            CurrentItem = HomeTab;
        }
        else
        {
            CurrentItem = HomeFlyoutItem;
        }

        //var userAuthRefreshed = await SignInAsync();

        //if (userAuthRefreshed)
        //{
        //    //notificationService.ShowNotification("Logged in...", "Logged in!");

        //    if (DeviceInfo.Idiom == DeviceIdiom.Phone || DeviceInfo.Idiom == DeviceIdiom.Tablet)
        //    {
        //        CurrentItem = HomeTab;
        //    }
        //    else
        //    {
        //        CurrentItem = HomeFlyoutItem;
        //    }
        //}
        //else
        //{
        //    //notificationService.ShowNotification("Logging in...", "You need to login.");

        //    await GoToAsync("login?operation=signin");
        //}
    }

    private void TapGestureRecognizer_Tapped(Object sender, EventArgs e)
    {
        GoToAsync("///settings");
    }

    #region Authentication

    //public async Task<bool> SignInAsync()
    //{
    //    try
    //    {
    //        // First, we check if the user has previously authenticated
    //        var refreshToken = Preferences.Get("refresh_token", "");
    //        //var refreshToken = StorageHelpers.Instance.LoadToken("refresh_token");

    //        // If refresh token is available, we can get a refreshed access token immediately
    //        if (!string.IsNullOrEmpty(refreshToken))
    //        {
    //            _viewModel.IsBusy = true;
    //            _viewModel.IsBusyMessage = "refreshing session...";

    //            var authorizationHeader = await RequestAuthorizationAsync(refreshToken, true);

    //            if (!string.IsNullOrEmpty(authorizationHeader))
    //            {
    //                _viewModel.IsBusy = true;
    //                _viewModel.IsBusyMessage = "authenticating...";

    //                App.StartupApiService(authorizationHeader);

    //                _viewModel.IsLoggedIn = true;

    //                _viewModel.IsBusyMessage = "downloading profile info...";
    //                _viewModel.Mvp = await App.ApiService.GetProfileAsync();

    //                _viewModel.IsBusyMessage = "downloading profile image...";
    //                _viewModel.ProfileImagePath = await App.ApiService.DownloadAndSaveProfileImage();

    //                _viewModel.IsBusyMessage = "";
    //                _viewModel.IsBusy = false;

    //                return true;
    //            }
    //        }

    //        return false;
    //    }
    //    catch (Exception e)
    //    {
    //        await e.LogExceptionAsync();
    //        throw;
    //    }
    //    finally
    //    {
    //        _viewModel.IsBusy = false;
    //        _viewModel.IsBusyMessage = "";
    //    }
    //}

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
                _viewModel.IsBusyMessage = "deleting profile photo file...";
                File.Delete((Current.BindingContext as ShellViewModel).ProfileImagePath);
            }
        }
        catch (Exception ex)
        {
            await ex.LogExceptionAsync();
            await DisplayAlert("Sign out Error", ex.Message, "ok");
        }
        finally
        {
            _viewModel.Mvp = null;
            _viewModel.ProfileImagePath = string.Empty;
            _viewModel.IsLoggedIn = false;
            _viewModel.IsBusy = false;

            _viewModel.IsBusyMessage = "";

            await GoToAsync("login?operation=signout");
        }
    }

    public async Task<string> RequestAuthorizationAsync(string authCode, bool isRefresh = false)
    {
        try
        {
            using var client = new HttpClient();

            var postData = new List<KeyValuePair<string, string>>
            {
                new("client_id", _clientId),
                new("grant_type", isRefresh ? "refresh_token" : "authorization_code"),
                new(isRefresh ? "refresh_token" : "code", authCode.Split('&')[0]),
                new("redirect_uri", _redirectUrl)
            };

            // Construct the Form content, this is where I add the OAuth token (could be access token or refresh token)
            var postContent = new FormUrlEncodedContent(postData);

            // post the Form data
            using var response = await client.PostAsync(new Uri(_accessTokenUrl), postContent);

            // Read the response
            var responseTxt = await response.Content.ReadAsStringAsync();

            // Deserialize the parameters from the response
            var tokenData = JsonSerializer.Deserialize<AuthResponse>(responseTxt, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (!string.IsNullOrEmpty(tokenData.AccessToken))
            {
                //StorageHelpers.Instance.StoreToken("access_token", tokenData["access_token"]);
                //StorageHelpers.Instance.StoreToken("refresh_token", tokenData["refresh_token"]);
                Preferences.Set("access_token", tokenData.AccessToken);
                Preferences.Set("refresh_token", tokenData.RefreshToken);

                // We need to prefix the access token with the token type for the auth header. 
                // Currently this is always "bearer", doing this to be more future proof
                var tokenType = tokenData.TokenType;
                var cleanedAccessToken = tokenData.AccessToken.Split('&')[0];

                // set public property that is "returned"
                return $"{tokenType} {cleanedAccessToken}";
            }
            else
            {
                await Shell.Current.DisplayAlert("Unauthorized", "The account you signed in with did not provide an authorization code.", "ok");
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

    #endregion
}