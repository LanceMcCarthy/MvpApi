using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.AppCenter.Analytics;
using MvpApi.Wpf.Models;

namespace MvpApi.Wpf
{
    public partial class ShellWindow : Window
    {
        public ShellWindow()
        {
            if (!string.IsNullOrEmpty(Properties.Settings.Default.PreferredTheme))
            {
                UpdateTheme(Properties.Settings.Default.PreferredTheme);
            }

            InitializeComponent();

            Loaded += ShellWindow_Loaded;
        }

        private async void ShellWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.OnLoadedAsync();
        }

        private void ChangeView(ViewName viewName)
        {
            RootNavigationView.SelectedIndex = (int)viewName;

            Analytics.TrackEvent("View Changed", new Dictionary<string, string>
            {
                {"ViewName", $"{viewName}"}
            });
        }

        public void UpdateTheme(string assemblyName)
        {
            Application.Current.Resources.MergedDictionaries.Clear();

            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri($"/Telerik.Windows.Themes.{assemblyName};component/Themes/System.Windows.xaml", UriKind.RelativeOrAbsolute)
            });

            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri($"/Telerik.Windows.Themes.{assemblyName};component/Themes/Telerik.Windows.Controls.xaml", UriKind.RelativeOrAbsolute)
            });

            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri($"/Telerik.Windows.Themes.{assemblyName};component/Themes/Telerik.Windows.Controls.Data.xaml", UriKind.RelativeOrAbsolute)
            });

            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri($"/Telerik.Windows.Themes.{assemblyName};component/Themes/Telerik.Windows.Controls.GridView.xaml", UriKind.RelativeOrAbsolute)
            });

            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri($"/Telerik.Windows.Themes.{assemblyName};component/Themes/Telerik.Windows.Controls.Input.xaml", UriKind.RelativeOrAbsolute)
            });

            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri($"/Telerik.Windows.Themes.{assemblyName};component/Themes/Telerik.Windows.Controls.Navigation.xaml", UriKind.RelativeOrAbsolute)
            });
            
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("Styles/GeneralStyles.xaml", UriKind.RelativeOrAbsolute)
            });
        }
    }
}
