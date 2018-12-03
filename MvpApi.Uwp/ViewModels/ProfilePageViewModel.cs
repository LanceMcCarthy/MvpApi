using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using MvpApi.Common.Models;
using MvpApi.Uwp.Helpers;

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
        
        public async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (App.ShellPage.DataContext is ShellPageViewModel shellVm)
            {
                await shellVm.SignInAsync();
                RefreshState();
            }
        }

        public async void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (App.ShellPage.DataContext is ShellPageViewModel shellVm)
            {
                await shellVm.SignOutAsync();
                RefreshState();
            }
        }

        private void RefreshState()
        {
            RaisePropertyChanged(nameof(this.Mvp));
            RaisePropertyChanged(nameof(this.ProfileImagePath));
        }
        
        #region Navigation

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (App.ShellPage.DataContext is ShellPageViewModel shellVm && !shellVm.IsLoggedIn)
            {
                await shellVm.SignInAsync();
            }
        }

        public override Task OnNavigatedFromAsync(IDictionary<string, object> pageState, bool suspending)
        {
            return base.OnNavigatedFromAsync(pageState, suspending);
        }

        #endregion
    }
}
