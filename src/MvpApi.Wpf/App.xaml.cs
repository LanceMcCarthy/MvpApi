using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.UI.Popups;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Crashes;
using MvpApi.Common.CustomEventArgs;
using MvpApi.Services.Apis;
using MvpApi.Services.Data;
using MvpApi.Services.Utilities;
using MvpApi.Wpf.Helpers;
using MvpApi.Wpf.Models;
using Newtonsoft.Json;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.SplashScreen;

namespace MvpApi.Wpf
{
    public partial class App : Application
    {
        public static LoginWindow MainLoginWindow { get; private set; }

        public App()
        {
           this.InitializeComponent();
           MainLoginWindow = new LoginWindow();
        }

        public static MvpApiService ApiService { get; set; }

        protected override async void OnStartup(StartupEventArgs e)
        {
            AppCenter.Start(
                "fb05c4f9-9e96-4fc2-80c4-d99e2227a54b",
                typeof(Analytics), 
                typeof(Crashes));

            bool didAppCrash = await Crashes.HasCrashedInLastSessionAsync();

            if (didAppCrash)
            {
                ErrorReport crashReport = await Crashes.GetLastSessionCrashReportAsync();
            }

            var dataContext = (SplashScreenDataContext)RadSplashScreenManager.SplashScreenDataContext;
            dataContext.ImagePath = "/MvpApi.Wpf;component/Images/HeroBackground.png";
            dataContext.Content = "starting up...";
            dataContext.Footer = new WelcomeMessageService().GetRandomMessage();

            RadSplashScreenManager.Show();


            var refreshToken = StorageHelpers.Instance.LoadToken("refresh_token");

            // We have a refresh token from a previous session
            if (!string.IsNullOrEmpty(refreshToken))
            {
                Microsoft.AppCenter.Analytics.Analytics.TrackEvent("Refreshing Session On App Start");

                ((SplashScreenDataContext)RadSplashScreenManager.SplashScreenDataContext).Content = "Signing in...";

                var authorizationHeader = await MainLoginWindow.RequestAuthorizationAsync(refreshToken);

                // If the bearer token was returned
                if (!string.IsNullOrEmpty(authorizationHeader))
                {
                    await MainLoginWindow.InitializeMvpApiAsync(authorizationHeader);
                }
                else
                {
                    await MainLoginWindow.SignInAsync();
                }
            }
            else
            {
                await MainLoginWindow.SignInAsync();
            }


            RadSplashScreenManager.Close();

            base.OnStartup(e);
        }


        private static async void ApiService_AccessTokenExpired(object sender, ApiServiceEventArgs e)
        {
            if (e.IsTokenRefreshNeeded)
            {
                await MainLoginWindow.SignInAsync();
            }
            else
            {
                // Future use
            }
        }

        private static async void ApiService_RequestErrorOccurred(object sender, ApiServiceEventArgs e)
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

        //private static async Task<string> RequestAuthorizationAsync(string authCode, bool isRefresh = false)
        //{
        //    var authorizationHeader = "";

        //    try
        //    {
        //        using var client = new HttpClient();

        //        // Construct the Form content
        //        var postContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
        //        {
        //            new KeyValuePair<string, string>("client_id", "090fa1d9-3d6f-4f6f-a733-a8b8a3fe16ff"),
        //            new KeyValuePair<string, string>("grant_type", isRefresh ? "refresh_token" : "authorization_code"),
        //            new KeyValuePair<string, string>(isRefresh ? "refresh_token" : "code", authCode.Split('&')[0]),
        //            new KeyValuePair<string, string>("redirect_uri", "https://login.live.com/oauth20_desktop.srf")
        //        });

        //        // Variable to hold the response data
        //        var responseTxt = "";

        //        // Post the Form data
        //        using (var response = await client.PostAsync(new Uri("https://login.live.com/oauth20_token.srf"), postContent))
        //        {
        //            // Read the response
        //            responseTxt = await response.Content.ReadAsStringAsync();
        //        }

        //        // Deserialize the parameters from the response
        //        var tokenData = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseTxt);

        //        // Ensure response has access token
        //        if (tokenData.ContainsKey("access_token"))
        //        {
        //            // Store the expiration time of the token, currently 3600 seconds (an hour)
        //            StorageHelpers.Instance.SaveSetting("expires_in", tokenData["expires_in"]);

        //            // Store tokens (NOTE: The tokens are encrypted with Rijindel before storing in LocalFolder)
        //            StorageHelpers.Instance.StoreToken("access_token", tokenData["access_token"]);
        //            StorageHelpers.Instance.StoreToken("refresh_token", tokenData["refresh_token"]);

        //            // We need to prefix the access token with the token type for the auth header. 
        //            // Currently this is always "bearer", doing this to be more future proof
        //            var tokenType = tokenData["token_type"];
        //            var cleanedAccessToken = tokenData["access_token"].Split('&')[0];

        //            // Build the Bearer authorization header
        //            authorizationHeader = $"{tokenType} {cleanedAccessToken}";
        //        }
        //    }
        //    catch (HttpRequestException ex)
        //    {
        //        Crashes.TrackError(ex);

        //        await ex.LogExceptionWithUserMessage();

        //        if (ex.Message.Contains("401"))
        //        {
        //            //TODO consider another message HTTP specific errors
        //        }

        //        Debug.WriteLine($"LoginDialog HttpRequestException: {ex}");
        //    }
        //    catch (Exception ex)
        //    {

        //        Crashes.TrackError(ex);

        //        await ex.LogExceptionWithUserMessage();
        //    }

        //    return authorizationHeader;
        //}
    }
}
