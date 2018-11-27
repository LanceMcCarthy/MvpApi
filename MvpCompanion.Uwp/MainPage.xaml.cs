using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Microsoft.Identity.Client;
using MvpApi.Services.Apis;
using MvpApi.Services.Utilities;

namespace MvpCompanion.Uwp
{
    public sealed partial class MainPage : Page
    {
        private MvpApiService _apiService;
        private OAuthHelper _oauthHelper;
        AuthenticationResult _authResult;

        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void CallMvpPApiButton_Click(object sender, RoutedEventArgs e)
        {
            BusyIndicator.IsActive = true;
            
            if (_oauthHelper == null)
            {
                BusyIndicator.Content = "creating OAuthHelper service...";
                _oauthHelper = new OAuthHelper();

                BusyIndicator.Content = "logging in...";
                _authResult = await _oauthHelper.LogInAsync();

                DisplayBasicTokenInfo(_authResult);

                ResultText.Text = _authResult.Account.Username + "is signed in";

                BusyIndicator.Content = "newing up MVP API service...";

                _apiService = new MvpApiService(_authResult.AccessToken);
                
            }

            BusyIndicator.Content = "getting photo...";

            var profile = await _apiService.GetProfileAsync();

            if (profile == null)
            {
                ResultText.Text += " BUT HAD A BAD API CALL";
            }
            else
            {
                ResultText.Text = $"YAY! {profile.DisplayName}'s profile was fetched!!!";
            }
            
            BusyIndicator.Content = "";
            BusyIndicator.IsActive = false;
        }

        private async void SignOutButton_Click(object sender, RoutedEventArgs e)
        {
            if (_apiService != null)
            {
                await _oauthHelper.LogOutAsync();
            }
        }

        private void DisplayBasicTokenInfo(AuthenticationResult authResult)
        {
            TokenInfoText.Text = "";
            if (authResult != null)
            {
                TokenInfoText.Text += $"Username: {authResult.Account.Username}" + Environment.NewLine;
                TokenInfoText.Text += $"Token Expires: {authResult.ExpiresOn.ToLocalTime()}" + Environment.NewLine;
                TokenInfoText.Text += $"Access Token: {authResult.AccessToken}" + Environment.NewLine;
            }
        }


        // System.ArgumentException: MSAL always sends the scopes 'openid profile offline_access', do not include in scopes parameters
        //static string[] scopes = { "user.read", "user.readbasic.all", "user.readwrite", "email" };

        //public async Task InitializeAsync()
        //{
        //    // MSAL test
        //    // - for any Work or School accounts, or Microsoft personal account, use "common"
        //    // - for Microsoft Personal account, use "consumers"
        //    string tenant = "common";
        //    string authority = $"https://login.microsoftonline.com/{tenant}";


        //    PublicClientApp = new PublicClientApplication(_clientId, authority, _userTokenCache); //string ClientId = "090fa1d9-3d6f-4f6f-a733-a8b8a3fe16ff";

        //}

        //public PublicClientApplication PublicClientApp { get; private set; }

        //public async Task<string> LogInAsync()
        //{
        //    AuthenticationResult authResult = null;

        //    var accounts = await PublicClientApp.GetAccountsAsync();

        //    try
        //    {
        //        authResult = await PublicClientApp.AcquireTokenSilentAsync(scopes, accounts.FirstOrDefault());
        //        return $"{authResult.Account.Username} - Signed In. Expires {authResult.ExpiresOn.ToLocalTime()}";
        //    }
        //    catch (MsalUiRequiredException ex)
        //    {
        //        // A MsalUiRequiredException happened on AcquireTokenSilentAsync, this indicates you need to call AcquireTokenAsync to acquire a token
        //        Debug.WriteLine($"MsalUiRequiredException: {ex.Message}");

        //        try
        //        {
        //            authResult = await PublicClientApp.AcquireTokenAsync(scopes);

        //        }
        //        catch (MsalException msalex)
        //        {
        //            return $"Error Acquiring Token:{Environment.NewLine}{msalex}";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return $"Error Acquiring Token Silently:{Environment.NewLine}{ex}";
        //    }

        //    if (authResult != null)
        //    {

        //        Debug.WriteLine(DisplayBasicTokenInfo(authResult));
        //        return $"{authResult.Account.Username} - Signed In. Expires {authResult.ExpiresOn.ToLocalTime()}";
        //    }
        //    else
        //    {
        //        return "user not signed in";
        //    }
        //}

        //public async Task<string> LogOutAsync()
        //{
        //    var accounts = await PublicClientApp.GetAccountsAsync();

        //    if (accounts.Any())
        //    {
        //        try
        //        {
        //            await PublicClientApp.RemoveAsync(accounts.FirstOrDefault());
        //            return "User has signed-out";
        //        }
        //        catch (MsalException ex)
        //        {
        //            return $"Error signing-out user: {ex.Message}";
        //        }
        //    }

        //    return "There were no accounts to sign out of";
        //}

        //public static string DisplayBasicTokenInfo(AuthenticationResult authResult)
        //{
        //    var tokenInfoText = "";

        //    if (authResult != null)
        //    {
        //        tokenInfoText += $"Username: {authResult.Account.Username}" + Environment.NewLine;
        //        tokenInfoText += $"Token Expires: {authResult.ExpiresOn.ToLocalTime()}" + Environment.NewLine;
        //        tokenInfoText += $"Access Token: {authResult.AccessToken}" + Environment.NewLine;
        //    }

        //    return tokenInfoText;
        //}

        //#region MSALToken Management

        //private TokenCache _userTokenCache;

        //private readonly object _fileLock = new object();

        //private void BeforeAccessNotification(TokenCacheNotificationArgs args)
        //{
        //    lock (_fileLock)
        //    {
        //        var userCache = StorageHelpers.Instance.LoadDecrypted()
        //        args.TokenCache.
        //    }
        //}

        //private void AfterAccessNotification(TokenCacheNotificationArgs args)
        //{
        //    // if the access operation resulted in a cache update
        //    if (args.TokenCache.HasStateChanged)
        //    {
        //        lock (_fileLock)
        //        {
        //            StorageHelpers.Instance.StoreEncrypted(args.TokenCache);
        //            args.TokenCache.HasStateChanged = false;
        //        }
        //    }
        //}

        //#endregion
    }
}
