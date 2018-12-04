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
        private ProfileViewModel _mvp;
        private string _profileImagePath;

        public ProfilePageViewModel()
        {
            if (DesignMode.DesignModeEnabled)
            {
                Mvp = DesignTimeHelpers.GenerateSampleMvp();
            }
        }

        public ProfileViewModel Mvp
        {
            get => _mvp;
            set => Set(ref _mvp, value);
        }

        public string ProfileImagePath
        {
            get => _profileImagePath;
            set => Set(ref _profileImagePath, value);
        }

        public async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (App.ShellPage.DataContext is ShellPageViewModel shellVm)
            {
                await shellVm.SignInAsync();
                this.Mvp = shellVm.Mvp;
                this.ProfileImagePath = shellVm.ProfileImagePath;
            }
        }

        public async void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (App.ShellPage.DataContext is ShellPageViewModel shellVm)
            {
                await shellVm.SignOutAsync();
                this.Mvp = null;
                this.ProfileImagePath = null;
            }
        }
        
        #region Navigation

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (App.ShellPage.DataContext is ShellPageViewModel shellVm)
            {
                if (!shellVm.IsLoggedIn)
                {
                    await shellVm.SignInAsync();
                }

                this.Mvp = shellVm.Mvp;
                this.ProfileImagePath = shellVm.ProfileImagePath;
            }
        }

        public override Task OnNavigatedFromAsync(IDictionary<string, object> pageState, bool suspending)
        {
            return base.OnNavigatedFromAsync(pageState, suspending);
        }

        #endregion
    }
}
