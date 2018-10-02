using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.UI.Popups;
using Windows.UI.Xaml.Navigation;
using Microsoft.Toolkit.Uwp.Connectivity;
using MvpApi.Common.Models;
using MvpApi.Uwp.Helpers;
using MvpApi.Uwp.Views;
using Template10.Common;

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
        
        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (!NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
            {
                await new MessageDialog("This application requires an internet connection. Please check your connection and launch the app again.", "No Internet").ShowAsync();
                
                return;
            }

            if (!IsLoggedIn)
                BootStrapper.Current.NavigationService.Navigate(typeof(LoginPage));
        }
        
        public override Task OnNavigatedFromAsync(IDictionary<string, object> pageState, bool suspending)
        {
            return base.OnNavigatedFromAsync(pageState, suspending);
        }
    }
}
