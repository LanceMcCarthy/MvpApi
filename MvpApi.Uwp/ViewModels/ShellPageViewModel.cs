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
        #region Fields
        
        private ProfileViewModel mvp;
        private string profileImagePath;
        private bool isLoggedIn;

        #endregion

        public ShellPageViewModel()
        {
            if (DesignMode.DesignModeEnabled)
            {
                Mvp = DesignTimeHelpers.GenerateSampleMvp();
                IsLoggedIn = true;
                ProfileImagePath = "ms-appx:///Images/iSeeSharpPeople.jpg";
                return;
            }
        }

        #region Properties

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
        /// Timestamp of last login. There is a 60 minute session limit for the access token returned after a login.
        /// </summary>
        public DateTime LoginTimeStamp { get; set; }

        /// <summary>
        /// Denotes whether the user is currently loigged in and able to make successful requests to the API
        /// </summary>
        public bool IsLoggedIn
        {
            get
            {
                // API has a valid session time of 60 minutes, force sign-in by returning false
                if (DateTime.Now - LoginTimeStamp > TimeSpan.FromMinutes(60))
                {
                    isLoggedIn = false;
                }

                return isLoggedIn;
            }
            set
            {
                Set(ref isLoggedIn, value);

                if(value)
                    LoginTimeStamp = DateTime.Now;
            }
        }

        #endregion

        #region Navigation

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

        #endregion
    }
}
