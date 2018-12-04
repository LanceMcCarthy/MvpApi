using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using MvpApi.Common.Models;
using MvpApi.Uwp.Helpers;
using MvpApi.Uwp.Views;

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

        public async void EditProfileButton_Click(object sender, RoutedEventArgs e)
        {
            await new MessageDialog("To Be Implemented soon").ShowAsync();
        }
        
        #region Navigation

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (ShellPage.Instance.DataContext is ShellPageViewModel shellVm)
            {
                if (!shellVm.IsLoggedIn)
                {
                    IsBusy = true;
                    IsBusyMessage = "signing in...";

                    await ShellPage.Instance.SignInAsync();
                }

                this.Mvp = shellVm.Mvp;
                this.ProfileImagePath = shellVm.ProfileImagePath;

                IsBusyMessage = "";
                IsBusy = false;
            }
        }

        public override Task OnNavigatedFromAsync(IDictionary<string, object> pageState, bool suspending)
        {
            return base.OnNavigatedFromAsync(pageState, suspending);
        }

        #endregion
    }
}
