using System.Windows;
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

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
        }
    }
}
