using Windows.Foundation.Metadata;
using Microsoft.UI.Xaml;
using MvpApi.Services.Apis;
using MvpCompanion.UI.WinUI.Dialogs;
using MvpCompanion.UI.WinUI.ViewModels;

namespace MvpCompanion.UI.WinUI
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        public static Window CurrentWindow { get; private set; }

        public static MvpApiService ApiService { get; set; }
        
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            CurrentWindow = new MainWindow();
            
            CurrentWindow.Activate();
        }
    }
}
