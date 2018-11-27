using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Identity.Client;
using MvpApi.Services.Apis;
using MvpApi.Services.Utilities;

namespace MvpCompanion.Uwp
{
    public sealed partial class MainPage : Page
    {
        private MvpApiService _apiService;
        private OAuthHelper _oauthHelper;
        private AuthenticationResult _authResult;

        public MainPage()
        {
            InitializeComponent();
        }

        private async void GetMvpProfileButton_Click(object sender, RoutedEventArgs e)
        {
            if (_oauthHelper == null)
                return;

            BusyIndicator.IsActive = true;

            if (_apiService == null)
            {
                BusyIndicator.Content = "creating MVPService...";

                _apiService = new MvpApiService(_authResult.AccessToken);
            }
            
            BusyIndicator.Content = "getting profile...";

            var profile = await _apiService.GetProfileAsync();

            if (profile == null)
            {
                ResultText.Text = "MVP Api Service did not respond. Check Debug window for more details.";
            }
            else
            {
                ResultText.Text = $"Profile Name: {profile.DisplayName}{Environment.NewLine}";
                ResultText.Text += $"Headline: {profile.Headline}{Environment.NewLine}";
                ResultText.Text += $"First Awarded in: {profile.FirstAwardYear}{Environment.NewLine}";
                ResultText.Text += $"Bio: {profile.Biography}";
            }
            
            BusyIndicator.Content = "";
            BusyIndicator.IsActive = false;
        }

        private async void GetMvpContributionsButton_Click(object sender, RoutedEventArgs e)
        {
            if (_oauthHelper == null)
                return;

            BusyIndicator.IsActive = true;

            if (_apiService == null || _authResult.ExpiresOn < DateTime.Now)
            {
                BusyIndicator.Content = "creating MVPService...";
                _apiService = new MvpApiService(_authResult.AccessToken);
            }

            BusyIndicator.Content = "getting contributions...";

            var contributions = await _apiService.GetContributionsAsync(0, 10);

            if (contributions == null)
            {
                ResultText.Text = "MVP Api Service did not respond. Check Debug window for more details.";
            }
            else if (contributions?.TotalContributions < 1)
            {
                ResultText.Text = $"TotalContributions is zero";
            }
            else if(contributions.Contributions != null)
            {
                foreach (var contribution in contributions?.Contributions)
                {

                    ResultText.Text = $"{contribution.Description}{Environment.NewLine}";
                }
            }

            BusyIndicator.Content = "";
            BusyIndicator.IsActive = false;
        }

        private async void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            if (_oauthHelper == null)
            {
                BusyIndicator.Content = "logging in...";

                _oauthHelper = new OAuthHelper();
                _authResult = await _oauthHelper.LogInAsync();

                if (_authResult != null)
                {
                    var bearerWithToken = _authResult.CreateAuthorizationHeader();

                    _apiService = new MvpApiService(bearerWithToken);

                    ResultText.Text = _authResult.Account.Username + "is signed in";

                    TokenInfoText.Text = "";
                    TokenInfoText.Text += $"Username: {_authResult.Account.Username}{Environment.NewLine}";
                    TokenInfoText.Text += $"Token Expires: {_authResult.ExpiresOn.ToLocalTime()}{Environment.NewLine}";
                    TokenInfoText.Text += $"Authorization: Bearer {_authResult.AccessToken}{Environment.NewLine}";

                    SetLoggedInStatus(true);
                }
                else
                {
                    SetLoggedInStatus(false);
                }
            }
        }

        private async void SignOutButton_Click(object sender, RoutedEventArgs e)
        {
            if (_apiService != null)
            {
                var result = await _oauthHelper.LogOutAsync();

                if (result.Item1)
                {
                    SetLoggedInStatus(false);
                }

                ResultText.Text = result.Item2;
            }
        }
        
        private void SetLoggedInStatus(bool isLoggedIn)
        {
            if (isLoggedIn)
            {
                SignInButton.Visibility = Visibility.Collapsed;
                SignOutButton.Visibility = Visibility.Visible;

                GetMvpProfileButton.Visibility = Visibility.Visible;
                GetMvpContributionsButton.Visibility = Visibility.Visible;
            }
            else
            {
                SignInButton.Visibility = Visibility.Visible;
                SignOutButton.Visibility = Visibility.Collapsed;

                GetMvpProfileButton.Visibility = Visibility.Collapsed;
                GetMvpContributionsButton.Visibility = Visibility.Collapsed;
            }
        }

        // WORKS when using a token generated by MSFT Live endpoint.
        private async void ManualTestButton_Click(object sender, RoutedEventArgs e)
        {
            BusyIndicator.IsActive = true;
            BusyIndicator.Content = "getting profile...";
            var tokenCopiedFromMvpApiPortal = "";

            _apiService = new MvpApiService(tokenCopiedFromMvpApiPortal);

            var profile = await _apiService.GetProfileAsync();

            if (profile == null)
            {
                ResultText.Text = "MVP Api Service did not respond. Check Debug window for more details.";
            }
            else
            {
                ResultText.Text = $"Profile Name: {profile.DisplayName}{Environment.NewLine}";
                ResultText.Text += $"Headline: {profile.Headline}{Environment.NewLine}";
                ResultText.Text += $"First Awarded in: {profile.FirstAwardYear}{Environment.NewLine}";
                ResultText.Text += $"Bio: {profile.Biography}";
            }

            BusyIndicator.Content = "";
            BusyIndicator.IsActive = false;
        }
    }
}