using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using Microsoft.Toolkit.Uwp.Connectivity;
using MvpApi.Common.Models;
using MvpApi.Uwp.Common;
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
        private readonly DispatcherTimer sessionTimer;
        
        public ShellPageViewModel()
        {
            if (DesignMode.DesignModeEnabled)
            {
                Mvp = DesignTimeHelpers.GenerateSampleMvp();
                IsLoggedIn = true;
                ProfileImagePath = "/Images/MvpIcon.png";
                return;
            }

            sessionTimer = new DispatcherTimer();
            sessionTimer.Interval = TimeSpan.FromMinutes(5);

            
        }

        private void SessionTimer_Tick(object sender, object e)
        {
            Debug.WriteLine($"Session Timer - Tick");

            if (DateTime.Now - LoginTimeStamp > TimeSpan.FromMinutes(60))
            {
                Debug.WriteLine($"Session Timer - Session Expired");
                IsLoggedIn = false;
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

                Debug.WriteLine($"IsLoggedIn (get): {isLoggedIn}");

                return isLoggedIn;
            }
            set
            {
                Debug.WriteLine($"IsLoggedIn (set): {value}");

                Set(ref isLoggedIn, value);
                
                if (value)
                {
                    LoginTimeStamp = DateTime.Now;

                    if(!sessionTimer.IsEnabled)
                        sessionTimer.Start();
                }
                else
                {
                    if(sessionTimer.IsEnabled)
                        sessionTimer.Stop();
                }

                // Fire off event to alert any subscribing view models that the user needs to log back in 
                // and it should cache any incomplete forms
                IsLoggedInChanged?.Invoke(this, new LoginChangedEventArgs(value));
            }
        }
        
        public delegate void LoggedInChanged(object sender, LoginChangedEventArgs args);

        /// <summary>
        /// Raised when the user is logged in, logs out or times out.
        /// </summary>
        public event LoggedInChanged IsLoggedInChanged;
        
        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (!NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
            {
                await new MessageDialog("This application requires an internet connection. Please check your connection and launch the app again.", "No Internet").ShowAsync();
                
                return;
            }

            sessionTimer.Tick += SessionTimer_Tick;
            
            if (!IsLoggedIn)
                BootStrapper.Current.NavigationService.Navigate(typeof(LoginPage));
        }
        
        public override Task OnNavigatedFromAsync(IDictionary<string, object> pageState, bool suspending)
        {
            sessionTimer.Tick -= SessionTimer_Tick;

            return base.OnNavigatedFromAsync(pageState, suspending);
        }
    }
}
