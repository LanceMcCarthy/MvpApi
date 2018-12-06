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
        private ObservableCollection<OnlineIdentityViewModel> _onlineIdentities;
        private string _profileImagePath;

        public ProfilePageViewModel()
        {
            if (DesignMode.DesignModeEnabled)
            {
                Mvp = DesignTimeHelpers.GenerateSampleMvp();
                OnlineIdentities = DesignTimeHelpers.GenerateOnlineIdentities();
            }
        }

        public ProfileViewModel Mvp
        {
            get => _mvp;
            set => Set(ref _mvp, value);
        }

        public ObservableCollection<OnlineIdentityViewModel> OnlineIdentities
        {
            get => _onlineIdentities ?? (_onlineIdentities = new ObservableCollection<OnlineIdentityViewModel>());
            set => Set(ref _onlineIdentities, value);
        }

        public string ProfileImagePath
        {
            get => _profileImagePath;
            set => Set(ref _profileImagePath, value);
        }
        
        private async Task RefreshOnlineIdentitiesAsync()
        {
            IsBusy = true;
            IsBusyMessage = "loading Online Identities...";

            var identities = await App.ApiService.GetOnlineIdentitiesAsync();

            if (identities != null & identities?.Count > 0)
            {
                foreach (var onlineIdentity in identities)
                {
                    OnlineIdentities.Add(onlineIdentity);
                }
            }
            
            IsBusyMessage = "";
            IsBusy = false;
        }

        public async void AddOnlineIdentityButton_Click(object sender, RoutedEventArgs e)
        {
            var md = new MessageDialog("What type of online identity would you like to add?\r\n'Linked Identity' is an MSDN property (e.g. MSDN or Microsoft Community Forum), these can be used to automatically create contributions based on your activity.\r\n'Other Identity' for everything else, like social networks, GitHub and StackOverflow. ", "Add Online Identity");

            md.Commands.Add(new UICommand("Linked Identity"));
            md.Commands.Add(new UICommand("Other Identity"));
            
            var result = await md.ShowAsync();

            if (result.Label == "Linked")
            {

            }

            if (result.Label == "Other")
            {

            }
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
