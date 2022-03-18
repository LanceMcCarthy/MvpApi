using MvpApi.Common.CustomEventArgs;
using MvpApi.Services.Apis;

namespace MvpCompanion.Maui;

public partial class App : Application
{
	public static MvpApiService ApiService { get; private set; }

	public App()
	{
		InitializeComponent();
        
        UserAppTheme = Preferences.Get("IsDarkMode", false) ? AppTheme.Dark : AppTheme.Light;

		MainPage = new ShellPage();
    }

    protected override void OnStart()
    {
    }

    protected override void OnSleep()
    {
    }

    protected override void OnResume()
    {
    }

    public static void StartupApiService(string authorizationHeader)
    {
        if (ApiService != null)
        {
            ApiService.AccessTokenExpired -= ApiService_AccessTokenExpired;
            ApiService.RequestErrorOccurred -= ApiService_RequestErrorOccurred;
            ApiService = null;
        }

        ApiService = new MvpApiService(authorizationHeader);

        ApiService.AccessTokenExpired += ApiService_AccessTokenExpired;
        ApiService.RequestErrorOccurred += ApiService_RequestErrorOccurred;
    }

    internal static async void ApiService_AccessTokenExpired(object sender, ApiServiceEventArgs e)
    {
        if (e.IsTokenRefreshNeeded)
        {
            //var userAuthRefreshed = await (Current.MainPage as ShellPage).SignInAsync();

            var refreshToken = Preferences.Get("refresh_token", "");
            //var refreshToken = StorageHelpers.Instance.LoadToken("refresh_token");

            // If refresh token is available, we can get a refreshed access token immediately
            if (!string.IsNullOrEmpty(refreshToken))
            {
                var authorizationHeader = await (App.Current.MainPage as ShellPage).RequestAuthorizationAsync(refreshToken, true);

                if (!string.IsNullOrEmpty(authorizationHeader))
                {
                    ApiService.UpdateAuthorizationHeader(authorizationHeader);
                }
                else
                {
                    await Shell.Current.GoToAsync("login?signin");
                }
            }
            else
            {
                await Shell.Current.GoToAsync("login?signin");
            }
        }
    }

    internal static async void ApiService_RequestErrorOccurred(object sender, ApiServiceEventArgs e)
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

        await Shell.Current.DisplayAlert("MVP API Request Error", message, "ok");
    }
}
