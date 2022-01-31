using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

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
        ViewModel.OnLoadedAsync();

        GearsStory.RepeatBehavior = RepeatBehavior.Forever;
        GearsStory.Begin();
    }

    private async void AboutView_Unloaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.OnUnloadedAsync();
    }
}