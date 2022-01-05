using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MvpApi.Common.Models;

namespace MvpCompanion.UI.WinUI.Views;

public sealed partial class HomeView : UserControl
{
    private ContributionsModel lastExpanded;

    public HomeView()
    {
        InitializeComponent();

        // This is because WinUI3 and UWP do not support OneWayToSource binding mode
        ViewModel.SelectedContributions = ContributionsGrid.SelectedItems;
        ViewModel.GroupDescriptors = ContributionsGrid.GroupDescriptors;

        Loaded += HomeView_Loaded;
        Unloaded += HomeView_Unloaded;
    }

    private void HomeView_Loaded(object sender, RoutedEventArgs e)
    {
        // DO NOT USE! This is called in the ShellView.xaml.cs in OnLoginCompleted() event
        // await ViewModel.OnLoadedAsync();
    }

    private async void HomeView_Unloaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.OnUnloadedAsync();
    }

    private void ExpandButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is ContributionsModel contribution)
        {
            if (lastExpanded != null && lastExpanded == contribution)
            {
                ContributionsGrid.HideRowDetailsForItem(contribution);
            }
            else
            {
                ContributionsGrid.ShowRowDetailsForItem(contribution);
            }

            lastExpanded = contribution;
        }
    }

    private void GoButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is ContributionsModel contribution)
        {
            ShellView.Instance.AddDetailTab(contribution);
        }
    }
}