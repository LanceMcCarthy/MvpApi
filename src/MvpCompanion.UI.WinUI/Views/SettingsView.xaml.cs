using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MvpCompanion.UI.WinUI.Views
{
    public sealed partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            this.InitializeComponent();

            Loaded += SettingsView_Loaded;
            Unloaded += SettingsView_Unloaded;
        }

        private void SettingsView_Unloaded(object sender, RoutedEventArgs e)
        {
            ViewModel.OnUnloaded();
        }

        private void SettingsView_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.OnLoaded();
        }
    }
}
