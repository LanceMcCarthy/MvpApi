using System.Windows;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using MvpApi.Services.Apis;

namespace MvpApi.Wpf
{
    public partial class App : Application
    {
        public App()
        {
           this.InitializeComponent();
        }

        public static MvpApiService ApiService { get; set; }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            AppCenter.Start(
                "fb05c4f9-9e96-4fc2-80c4-d99e2227a54b",
                typeof(Analytics), 
                typeof(Crashes));

            bool didAppCrash = await Crashes.HasCrashedInLastSessionAsync();

            if (didAppCrash)
            {
                ErrorReport crashReport = await Crashes.GetLastSessionCrashReportAsync();
            }
        }
    }
}
