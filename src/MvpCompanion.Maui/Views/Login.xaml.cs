using MvpCompanion.Maui.Models.Authentication;
using MvpCompanion.Maui.ViewModels;
using System.Web;

namespace MvpCompanion.Maui.Views;

public partial class Login : ContentPage, IQueryAttributable
{
    private const string _scope = "wl.emails%20wl.basic%20wl.offline_access%20wl.signin";
    private const string _clientId = "090fa1d9-3d6f-4f6f-a733-a8b8a3fe16ff";
    private readonly Uri _signInUrl = new($"https://login.live.com/oauth20_authorize.srf?client_id={_clientId}&redirect_uri=https:%2F%2Flogin.live.com%2Foauth20_desktop.srf&response_type=code&scope={_scope}");
    private readonly Uri _signOutUri = new($"https://login.live.com/oauth20_logout.srf?client_id={_clientId}&redirect_uri=https:%2F%2Flogin.live.com%2Foauth20_desktop.srf");
    
    private readonly LoginViewModel _viewModel;

    private string _operation;

    public Login()
    {
		InitializeComponent();

        BindingContext = _viewModel = new LoginViewModel();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query["operation"] is string val)
        {
            _operation = HttpUtility.UrlDecode(val);
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        WebView1.Source = _operation == "signout"
            ? _signOutUri
            : _signInUrl;
    }

    public async void WebView1_Navigated(object sender, WebNavigatedEventArgs e)
    {
        switch (e.Result)
        {
            case WebNavigationResult.Success:
                if (e.Url.Contains("code="))
                {
                    var queryString = e.Url.Split('?')[1];
                    var queryDictionary = HttpUtility.ParseQueryString(queryString);
                    var authCode = queryDictionary["code"];

                    // Use Microsoft OAuth code to authenticate with the MVP API portal 
                    var authorizationHeader = await AuthHelpers.RequestAuthorizationAsync(authCode);

                    if (!string.IsNullOrEmpty(authorizationHeader))
                    {
                        _viewModel.IsBusy = true;
                        _viewModel.IsBusyMessage = "authenticating...";

                        App.StartupApiService(authorizationHeader);

                        (Shell.Current.BindingContext as ShellViewModel).IsLoggedIn = true;

                        _viewModel.IsBusyMessage = "downloading profile info...";
                        (Shell.Current.BindingContext as ShellViewModel).Mvp = await App.ApiService.GetProfileAsync();

                        _viewModel.IsBusyMessage = "downloading profile image...";
                        (Shell.Current.BindingContext as ShellViewModel).ProfileImagePath = await App.ApiService.DownloadAndSaveProfileImage();

                        _viewModel.IsBusyMessage = "";
                        _viewModel.IsBusy = false;

                        await Shell.Current.GoToAsync("..");
                    }
                }
                else if (e.Url.Contains("lc="))
                {
                    // Redirect to signin page if there's a bounce
                    (sender as WebView).Source = _signInUrl;
                }
                break;
            case WebNavigationResult.Failure:
            case WebNavigationResult.Timeout:
            case WebNavigationResult.Cancel:
            default:
                break;
        }
    }

}