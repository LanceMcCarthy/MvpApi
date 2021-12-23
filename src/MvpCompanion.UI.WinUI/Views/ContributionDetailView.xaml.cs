using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MvpApi.Common.Models;

namespace MvpCompanion.UI.WinUI.Views;

public sealed partial class ContributionDetailView : UserControl
{
    public ContributionDetailView()
    {
        InitializeComponent();
        Loaded += ContributionDetailView_Loaded;
        Unloaded += ContributionDetailView_Unloaded;

        AnnualReachNumericBox.Minimum = 0;
        SecondAnnualQuantityNumericBox.Minimum = 0;
        AnnualReachNumericBox.Minimum = 0;
        AnnualQuantityNumericBox.Maximum = int.MaxValue;
        SecondAnnualQuantityNumericBox.Maximum = int.MaxValue;
        AnnualReachNumericBox.Maximum = int.MaxValue;
    }

    public ContributionDetailView(ContributionsModel contribution)
    {
        InitializeComponent();

        ViewModel.SelectedContribution = contribution;

        Loaded += ContributionDetailView_Loaded;
        Unloaded += ContributionDetailView_Unloaded;

        AnnualReachNumericBox.Minimum = 0;
        SecondAnnualQuantityNumericBox.Minimum = 0;
        AnnualReachNumericBox.Minimum = 0;
        AnnualQuantityNumericBox.Maximum = int.MaxValue;
        SecondAnnualQuantityNumericBox.Maximum = int.MaxValue;
        AnnualReachNumericBox.Maximum = int.MaxValue;
    }

    private void ContributionDetailView_Loaded(object sender, RoutedEventArgs e)
    {
        ViewModel.OnLoaded();
    }

    private void ContributionDetailView_Unloaded(object sender, RoutedEventArgs e)
    {
        ViewModel.OnUnloaded();
    }
}