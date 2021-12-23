using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MvpCompanion.UI.WinUI.Views;

public sealed partial class ProfileView : UserControl
{
    public ProfileView()
    {
        InitializeComponent();
        Loaded += ProfileView_Loaded;
        Unloaded += ProfileView_Unloaded;
    }

    private void ProfileView_Loaded(object sender, RoutedEventArgs e)
    {
        ViewModel.OnLoaded();
    }

    private void ProfileView_Unloaded(object sender, RoutedEventArgs e)
    {
        ViewModel.OnUnloaded();
    }
}