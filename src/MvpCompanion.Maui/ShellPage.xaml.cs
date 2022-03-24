using MvpApi.Services.Utilities;
using MvpCompanion.Maui.Models.Authentication;
using MvpCompanion.Maui.Services;
using MvpCompanion.Maui.ViewModels;
using MvpCompanion.Maui.Views;

namespace MvpCompanion.Maui;

public partial class ShellPage : Shell
{
    private readonly ShellViewModel _viewModel;
    private readonly INotificationService notificationService;

    public ShellPage()
    {
        notificationService = Services.ServiceProvider.Current.GetService<INotificationService>();

        InitializeComponent();
        RegisterRoutes();

        if (DeviceInfo.Idiom == DeviceIdiom.Phone || DeviceInfo.Idiom == DeviceIdiom.Tablet)
        {
            CurrentItem = PhoneTabs;
        }

        _viewModel = new ShellViewModel();
        BindingContext = _viewModel;
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

    public async Task SignOutAsync()
    {
        try
        {
            // Indicate to user we are signing out
            _viewModel.IsBusy = true;
            _viewModel.IsBusyMessage = "logging out...";

            await AuthHelpers.ClearAuthorizationAsync();

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

    #endregion
}