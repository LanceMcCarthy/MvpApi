using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private bool _isInEditMode;
        private ObservableCollection<OnlineIdentity> _onlineIdentities;

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

        public ObservableCollection<OnlineIdentity> OnlineIdentities
        {
            get => _onlineIdentities;
            set => Set(ref _onlineIdentities, value);
        }

        public string ProfileImagePath
        {
            get => _profileImagePath;
            set => Set(ref _profileImagePath, value);
        }

        public bool IsInEditMode
        {
            get => _isInEditMode;
            set => Set(ref _isInEditMode, value);
        }

        private async Task RefreshOnlineIdentitiesAsync()
        {
            IsBusy = true;
            IsBusyMessage = "loading Online Identities...";

            var identities = await App.ApiService.GetOnlineIdentitiesAsync();

            OnlineIdentities = identities != null 
                ? new ObservableCollection<OnlineIdentity>(identities) 
                : new ObservableCollection<OnlineIdentity>();

            IsBusyMessage = "";
            IsBusy = false;
        }

        public async void SaveProfileButton_Click(object sender, RoutedEventArgs e)
        {
            await new MessageDialog("You can't save profile changes yet, this will be available in v1.9, the next major update.", "Coming Soon").ShowAsync();
        }

        public async void UpdateProfilePictureButton_OnClick(object sender, RoutedEventArgs e)
        {
            await new MessageDialog("You can't upload photo yet, this will be available in v1.9, the next major update.", "Coming Soon").ShowAsync();
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

                await RefreshOnlineIdentitiesAsync();
                
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
