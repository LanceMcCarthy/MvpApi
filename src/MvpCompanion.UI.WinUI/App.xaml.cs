using Microsoft.UI.Xaml;
using MvpApi.Services.Apis;
using MvpApi.Services.Utilities;
using MvpCompanion.UI.WinUI.Dialogs;
using MvpCompanion.UI.WinUI.Views;

namespace MvpCompanion.UI.WinUI
{
    public partial class App : Application
    {
        private Window mainWindow;

        public App()
        {
            this.InitializeComponent();
        }

        public static MvpApiService ApiService { get; set; }
        
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            mainWindow = new MainWindow();
            mainWindow.Activate();
        }
    }
}
