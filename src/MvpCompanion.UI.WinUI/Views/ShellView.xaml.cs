using Microsoft.UI.Xaml.Controls;
using Windows.Foundation.Metadata;
using Microsoft.UI.Xaml.Input;
using MvpApi.Services.Utilities;
using MvpCompanion.UI.WinUI.Dialogs;
using Telerik.UI.Xaml;

namespace MvpCompanion.UI.WinUI.Views;

public sealed partial class ShellView : UserControl
{
    public static ShellView Instance { get; set; }

    public LoginDialog LoginDialog { get; set; }

    public ShellView()
    {
        Instance = this;

        InitializeComponent();

        Loaded += ShellView_Loaded;
        Unloaded += ShellView_Unloaded;
    }

    private void RadRibbonView_OnHelpRequested(object? sender, RadRoutedEventArgs e)
    {

    }

    private async void ShellView_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        LoginDialog = new LoginDialog(OnLoginCompleted);
        LoginDialog.XamlRoot = App.CurrentWindow.Content.XamlRoot;

        var refreshToken = StorageHelpers.Instance.LoadToken("refresh_token");

        // We have a refresh token from a previous session
        if (!string.IsNullOrEmpty(refreshToken))
        {
            // Use the refresh token to get a new access token
            var authorizationHeader = await LoginDialog.RequestAuthorizationAsync(refreshToken);

            // If the bearer token was returned, login
            if (!string.IsNullOrEmpty(authorizationHeader))
            {
                await LoginDialog.InitializeMvpApiAsync(authorizationHeader);

                ViewModel.OnLoaded();

                return;
            }
        }

        // all other cases fall down to needing the user to sign back in
        await LoginDialog.SignInAsync();

        ViewModel?.OnLoaded();

        LoadView(new HomeView());
    }

    private void OnLoginCompleted()
    {
        ViewModel.RefreshProperties();
    }

    private void ShellView_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        ViewModel.OnUnloaded();
    }

    private void AboutTab_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        LoadView(new AboutView());
    }

    public void LoadView(UserControl view)
    {
        MainPresenter.Content = view;
    }
}