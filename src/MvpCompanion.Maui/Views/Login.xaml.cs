using System.Text.Json;
using MvpApi.Common.CustomEventArgs;
using MvpApi.Services.Apis;
using MvpApi.Services.Utilities;
using MvpCompanion.Maui.ViewModels;

namespace MvpCompanion.Maui.Views;

public partial class Login : ContentPage
{
    private LoginViewModel _viewModel;
    public Login()
	{
		InitializeComponent();
        _viewModel = new LoginViewModel();
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await SignInAsync();
    }

    #region Authentication

    private static readonly string _scope = "wl.emails%20wl.basic%20wl.offline_access%20wl.signin";
    private static readonly string _clientId = "090fa1d9-3d6f-4f6f-a733-a8b8a3fe16ff";
    private readonly string _redirectUrl = "https://login.live.com/oauth20_desktop.srf";
    private readonly string _accessTokenUrl = "https://login.live.com/oauth20_token.srf";
    private readonly Uri _signInUrl = new($"https://login.live.com/oauth20_authorize.srf?client_id={_clientId}&redirect_uri=https:%2F%2Flogin.live.com%2Foauth20_desktop.srf&response_type=code&scope={_scope}");
    private readonly Uri _signOutUri = new($"https://login.live.com/oauth20_logout.srf?client_id={_clientId}&redirect_uri=https:%2F%2Flogin.live.com%2Foauth20_desktop.srf");

    public async Task SignInAsync()
    {
        var refreshToken = StorageHelpers.Instance.LoadToken("refresh_token");

        // If refresh token is available, the user has previously been logged in and we can get a refreshed access token immediately
        if (!string.IsNullOrEmpty(refreshToken))
        {
            _viewModel.IsBusy = true;
            _viewModel.IsBusyMessage = "refreshing session...";

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

            _viewModel.IsBusy = false;
            _viewModel.IsBusyMessage = "";
        }
        else
        {
            // No stored credentials, use WebView workflow
            LoginUsingWebView();
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
            StorageHelpers.Instance.DeleteToken("access_token");
            StorageHelpers.Instance.DeleteToken("refresh_token");

            // Delete profile photo file
            if (File.Exists((Shell.Current.BindingContext as ShellViewModel).ProfileImagePath))
            {
                (Shell.Current.BindingContext as ShellViewModel).IsBusyMessage = "deleting profile photo file...";
                File.Delete((Shell.Current.BindingContext as ShellViewModel).ProfileImagePath);
            }

            // Clean up profile objects
            _viewModel.IsBusyMessage = "resetting profile...";
            (Shell.Current.BindingContext as ShellViewModel).Mvp = null;
            (Shell.Current.BindingContext as ShellViewModel).ProfileImagePath = "";
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
            (Shell.Current.BindingContext as ShellViewModel).IsLoggedIn = false;

            LoginUsingWebView();
        }
    }

    private void LoginUsingWebView()
    {
        _webView.Source = _signInUrl;
    }

    private async Task InitializeMvpApiAsync(string authorizationHeader)
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

        (Shell.Current.BindingContext as ShellViewModel).IsLoggedIn = true;

        _viewModel.IsBusyMessage = "downloading profile info...";
        (Shell.Current.BindingContext as ShellViewModel).Mvp = await App.ApiService.GetProfileAsync();

        _viewModel.IsBusyMessage = "downloading profile image...";
        (Shell.Current.BindingContext as ShellViewModel).ProfileImagePath = await App.ApiService.DownloadAndSaveProfileImage();


        await Shell.Current.GoToAsync("///home");

        _viewModel.IsBusyMessage = "";
        _viewModel.IsBusy = false;
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

        await DisplayAlert("MVP API Request Error", message, "ok");
    }

    // WebView logic

    private async void WebView_OnNavigated(object sender, WebNavigatedEventArgs e)
    {
        switch (e.Result)
        {
            case WebNavigationResult.Success:
                if (e.Url.Contains("code="))
                {
                    // old
                    //var myUri = new Uri(e.Url);
                    //var authCode = myUri.ExtractQueryValue("code");

                    // cross platform safe
                    var queryString = e.Url.Split('?')[1];
                    var queryDictionary = System.Web.HttpUtility.ParseQueryString(queryString);
                    var authCode = queryDictionary["code"];

                    await RequestAuthorizationAsync(authCode);
                }
                else if (e.Url.Contains("lc="))
                {
                    // Redirect to signin page if there's a bounce
                    _webView.Source = _signInUrl;
                }
                break;
            case WebNavigationResult.Failure:
                break;
            case WebNavigationResult.Timeout:
                break;
            case WebNavigationResult.Cancel:
                break;
            default:
                break;
        }

    }

    private async Task<string> RequestAuthorizationAsync(string authCode, bool isRefresh = false)
    {
        try
        {
            using (var client = new HttpClient())
            {
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
                    StorageHelpers.Instance.StoreToken("access_token", tokenData["access_token"]);
                    StorageHelpers.Instance.StoreToken("refresh_token", tokenData["refresh_token"]);

                    // We need to prefix the access token with the token type for the auth header. 
                    // Currently this is always "bearer", doing this to be more future proof
                    var tokenType = tokenData["token_type"];
                    var cleanedAccessToken = tokenData["access_token"].Split('&')[0];

                    // set public property that is "returned"
                    return $"{tokenType} {cleanedAccessToken}";
                }
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