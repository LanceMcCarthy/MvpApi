using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MvpApi.Common.Models;
using Telerik.UI.Xaml.Controls.Grid;
using Telerik.UI.Xaml.Controls.Grid.Primitives;

namespace MvpCompanion.UI.WinUI.Views;

public sealed partial class HomeView : UserControl
{
    public HomeView()
    {
        InitializeComponent();
        Loaded += HomeView_Loaded;
        Unloaded += HomeView_Unloaded;
    }

    private void HomeView_Loaded(object sender, RoutedEventArgs e)
    {
        ViewModel.SelectedContributions = ContributionsGrid.SelectedItems;
        //ViewModel.OnLoaded(false);
    }

    private void HomeView_Unloaded(object sender, RoutedEventArgs e)
    {
        ViewModel.OnUnloaded();
    }

    private ContributionsModel lastExpanded;
    
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
}