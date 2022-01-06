//using Microsoft.AppCenter;
//using Microsoft.AppCenter.Analytics;
//using Microsoft.AppCenter.Crashes;

using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Popups;
using Microsoft.UI.Xaml;
using MvpApi.Services.Apis;
using MvpCompanion.UI.WinUI.Helpers;
using WinUIEx;

namespace MvpCompanion.UI.WinUI
{
    public partial class App : Application
    {
        public static Window CurrentWindow { get; private set; }

        public static MvpApiService ApiService { get; set; }

        public App()
        {
            SetTheme();
            InitializeComponent();
            UnhandledException += App_UnhandledException;
        }
        
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            //AppCenter.Start(ExternalConstants.AppCenterId, typeof(Analytics), typeof(Crashes));

            CurrentWindow = new MainWindow();

            CurrentWindow.SetWindowSize(1200, 900);
            CurrentWindow.CenterOnScreen();
            
            CurrentWindow.Activate();
        }

        private async void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            e.Handled = true;

            await e.Exception.LogExceptionWithUserMessage();
        }

        public static async Task ShowMessageAsync(string body, string title = "")
        {
            var md = string.IsNullOrEmpty(title) 
                ? new MessageDialog(body) 
                : new MessageDialog(body, title);

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(CurrentWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(md, hwnd);

            await md.ShowAsync();
        }

        public static void SetTheme()
        {
            if (ApplicationData.Current.LocalSettings.Values.TryGetValue("UseDarkTheme", out var rawValue))
            {
                Application.Current.RequestedTheme = (bool)rawValue ? ApplicationTheme.Dark : ApplicationTheme.Light;
            }
            else
            {
                Application.Current.RequestedTheme = ApplicationTheme.Light;
            }
        }
    }
}
