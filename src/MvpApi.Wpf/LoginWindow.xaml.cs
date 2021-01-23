using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using Windows.Storage;
using Windows.UI.Popups;
using Microsoft.AppCenter.Crashes;
using MvpApi.Common.CustomEventArgs;
using MvpApi.Common.Extensions;
using MvpApi.Services.Apis;
//using MvpApi.Services.Utilities;
using MvpApi.Wpf.Helpers;
using Newtonsoft.Json;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.SplashScreen;
using Analytics = Microsoft.AppCenter.Analytics.Analytics;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using System.Linq;

namespace MvpApi.Wpf
{
    public partial class LoginWindow : Window
    {
        private static readonly string _scope = "wl.emails%20wl.basic%20wl.offline_access%20wl.signin";
        private static readonly string _clientId = "090fa1d9-3d6f-4f6f-a733-a8b8a3fe16ff";
        private readonly string _redirectUrl = "https://login.live.com/oauth20_desktop.srf";
        private readonly string _accessTokenUrl = "https://login.live.com/oauth20_token.srf";
        private readonly Uri _signInUri = new Uri($"https://login.live.com/oauth20_authorize.srf?client_id={_clientId}&redirect_uri=https:%2F%2Flogin.live.com%2Foauth20_desktop.srf&response_type=code&scope={_scope}");
        private readonly Uri _signOutUri = new Uri($"https://login.live.com/oauth20_logout.srf?client_id={_clientId}&redirect_uri=https:%2F%2Flogin.live.com%2Foauth20_desktop.srf");

        private readonly Action _loginCompleted;

        private readonly string _appDataFolder;
        private readonly byte[] _symmetricKey;
        private readonly byte[] _initializationVector;

        public LoginWindow()
        {
            InitializeComponent();
            var keyGenerator = new Rfc2898DeriveBytes("b78BfJKEs7g", Encoding.ASCII.GetBytes("b78BfJKEs7g"));
            _symmetricKey = keyGenerator.GetBytes(32);
            _initializationVector = keyGenerator.GetBytes(16);
        }

        /// <summary>
        /// Initialize with a callback that invokes when login completes
        /// </summary>
        /// <param name="loginCompleted"></param>
        public LoginWindow(Action loginCompleted)
        {
            InitializeComponent();
            var keyGenerator = new Rfc2898DeriveBytes("b78BfJKEs7g", Encoding.ASCII.GetBytes("b78BfJKEs7g"));
            _symmetricKey = keyGenerator.GetBytes(32);
            _initializationVector = keyGenerator.GetBytes(16);
            _loginCompleted = loginCompleted;
        }

        private void ToggleBusy(string message, bool isBusy = true)
        {
            StatusTextBlock.Text = message;
            StatusTextBlock.Visibility = isBusy ? Visibility.Visible : Visibility.Collapsed;
        }

        private async Task CompleteSignInAsync(string authorizationHeader)
        {
            await InitializeMvpApiAsync(authorizationHeader);

            _loginCompleted?.Invoke();

            this.Hide();
        }

        private async void WebBrowser_OnNavigated(object sender, NavigationEventArgs e)
        {
            if (e.Uri.AbsoluteUri.Contains("code="))
            {
                var authCode = e.Uri.ExtractQueryValue("code");

                var authorizationHeader = await RequestAuthorizationAsync(authCode);

                if (!string.IsNullOrEmpty(authorizationHeader))
                {
                    await CompleteSignInAsync(authorizationHeader);
                }
            }
            else if (e.Uri.AbsoluteUri.Contains("lc="))
            {
                // If the redirected uri doesn't have a code in query string, redirect with Uri that explicitly requests response_type=code
                AuthWebView.Source = _signInUri;
            }
        }

        public async Task SignInAsync()
        {
            var refreshToken = LoadToken("refresh_token");

            // If refresh token is available, the user has previously been logged in and we can get a refreshed access token immediately
            if (!string.IsNullOrEmpty(refreshToken))
            {
                var authorizationHeader = await RequestAuthorizationAsync(refreshToken, true);

                if (!string.IsNullOrEmpty(authorizationHeader))
                {
                    Analytics.TrackEvent("LoginWindow SignInAsync - Seamless Signin Achieved");

                    await CompleteSignInAsync(authorizationHeader);

                    return;
                }
            }

            // important we let this fall through to avoid multiple else statements
            Analytics.TrackEvent("LoginWindow SignInAsync - Manual Signin Required");

            AuthWebView.Source = _signInUri;

            // Needs fresh login, navigate to sign in page
            this.ShowDialog();
        }

        public async Task SignOutAsync()
        {
            Analytics.TrackEvent("LoginWindow SignOutAsync");

            this.Show();

            try
            {
                // Indicate to user we are signing out
                ToggleBusy("logging out...");

                // Erase cached tokens
                //ViewModel.IsBusyMessage = "deleting cache files...";
                DeleteToken("access_token");
                DeleteToken("refresh_token");

                // Clean up profile objects
                ToggleBusy("resetting profile...");
                App.ApiService.Mvp = null;
                App.ApiService.ProfileImagePath = "";

                // Delete profile photo file
                ToggleBusy("deleting profile photo file...");

                var imageFile = await ApplicationData.Current.LocalFolder.TryGetItemAsync("ProfilePicture.jpg");
                if (imageFile != null) await imageFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);
                await ex.LogExceptionWithUserMessage();
            }
            finally
            {
                // Hide busy indicator
                ToggleBusy("", false);

                // Toggle flag
                App.ApiService.IsLoggedIn = false;

                // Start auth workflow again (the logout Uri redirects to sign in again)
                AuthWebView.Source = _signOutUri;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
        public async Task InitializeMvpApiAsync(string authorizationHeader)
        {
            if (App.ApiService != null)
            {
                App.ApiService.AccessTokenExpired -= ApiService_AccessTokenExpired;
                App.ApiService.RequestErrorOccurred -= ApiService_RequestErrorOccurred;
            }

            // New-up the service
            App.ApiService = new MvpApiService(authorizationHeader);

            App.ApiService.AccessTokenExpired += ApiService_AccessTokenExpired;
            App.ApiService.RequestErrorOccurred += ApiService_RequestErrorOccurred;

            App.ApiService.IsLoggedIn = true;

            // Get MVP profile
            ((SplashScreenDataContext)RadSplashScreenManager.SplashScreenDataContext).Content = "downloading profile info...";
            App.ApiService.Mvp = await App.ApiService.GetProfileAsync();

            // Get MVP profile image
            ((SplashScreenDataContext)RadSplashScreenManager.SplashScreenDataContext).Content = "downloading profile image...";
            App.ApiService.ProfileImagePath = await App.ApiService.DownloadAndSaveProfileImage();

            ((SplashScreenDataContext)RadSplashScreenManager.SplashScreenDataContext).Content = "";
        }

        private async void ApiService_AccessTokenExpired(object sender, ApiServiceEventArgs e)
        {
            if (e.IsTokenRefreshNeeded)
            {
                await SignInAsync();
            }
            else
            {
                // Future use
            }
        }

        private async void ApiService_RequestErrorOccurred(object sender, ApiServiceEventArgs e)
        {
            var message = "Unknown Server Error";

            if (e.IsBadRequest)
            {
                message = e.Message;
            }
            else if (e.IsServerError)
            {
                message = e.Message + "\r\n\nIf this continues to happen, please open a GitHub Issue and we'll investigate further (find the GitHub link on the About page).";
            }

            await new MessageDialog(message, "MVP API Request Error").ShowAsync();
        }

        public async Task<string> RequestAuthorizationAsync(string authCode, bool isRefresh = false)
        {
            var authorizationHeader = "";

            try
            {
                using var client = new HttpClient();

                // Construct the Form content
                var postContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("client_id", _clientId),
                    new KeyValuePair<string, string>("grant_type", isRefresh ? "refresh_token" : "authorization_code"),
                    new KeyValuePair<string, string>(isRefresh ? "refresh_token" : "code", authCode.Split('&')[0]),
                    new KeyValuePair<string, string>("redirect_uri", _redirectUrl)
                });

                // Variable to hold the response data
                var responseTxt = "";

                // Post the Form data
                using (var response = await client.PostAsync(new Uri(_accessTokenUrl), postContent))
                {
                    // Read the response
                    responseTxt = await response.Content.ReadAsStringAsync();
                }

                // Deserialize the parameters from the response
                var tokenData = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseTxt);

                // Ensure response has access token
                if (tokenData.ContainsKey("access_token"))
                {
                    // Store the expiration time of the token, currently 3600 seconds (an hour)
                    SaveSetting("expires_in", tokenData["expires_in"]);

                    // Store tokens (NOTE: The tokens are encrypted with Rijindel before storing in LocalFolder)
                    StoreToken("access_token", tokenData["access_token"]);
                    StoreToken("refresh_token", tokenData["refresh_token"]);

                    // We need to prefix the access token with the token type for the auth header. 
                    // Currently this is always "bearer", doing this to be more future proof
                    var tokenType = tokenData["token_type"];
                    var cleanedAccessToken = tokenData["access_token"].Split('&')[0];

                    // Build the Bearer authorization header
                    authorizationHeader = $"{tokenType} {cleanedAccessToken}";
                }
            }
            catch (HttpRequestException ex)
            {
                Crashes.TrackError(ex);

                await ex.LogExceptionWithUserMessage();

                if (ex.Message.Contains("401"))
                {
                    //TODO consider another message HTTP specific errors
                }

                Debug.WriteLine($"LoginWindow HttpRequestException: {ex}");
            }
            catch (Exception ex)
            {

                Crashes.TrackError(ex);

                await ex.LogExceptionWithUserMessage();
            }

            return authorizationHeader;
        }

        #region Encryption methods

        public bool SaveSetting(string key, string value)
        {
            try
            {
                var filePath = Path.Combine(_appDataFolder, "settings.json");

                Dictionary<string, string> settings = null;

                if (File.Exists(filePath))
                {
                    var json = "";
                    json = File.ReadAllText(filePath);
                    settings = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                }
                else
                {
                    settings = new Dictionary<string, string>();
                }

                settings[key] = value;

                var updatedJson = JsonConvert.SerializeObject(settings);

                File.WriteAllText(filePath, updatedJson);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public string LoadSetting(string key)
        {
            try
            {
                var filePath = Path.Combine(_appDataFolder, "settings.json");

                if (File.Exists(filePath))
                {
                    var json = File.ReadAllText(filePath);
                    var settings = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                    return settings.ContainsKey(key) ? settings[key] : null;
                }
                else
                {
                    var settings = new Dictionary<string, string>();
                    var json = JsonConvert.SerializeObject(settings);
                    File.WriteAllText(filePath, json);
                    return null;
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public bool StoreToken(string key, string value)
        {
            try
            {
                var filePath = Path.Combine(_appDataFolder, $"{key}.txt");
                var encryptedToken = EncryptString(value);

                File.WriteAllText(filePath, encryptedToken);

                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"StoreToken Exception: {e}");
                return false;
            }
        }

        public string LoadToken(string key)
        {
            try
            {
                var filePath = Path.Combine(_appDataFolder, $"{key}.txt");

                if (File.Exists(filePath))
                {
                    var storedValue = File.ReadAllText(filePath);
                    var decryptedToken = DecryptString(storedValue);
                    return decryptedToken;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"LoadToken Exception: {e}");
                return null;
            }
        }

        public bool DeleteToken(string key)
        {
            try
            {
                var filePath = Path.Combine(_appDataFolder, $"{key}.txt");

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"DeleteToken Exception: {e}");
                return false;
            }
        }

        private string EncryptString(string inputText)
        {
            var textBytes = Encoding.Unicode.GetBytes(inputText);
            var encryptedBytes = EncryptBytes(textBytes);

            Debug.WriteLine($"EncryptString complete: {encryptedBytes.Length} bytes");

            return Convert.ToBase64String(encryptedBytes);
        }

        private string DecryptString(string encryptedText)
        {
            // NOTE: This string is encrypted first, THEN converted to Base64 (not just obfuscated as Base64)
            var encryptedBytes = Convert.FromBase64String(encryptedText);
            var decryptedBytes = DecryptBytes(encryptedBytes);

            Debug.WriteLine($"DecryptString complete: {decryptedBytes.Length} bytes");
            return Encoding.Unicode.GetString(decryptedBytes, 0, decryptedBytes.Length);
        }

        private byte[] EncryptBytes(byte[] unencryptedData)
        {
            // I chose Rijndael instead of AES because of it's support for larger block size (AES only support 128)
            using (var cipher = new RijndaelManaged { Key = _symmetricKey, IV = _initializationVector })
            using (var cryptoTransform = cipher.CreateEncryptor())
            using (var memoryStream = new MemoryStream())
            using (var cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write))
            {
                cryptoStream.Write(unencryptedData, 0, unencryptedData.Length);
                cryptoStream.FlushFinalBlock();
                var encryptedBytes = memoryStream.ToArray();
                Debug.WriteLine($"EncryptBytes complete: {encryptedBytes.Length} bytes");
                return encryptedBytes;
            }
        }

        private byte[] DecryptBytes(byte[] encryptedBytes)
        {
            using (var cipher = new RijndaelManaged())
            using (var cryptoTransform = cipher.CreateDecryptor(_symmetricKey, _initializationVector))
            using (var memoryStream = new MemoryStream(encryptedBytes))
            using (var cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Read))
            {
                byte[] decryptedBytes = new byte[encryptedBytes.Length];
                int bytesRead = cryptoStream.Read(decryptedBytes, 0, decryptedBytes.Length);

                // Note - I'm using Take() to clean up junk bytes at the end of the array
                return decryptedBytes.Take(bytesRead).ToArray();
            }
        }

        #endregion
    }
}
