using MvpCompanion.Maui.ViewModels;

namespace MvpCompanion.Maui.Views;

[QueryProperty(nameof(Operation), "signin")]
public partial class Login : ContentPage
{
    private static readonly string _scope = "wl.emails%20wl.basic%20wl.offline_access%20wl.signin";
    private static readonly string _clientId = "090fa1d9-3d6f-4f6f-a733-a8b8a3fe16ff";
    private readonly Uri _signInUrl = new($"https://login.live.com/oauth20_authorize.srf?client_id={_clientId}&redirect_uri=https:%2F%2Flogin.live.com%2Foauth20_desktop.srf&response_type=code&scope={_scope}");
    private readonly Uri _signOutUri = new($"https://login.live.com/oauth20_logout.srf?client_id={_clientId}&redirect_uri=https:%2F%2Flogin.live.com%2Foauth20_desktop.srf");

    private readonly LoginViewModel _viewModel;
    
    public Login()
	{
		InitializeComponent();
        _viewModel = new LoginViewModel();
        BindingContext = _viewModel;
    }

    public string Operation { get; set; }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        WebView1.Source = Operation == "signout"
            ? _signOutUri
            : _signInUrl;
    }

    public async void WebView_OnNavigated(object sender, WebNavigatedEventArgs e)
    {
        switch (e.Result)
        {
            case WebNavigationResult.Success:
                if (e.Url.Contains("code="))
                {
                    // old
                    //var myUri = new Uri(e.Url);
                    //var authCode = myUri.ExtractQueryValue("code");

                    // cross platform safe
                    var queryString = e.Url.Split('?')[1];
                    var queryDictionary = System.Web.HttpUtility.ParseQueryString(queryString);
                    var authCode = queryDictionary["code"];

                    // 
                    var authorizationHeader = await (App.Current.MainPage as ShellPage).RequestAuthorizationAsync(authCode);

                    if (!string.IsNullOrEmpty(authorizationHeader))
                    {
                        await (App.Current.MainPage as ShellPage).InitializeMvpApiAsync(authorizationHeader);
                    }
                    
                    await Shell.Current.GoToAsync("home");
                }
                else if (e.Url.Contains("lc="))
                {
                    // Redirect to signin page if there's a bounce

                    (sender as WebView).Source = _signInUrl;
                }
                break;
            case WebNavigationResult.Failure:
                break;
            case WebNavigationResult.Timeout:
                break;
            case WebNavigationResult.Cancel:
                break;
            default:
                break;
        }
    }
}