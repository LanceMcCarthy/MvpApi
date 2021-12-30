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

    public static readonly DependencyProperty ContributionProperty = DependencyProperty.Register(
        "Contribution", 
        typeof(ContributionsModel), 
        typeof(ContributionDetailView), 
        new PropertyMetadata(default(ContributionsModel), OnContributionChanged));

    private static void OnContributionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ContributionDetailView self)
        {
            self.ViewModel.SelectedContribution = e.NewValue as ContributionsModel;
        }
    }

    public ContributionsModel Contribution
    {
        get => (ContributionsModel)GetValue(ContributionProperty);
        set => SetValue(ContributionProperty, value);
    }

    private async void ContributionDetailView_Loaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.OnLoadedAsync();
    }

    private async void ContributionDetailView_Unloaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.OnUnloadedAsync();
    }
}