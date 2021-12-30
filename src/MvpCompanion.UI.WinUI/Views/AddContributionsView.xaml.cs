using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MvpCompanion.UI.WinUI.Views;

public sealed partial class AddContributionsView : UserControl
{
    public AddContributionsView()
    {
        InitializeComponent();
        
        AnnualReachNumericBox.Minimum = 0;
        SecondAnnualQuantityNumericBox.Minimum = 0;
        AnnualReachNumericBox.Minimum = 0;
        AnnualQuantityNumericBox.Maximum = int.MaxValue;
        SecondAnnualQuantityNumericBox.Maximum = int.MaxValue;
        AnnualReachNumericBox.Maximum = int.MaxValue;

        Loaded += AddContributionsView_Loaded;
        Unloaded += AddContributionsView_Unloaded;
    }
    
    private async void AddContributionsView_Loaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.OnLoadedAsync();
    }

    private async void AddContributionsView_Unloaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.OnUnloadedAsync();
    }
}