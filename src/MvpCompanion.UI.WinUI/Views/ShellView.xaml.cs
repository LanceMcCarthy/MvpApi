using Microsoft.UI.Xaml.Controls;
using System.Threading.Tasks;
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
        this.InitializeComponent();
        LoginDialog.XamlRoot = this.XamlRoot;

        this.Loaded += ShellView_Loaded;
    }
    
    private void RadRibbonView_OnHelpRequested(object? sender, RadRoutedEventArgs e)
    {

    }

    private async void ShellView_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var refreshToken = StorageHelpers.Instance.LoadToken("refresh_token");

        // We have a refresh token from a previous session
        if (!string.IsNullOrEmpty(refreshToken))
        {
            // Use the refresh token to get a new access token
            var authorizationHeader = await LoginDialog.RequestAuthorizationAsync(refreshToken);

            // If the bearer token was returned, login
            if (!string.IsNullOrEmpty(authorizationHeader))
            {
                await this.LoginDialog.InitializeMvpApiAsync(authorizationHeader);

                return;
            }
        }

        // all other cases fall down to needing the user to sign back in
        await this.LoginDialog.SignInAsync();
    }
}