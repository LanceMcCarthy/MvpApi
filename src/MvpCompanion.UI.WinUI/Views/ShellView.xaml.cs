using System;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MvpApi.Common.Models;
using MvpApi.Common.Models.Navigation;
using MvpApi.Services.Utilities;
using MvpCompanion.UI.WinUI.Dialogs;
using MvpCompanion.UI.WinUI.ViewModels;

namespace MvpCompanion.UI.WinUI.Views;

public sealed partial class ShellView : UserControl
{
    private TabViewItem lastSelectedTab;

    public ShellView()
    {
        Instance = this;

        InitializeComponent();

        Loaded += ShellView_Loaded;
        Unloaded += ShellView_Unloaded;
    }

    public static ShellView Instance { get; set; }

    public LoginDialog LoginDialog { get; set; }

    private async void ShellView_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        LoginDialog = new LoginDialog(OnLoginCompleted)
        {
            XamlRoot = App.CurrentWindow.Content.XamlRoot
        };

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

                await ViewModel.OnLoadedAsync();

                return;
            }
        }

        // all other cases fall down to needing the user to sign back in
        await LoginDialog.SignInAsync();

        await ViewModel.OnLoadedAsync();

        SelectTab(ViewType.Home);
    }

    private async void ShellView_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        await ViewModel.OnUnloadedAsync();
    }

    private void OnLoginCompleted()
    {
        ViewModel.RefreshProperties();
    }

    public void SelectTab(ViewType viewType)
    {
        var desiredTab = ShellTabView.TabItems.FirstOrDefault(t => (t as TabViewItem)?.Tag.ToString() == viewType.ToString());

        if (desiredTab != null)
        {
            ShellTabView.SelectedItem = desiredTab;
        }
    }

    private void ShellTabView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.RemovedItems is { Count: > 0 })
        {
            lastSelectedTab = e.RemovedItems[0] as TabViewItem;
        }
    }

    private async void ShellTabView_OnTabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
    {
        if (args.Item is TabViewItem tab && (ViewType)tab.Tag == ViewType.Detail)
        {
            sender.TabItems.Remove(args.Item);

            ShellTabView.SelectedItem = ShellTabView.TabItems.FirstOrDefault(t => t is TabViewItem t2 && (ViewType)t2.Tag == ViewType.Home);
        }
        else
        {
            await App.ShowMessageAsync("Only dynamically-added (contribution detail) tabs can be removed.", "Non-removable Tab");
        }
    }
    
    public void AddDetailTab(ContributionsModel contribution)
    {
        try
        {
            var tabItem = ShellTabView.TabItems.FirstOrDefault(t => t is TabViewItem tab 
                                                                    && tab.DataContext is ContributionDetailViewModel vm 
                                                                    && vm.SelectedContribution == contribution);

            TabViewItem tvi;

            if (tabItem == null)
            {
                tvi = new TabViewItem
                {
                    Header = contribution.ContributionId,
                    HeaderTemplate = this.Resources["SmallTabHeaderTemplate"] as DataTemplate,
                    Tag = ViewType.Detail,
                    Content = new ContributionDetailView { Contribution = contribution }
                };

                var insertIndex = ShellTabView.SelectedIndex + 1;

                ShellTabView.TabItems.Insert(insertIndex, tvi);
            }
            else
            {
                tvi = tabItem as TabViewItem;
            }

            ShellTabView.SelectedItem = tvi;
        }
        catch (Exception ex)
        {
            App.ShowMessageAsync($"There was a problem creating a new tab. Error: {ex.Message}.", "Whoops").ConfigureAwait(false);
        }
    }
}