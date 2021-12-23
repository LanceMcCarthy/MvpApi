using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MvpCompanion.UI.WinUI.Views;

public sealed partial class AboutView : UserControl
{
    public AboutView()
    {
        InitializeComponent();
        Loaded += AboutView_Loaded;
        Unloaded += AboutView_Unloaded;
    }

    private void AboutView_Loaded(object sender, RoutedEventArgs e)
    {
        ViewModel.OnLoaded();
    }

    private void AboutView_Unloaded(object sender, RoutedEventArgs e)
    {
        ViewModel.OnUnloaded();
    }
}