using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.UI.Popups;
using Windows.UI.Xaml.Navigation;
using Microsoft.Toolkit.Uwp.Connectivity;
using MvpApi.Common.Models;
using MvpApi.Services.Apis;
using MvpApi.Uwp.Dialogs;
using MvpApi.Uwp.Helpers;

namespace MvpApi.Uwp.ViewModels
{
    public class ShellPageViewModel : PageViewModelBase
    {
        private ProfileViewModel mvp;
        private string profileImagePath;
        private bool isLoggedIn;
        
        public ShellPageViewModel()
        {
            if (DesignMode.DesignModeEnabled)
            {
                Mvp = DesignTimeHelpers.GenerateSampleMvp();
                IsLoggedIn = true;
                ProfileImagePath = "/Images/MvpIcon.png";
                return;
            }
        }

        /// <summary>
        /// File path to locally saved MVP profile image
        /// </summary>
        public string ProfileImagePath
        {
            get => profileImagePath;
            set
            {
                //enforcing propChanged
                profileImagePath = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Currently signed in MVP profile
        /// </summary>
        public ProfileViewModel Mvp
        {
            get => mvp;
            set => Set(ref mvp, value);
        }
        
        /// <summary>
        /// Denotes whether the user is currently logged in and able to make successful requests to the API
        /// </summary>
        public bool IsLoggedIn
        {
            get => isLoggedIn;
            set => Set(ref isLoggedIn, value);
        }

        /// <summary>
        /// Authenticates the user on app launch, but can be shown again from anywhere in the application
        /// </summary>
        public LoginDialog LoginManager { get; set; }

        public async Task VerifyLoginAsync()
        {
            if(LoginManager == null)
            {
                LoginManager = new LoginDialog();
            }
            
            await LoginManager.AttemptSilentRefreshAsync();

            if (!string.IsNullOrEmpty(LoginManager.AuthorizationCode))
            {
                App.ApiService = new MvpApiService(LoginManager.AuthorizationCode);

                IsLoggedIn = true;

                IsBusyMessage = "downloading profile info...";
                Mvp = await App.ApiService.GetProfileAsync();

                IsBusyMessage = "downloading profile image...";
                ProfileImagePath = await App.ApiService.DownloadAndSaveProfileImage();
            }
        }
        
        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (!NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
            {
                await new MessageDialog("This application requires an internet connection. Please check your connection and launch the app again.", "No Internet").ShowAsync();
                return;
            }
        }

        public override Task OnNavigatedFromAsync(IDictionary<string, object> pageState, bool suspending)
        {
            return base.OnNavigatedFromAsync(pageState, suspending);
        }
    }
}
