using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI.Xaml.Navigation;
using MvpApi.Services;
using MvpApi.Uwp.Common;
using MvpApi.Uwp.Helpers;
using Template10.Common;
using Template10.Mvvm;

namespace MvpApi.Uwp.ViewModels
{
    public class LoginPageViewModel : ViewModelBase
    {
        #region Fields

        private static string redirectUrl = "https://login.live.com/oauth20_desktop.srf";
        private static string accessTokenUrl = "https://login.live.com/oauth20_token.srf";
        private static readonly Uri SignInUrl = new Uri($"https://login.live.com/oauth20_authorize.srf?client_id={Constants.ClientId}&redirect_uri=https:%2F%2Flogin.live.com%2Foauth20_desktop.srf&response_type=code&scope={Constants.Scope}");
        private static string refreshTokenUrl = $"https://login.live.com/oauth20_token.srf?client_id={Constants.ClientId}&client_secret={Constants.ClientSecret}&redirect_uri=https://login.live.com/oauth20_desktop.srf&grant_type=refresh_token&refresh_token=";
        
        private readonly ApplicationDataContainer localSettings;
        
        // backing fields
        private Uri browserUri;
        private bool isBusy;
        private string isBusyMessage;

        #endregion

        public LoginPageViewModel()
        {
            if (!DesignMode.DesignModeEnabled)
            {
                localSettings = ApplicationData.Current.LocalSettings;
            }
        }

        #region Properties

        public Uri BrowserUri
        {
            get { return browserUri; }
            set { Set(ref browserUri, value); }
        }
        public bool IsBusy
        {
            get { return isBusy; }
            set { Set(ref isBusy, value); }
        }

        public string IsBusyMessage
        {
            get { return isBusyMessage; }
            set { Set(ref isBusyMessage, value); }
        }

        #endregion

        #region event handlers

        public async void BrowserWindow_LoadCompleted(object sender, NavigationEventArgs e)
        {
            if (e.Uri.AbsoluteUri.Contains("code="))
            {
                var authCode = Regex.Split(e.Uri.AbsoluteUri, "code=")[1];

                // get access token
                var apiAuthorization = await OauthHelpers.RequestAuthorizationAsync(accessTokenUrl, authCode);

                if (string.IsNullOrEmpty(apiAuthorization))
                {
                    Debug.WriteLine("BrowserWindow_LoadCompleted - apiAuthorization was null");
                    return;
                }
                
                // Auth is good, new up the ApiService
                App.ApiService = new MvpApiService(Constants.SubscriptionKey, apiAuthorization);

                await LoadProfileInfoAsync();

            }
            else if (e.Uri.AbsoluteUri.Contains("lc="))
            {
                BrowserUri = SignInUrl;
            }
        }
        
        #endregion

        #region Methods

        private async Task LoadProfileInfoAsync()
        {
            try
            {
                IsBusy = true;

                var shellVm = App.ShellPage.DataContext as ShellPageViewModel;
                if (shellVm != null)
                {
                    shellVm.IsLoggedIn = true;
                    IsBusyMessage = "downloading profile info...";
                    shellVm.Mvp = await App.ApiService.GetProfileAsync();

                    IsBusyMessage = "downloading profile image...";
                    shellVm.ProfileImagePath = await LoadProfileImageAsync();
                }

                if (BootStrapper.Current.NavigationService.CanGoBack)
                    BootStrapper.Current.NavigationService.GoBack();
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
            }
            finally
            {
                IsBusyMessage = "";
                IsBusy = false;
            }
        }

        private async Task<string> LoadProfileImageAsync()
        {
            try
            {
                // download the image
                var imageBytes = await App.ApiService.GetProfileImageAsync();

                var imageFile = await StorageHelpers.SaveImageFileAsync(imageBytes, "ProfilePicture.jpg");

                return imageFile.Path;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }

        #endregion
        
        #region Navigation

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            bool loggingOut = false;

            if (parameter is bool)
                loggingOut = (bool)parameter;

            if (loggingOut)
            {
                // Log out
                BrowserUri = new Uri($"https://login.live.com/oauth20_logout.srf?client_id={Constants.ClientId}&redirect_uri=https:%2F%2Flogin.live.com%2Foauth20_desktop.srf");

                StorageHelpers.DeleteToken("access_token");
                StorageHelpers.DeleteToken("refresh_token");

                // Clean up Mvp info
                var shellVm = App.ShellPage.DataContext as ShellPageViewModel;
                if (shellVm != null)
                {
                    shellVm.IsLoggedIn = false;
                    shellVm.Mvp = null;
                    shellVm.ProfileImagePath = null;
                }

                // Clean up file
                try
                {
                    var imageFile = await ApplicationData.Current.LocalFolder.TryGetItemAsync("ProfilePicture.jpg");
                    if (imageFile != null)
                        await imageFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
            else
            {
                // Login
                BrowserUri = SignInUrl;
            }
        }

        public override Task OnNavigatedFromAsync(IDictionary<string, object> pageState, bool suspending)
        {
            return base.OnNavigatedFromAsync(pageState, suspending);
        }

        #endregion
    }
}
