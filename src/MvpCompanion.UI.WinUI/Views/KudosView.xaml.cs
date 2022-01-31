using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MvpCompanion.UI.WinUI.Views;

public sealed partial class KudosView : UserControl
{
    public KudosView()
    {
        this.InitializeComponent();
        Loaded += KudosView_Loaded;
        Unloaded += KudosView_Unloaded;
    }

    private async void KudosView_Loaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.OnLoadedAsync();
    }

    private async void KudosView_Unloaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.OnUnloadedAsync();
    }
}