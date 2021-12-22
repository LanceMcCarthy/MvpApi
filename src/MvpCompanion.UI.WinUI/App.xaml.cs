using Microsoft.UI.Xaml;
using MvpApi.Services.Apis;

namespace MvpCompanion.UI.WinUI
{
    public partial class App : Application
    {
        private Window mainWindow;

        public App()
        {
            InitializeComponent();
        }

        public static MvpApiService ApiService { get; set; }
        
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            mainWindow = new MainWindow();
            mainWindow.Activate();
        }
    }
}
