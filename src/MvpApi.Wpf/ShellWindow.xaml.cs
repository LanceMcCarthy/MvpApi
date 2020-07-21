using System;
using System.Threading.Tasks;
using System.Windows;
using Windows.UI.Popups;
using MvpApi.Wpf.Helpers;

namespace MvpApi.Wpf
{
    public partial class ShellWindow : Window
    {
        private static readonly string _scope = "wl.emails%20wl.basic%20wl.offline_access%20wl.signin";
        private static readonly string _clientId = "090fa1d9-3d6f-4f6f-a733-a8b8a3fe16ff";
        private readonly string _redirectUrl = "https://login.live.com/oauth20_desktop.srf";
        private readonly string _accessTokenUrl = "https://login.live.com/oauth20_token.srf";
        private readonly Uri _signInUri = new Uri($"https://login.live.com/oauth20_authorize.srf?client_id={_clientId}&redirect_uri=https:%2F%2Flogin.live.com%2Foauth20_desktop.srf&response_type=code&scope={_scope}");
        private readonly Uri _signOutUri = new Uri($"https://login.live.com/oauth20_logout.srf?client_id={_clientId}&redirect_uri=https:%2F%2Flogin.live.com%2Foauth20_desktop.srf");

        public static ShellWindow Instance { get; set; }

        public ShellWindow()
        {
            Instance = this;
            InitializeComponent();

            Loaded += ShellWindow_Loaded;
        }

        private async void ShellWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (NetworkHelper.Current.CheckInternetConnection())
            {
                await SignInAsync();
            }
            else
            {
                await new MessageDialog("This application requires an internet connection. Please check your connection and launch the app again.", "No Internet").ShowAsync();
            }
        }

        public async Task SignInAsync()
        {

        }
    }
}
