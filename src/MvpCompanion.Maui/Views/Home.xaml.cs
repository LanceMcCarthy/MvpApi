using MvpCompanion.Maui.ViewModels;
using MvpApi.Services.Utilities;
using MvpCompanion.Maui.Models.Authentication;

namespace MvpCompanion.Maui.Views;

public partial class Home : ContentPage
{
    private HomeViewModel _viewModel;

	public Home()
	{
		InitializeComponent();
        this.BindingContext = _viewModel = new HomeViewModel();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if(App.ApiService != null && App.ApiService.IsLoggedIn && _viewModel.Contributions.Count == 0)
        {
            await _viewModel.RefreshContributionsAsync();
            return;
        }

        var userAuthRefreshed = await SignInAsync();

        if (userAuthRefreshed)
        {
            //notificationService.ShowNotification("Logged in...", "Logged in!");

            await _viewModel.RefreshContributionsAsync();
        }
        else
        {
            //notificationService.ShowNotification("Logging in...", "You need to login.");

            await Shell.Current.GoToAsync("login?operation=signin");
        }
    }

    public async Task<bool> SignInAsync()
    {
        try
        {
            // First, we check if the user has previously authenticated
            var refreshToken = Preferences.Get("refresh_token", "");
            //var refreshToken = StorageHelpers.Instance.LoadToken("refresh_token");

            // If refresh token is available, we can get a refreshed access token immediately
            if (!string.IsNullOrEmpty(refreshToken))
            {
                _viewModel.IsBusy = true;
                _viewModel.IsBusyMessage = "refreshing session...";

                var authorizationHeader = await AuthHelpers.RequestAuthorizationAsync(refreshToken, true);

                if (!string.IsNullOrEmpty(authorizationHeader))
                {
                    _viewModel.IsBusy = true;
                    _viewModel.IsBusyMessage = "authenticating...";

                    App.StartupApiService(authorizationHeader);

                    (Shell.Current.BindingContext as ShellViewModel).IsLoggedIn = true;

                    _viewModel.IsBusyMessage = "downloading profile info...";
                    (Shell.Current.BindingContext as ShellViewModel).Mvp = await App.ApiService.GetProfileAsync();

                    _viewModel.IsBusyMessage = "downloading profile photo...";
                    (Shell.Current.BindingContext as ShellViewModel).ProfileImagePath = await App.ApiService.DownloadAndSaveProfileImage();

                    _viewModel.IsBusyMessage = "";
                    _viewModel.IsBusy = false;

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

}