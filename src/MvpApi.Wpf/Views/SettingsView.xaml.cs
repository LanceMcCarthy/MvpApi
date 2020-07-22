using System.Windows;
using System.Windows.Controls;

namespace MvpApi.Wpf.Views
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
            Loaded += SettingsView_Loaded;
        }

        private async void SettingsView_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.OnLoadedAsync();
        }
    }
}
