using Microsoft.UI.Xaml;
using MvpApi.Services.Apis;
using MvpApi.Services.Utilities;

namespace MvpCompanion.UI.WinUI
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

        protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            var refreshToken = StorageHelpers.Instance.LoadToken("refresh_token");

            // We have a refresh token from a previous session
            if (!string.IsNullOrEmpty(refreshToken))
            {
                var authorizationHeader = await LoginWindow.RequestAuthorizationAsync(refreshToken);

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

            m_window = new MainWindow();
            m_window.Activate();

            // TODO Ask user to send crash report from previous crash

            //bool didAppCrash = await Crashes.HasCrashedInLastSessionAsync();

            //if (didAppCrash)
            //{
            //    ErrorReport crashReport = await Crashes.GetLastSessionCrashReportAsync();
            //}
        }

        private Window m_window;
    }
}
