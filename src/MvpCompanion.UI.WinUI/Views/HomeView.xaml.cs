using System.Linq;
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
        Loaded += HomeView_Loaded;
        Unloaded += HomeView_Unloaded;
    }

    private async void HomeView_Loaded(object sender, RoutedEventArgs e)
    {
        ViewModel.SelectedContributions = ContributionsGrid.SelectedItems;

        //await ViewModel.OnLoadedAsync();
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