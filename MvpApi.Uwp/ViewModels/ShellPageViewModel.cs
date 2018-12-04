using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
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
        private ProfileViewModel _mvp;
        private string _profileImagePath;
        private bool _isLoggedIn;
        
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
            get => _profileImagePath;
            set
            {
                //enforcing propChanged
                _profileImagePath = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Currently signed in MVP profile
        /// </summary>
        public ProfileViewModel Mvp
        {
            get => _mvp;
            set => Set(ref _mvp, value);
        }
        
        /// <summary>
        /// Denotes whether the user is currently logged in and able to make successful requests to the API
        /// </summary>
        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set => Set(ref _isLoggedIn, value);
        }

        /// <summary>
        /// Self-contained authentication helper. Will attempt silent login if a refresh token is available, otherwise will show a 
        /// </summary>
        private LoginDialog _loginManager;

        /// <summary>
        /// Logs in the user by showing a ContentDialog for authentication.
        /// Silent Operation if the user has previously logged in, a silent login will be attempted first (using the refresh token).
        /// </summary>
        /// <returns></returns>
        public async Task<bool> SignInAsync()
        {
            // Initialize LoginDialog. This is a special ContentDialog that will handle authentication lifecycle
            if(_loginManager == null)
            {
                _loginManager = new LoginDialog();
            }
            
            try
            {
                // Invoke sign in procedure (this handles token storage internally)
                await _loginManager.AttemptSilentRefreshAsync();

                if (!string.IsNullOrEmpty(_loginManager.AuthorizationCode))
                {
                    App.ApiService = new MvpApiService(_loginManager.AuthorizationCode);
                    
                    IsBusyMessage = "downloading profile info...";
                    Mvp = await App.ApiService.GetProfileAsync();

                    IsBusyMessage = "downloading profile image...";
                    ProfileImagePath = await App.ApiService.DownloadAndSaveProfileImage();

                    IsLoggedIn = true;
                }
            }
            catch (Exception e)
            {
                await e.LogExceptionAsync();
            }

            return !string.IsNullOrEmpty(_loginManager.AuthorizationCode);
        }

        public async Task SignOutAsync()
        {
            // Invoke sign out procedure (this will handle token storage internally), returns Tuple with success and message
            var result = await _loginManager.SignOutAsync();

            // If sign out was successful, handle app-specific files and resources
            if (result.Item1)
            {
                // Clean up profile objects
                IsLoggedIn = false;
                Mvp = null;
                ProfileImagePath = "";
                
                // Clean up files
                try
                {
                    // Profile photo file
                    var imageFile = await ApplicationData.Current.LocalFolder.TryGetItemAsync("ProfilePicture.jpg");
                    if (imageFile != null)
                    {
                        await imageFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
                    }

                    // TODO Clean up Log files (there are automatically kept to a limit of 5 by the logging system).
                    //var query = ApplicationData.Current.LocalFolder.CreateFileQuery();
                    //var files = await query.GetFilesAsync();
                    
                    //foreach (var file in files)
                    //{
                    //    if (file.FileType == "log")
                    //    {
                    //        await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
                    //    }
                    //}
                }
                catch (Exception e)
                {
                    await e.LogExceptionAsync();
                    Debug.WriteLine(e);
                }
            }

            // show user result of sign out attempt
            await new MessageDialog($"{result.Item2}", result.Item1 ? "Success" : "Error").ShowAsync();
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
