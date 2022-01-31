using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MvpCompanion.UI.WinUI.Views;

public sealed partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();

        Loaded += SettingsView_Loaded;
        Unloaded += SettingsView_Unloaded;
    }

    private async void SettingsView_Unloaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.OnUnloadedAsync();
    }

    private async void SettingsView_Loaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.OnLoadedAsync();
    }
}