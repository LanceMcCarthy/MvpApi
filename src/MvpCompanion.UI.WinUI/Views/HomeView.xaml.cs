using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

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
        ViewModel.OnLoaded(false);
    }

    private void HomeView_Unloaded(object sender, RoutedEventArgs e)
    {
        ViewModel.OnUnloaded();
    }
}