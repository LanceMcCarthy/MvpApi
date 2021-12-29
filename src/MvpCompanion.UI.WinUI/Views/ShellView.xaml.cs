using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using MvpApi.Common.Models;
using MvpApi.Common.Models.Navigation;
using MvpApi.Services.Utilities;
using MvpCompanion.UI.WinUI.Dialogs;

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

        SelectTab(ViewType.Home);
    }

    private void OnLoginCompleted()
    {
        ViewModel.RefreshProperties();
    }

    private void ShellView_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        ViewModel.OnUnloaded();
    }

    public void SelectTab(ViewType viewType)
    {
        var desiredTab = ShellTabView.TabItems.FirstOrDefault(t => (t as TabViewItem)?.Tag.ToString() == viewType.ToString());

        if (desiredTab != null)
        {
            ShellTabView.SelectedItem = desiredTab;
        }
    }

    private async void ShellTabView_OnTabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
    {
        if (args.Item is TabViewItem tab && (ViewType)tab.Tag == ViewType.Detail)
        {
            sender.TabItems.Remove(args.Item);
        }
        else
        {
            await App.ShowMessageAsync("Only dynamically-added (contribution detail) tabs can be removed.", "Non-removable Tab");
        }
    }

    public void AddDetailTab(ContributionsModel contribution)
    {
        var tvi = new TabViewItem
        {
            Header = contribution.ContributionId,
            Tag = ViewType.Detail,
            Content = new ContributionDetailView { Contribution = contribution }
        };

        ShellTabView.TabItems.Insert(1, tvi);
    }
}