using MvpApi.Services.Apis;
using Xamarin.Forms;

namespace MvpApi.Forms.Portable
{
    public partial class App : Application
    {
        public static MvpApiService ApiService { get; set; }

        public App()
        {
            InitializeComponent();
            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
