using System;
using System.Windows;
using Windows.UI.Popups;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Crashes;
using MvpApi.Common.CustomEventArgs;
using MvpApi.Services.Apis;
using MvpApi.Services.Data;
using MvpApi.Services.Utilities;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.SplashScreen;

namespace MvpApi.Wpf
{
    public partial class App : Application
    {
        public static MvpApiService ApiService { get; set; }

        public static LoginWindow MainLoginWindow { get; private set; }

        public App()
        {
           this.InitializeComponent();
           MainLoginWindow = new LoginWindow();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            AppCenter.Start(
                "fb05c4f9-9e96-4fc2-80c4-d99e2227a54b",
                typeof(Analytics), 
                typeof(Crashes));

            var welcomeMessage = new WelcomeMessageService().GetRandomMessage();
            ((SplashScreenDataContext)RadSplashScreenManager.SplashScreenDataContext).Footer = welcomeMessage.Message;
            ((SplashScreenDataContext)RadSplashScreenManager.SplashScreenDataContext).HorizontalFooterAlignment = HorizontalAlignment.Center;

            ((SplashScreenDataContext)RadSplashScreenManager.SplashScreenDataContext).ImagePath = "/MvpApi.Wpf;component/Images/HeroBackground.png";
            ((SplashScreenDataContext)RadSplashScreenManager.SplashScreenDataContext).Content = "starting up...";


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

            this.MainWindow = new ShellWindow();
            this.MainWindow.Show();

            // TODO Ask user to send crash report from previous crash

            bool didAppCrash = await Crashes.HasCrashedInLastSessionAsync();

            if (didAppCrash)
            {
                ErrorReport crashReport = await Crashes.GetLastSessionCrashReportAsync();
            }

            base.OnStartup(e);
        }


        

    }
}
