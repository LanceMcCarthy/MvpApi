using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using MvpApi.Common.Models;
using MvpApi.Uwp.Helpers;
using MvpApi.Uwp.Views;
using Template10.Common;

namespace MvpApi.Uwp.ViewModels
{
    public class ProfilePageViewModel : PageViewModelBase
    {
        public ProfilePageViewModel()
        {
            if (DesignMode.DesignModeEnabled)
            {
                Mvp = DesignTimeHelpers.GenerateSampleMvp();
            }
        }
        
        public ProfileViewModel Mvp { get; set; } = (App.ShellPage.DataContext as ShellPageViewModel)?.Mvp;

        public string ProfileImagePath { get; set; } = (App.ShellPage.DataContext as ShellPageViewModel)?.ProfileImagePath;
        
        public void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            BootStrapper.Current.NavigationService.Navigate(typeof(LoginPage));
        }

        public void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            // Passing a bool true parameter performs logout
            BootStrapper.Current.NavigationService.Navigate(typeof(LoginPage), true);
        }
        
        #region Navigation

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (App.ShellPage.DataContext is ShellPageViewModel shellVm && shellVm.IsLoggedIn)
            {

            }
            else
            {
                await BootStrapper.Current.NavigationService.NavigateAsync(typeof(LoginPage));
            }
        }

        public override Task OnNavigatedFromAsync(IDictionary<string, object> pageState, bool suspending)
        {
            return base.OnNavigatedFromAsync(pageState, suspending);
        }

        #endregion
    }
}
