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
                    SetLoggedInStatus();
                }

                ResultText.Text = result.Item2;
            }
        }
        
        private void SetLoggedInStatus(bool isLoggedIn)
        {
            TokenInfoText.Text = "";

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
            var tokenCopiedFromMvpApiPortal = "EwAoA61DBAAUcSSzoTJJsy+XrnQXgAKO5cj4yc8AAbvJWkc4qLdSLAufZ17LoE+xdF4XFpgruR3xP7bhrHJuA/AXKSDJ9ZeG3FDoFbA3VVqkngpF0ehExwmvJb3wimYze9p6IYx3cvNSDbsybw8h4T8In7OhcjQh+U3rs5fY2wT0+RbqUDzixn6RzXELTsB0F/C0Yr5StnjOs77IAW0qmGvRqki7eJ142YEgr9yRIBWBnzK3UHDkCsfPXmqphSE2eHpedcIvLf2gJO8PBciQ5wwOAq43APieIlBLyshbIlQj+U+ztZbodYDrzUxBj6vFl20ODyHFnD7RrUD3jNeVEQ250Me+umAjXpLW3mPdPZDaqCfk+towMzbR9LfRPG0DZgAACLBe613ym4jB+AHUZko6BgHin2Ik9lvkZeKjaziw39BO23BaG2UXW6ApXrBdl7LdRBCFL614Znhd/v9rb5uxjh0Z09K75pblUHB9d+eEyq/K7FwK3cIpHSx6kKLiut9grcYVaIzkvGaz7AgvYPotsiriV1mOwbrQLlQnGY1y+2i4f6xuIq9+DoAnP5Zo5KRk18l7bh9lvzYAXD8BrZsf7Mq2JLU3DHAYP4KFnk5FUS0f3o86g7QQqYwThvGWpvVzT7E54hQpsqbXLa4VLGoDICKwmdSxviOE1AhBNtdTCOMNOej2Uoa50Yw5IzPoqLewLdJTq/1bCwBW31CgPZDaWkopI/95dxf5YLN8joXVDPsMe8QtNhwihYAj9ei7s3ji7VkajZUZNIXJ1SXyu64RZ5eSH4tbhGbs7pOdvCwM8z2Jb32MXWjzzrmAJopJxlrhkD7RqMWeSJcJHlzZKtSA2Xbo5hURgN2V+O9vl72o5g1QywI7AC+tCuSSfpo47U5S20JeD9NNWFqzpoLxm2y1BNslo0/f43FkwcnTMJ2bMZzCia25LWorb/3iIaDXmmNHZVhWwBb6sbug+DDDgtHju1hPXNWExdsr/3IvJH5w+NCh91FKVmL6vC5iK7SUwSNXqzjVIBSjs07fe3aU2EehUunR2+kGASDBsft5BUolHa+sxMQiAg==";

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