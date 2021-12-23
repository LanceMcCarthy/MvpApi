using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.UI.Xaml;
using MvpApi.Services.Apis;
using MvpCompanion.UI.WinUI.Common;
using MvpCompanion.UI.WinUI.Helpers;

namespace MvpCompanion.UI.WinUI
{
    public partial class App : Application
    {
        public static Window CurrentWindow { get; private set; }

        public static MvpApiService ApiService { get; set; }

        public App()
        {
            InitializeComponent();
            UnhandledException += App_UnhandledException;
        }
        
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            AppCenter.Start(ExternalConstants.AppCenterId, typeof(Analytics), typeof(Crashes));

            CurrentWindow = new MainWindow();
            
            CurrentWindow.Activate();
        }

        private async void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            e.Handled = true;

            await e.Exception.LogExceptionWithUserMessage();
        }
    }
}
