using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using MvpApi.Wpf.ViewModels;

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
            if (DataContext is SettingsViewModel vm)
            {
                await vm.OnLoadedAsync();
            }
        }

        private void UrlButton_Click(object sender, RoutedEventArgs e)
        {
            var url = (sender as Button)?.Tag.ToString();

            Process.Start(url);
        }

        private void FeedbackButton_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void EmailButton_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
