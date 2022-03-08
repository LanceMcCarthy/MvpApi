using MvpApi.Common.CustomEventArgs;
using MvpApi.Common.Interfaces;
using MvpApi.Common.Models.Navigation;
using MvpApi.Services.Apis;
using MvpApi.Services.Utilities;
using MvpCompanion.Maui.ViewModels;
using System.Text.Json;

namespace MvpCompanion.Maui;

public partial class ShellPage : Shell, INavigationHandler
{
    //private readonly WebView _webView;
    private readonly ShellViewModel _viewModel;

	public ShellPage()
	{
		InitializeComponent();
        _viewModel = new ShellViewModel();
        this.BindingContext = _viewModel;

        if (Device.Idiom == TargetIdiom.Phone)
        {
            CurrentItem = PhoneTabs;
        }

        Routing.RegisterRoute("login", typeof(Views.Login));
        Routing.RegisterRoute("home", typeof(Views.Home));
        Routing.RegisterRoute("upload", typeof(Views.Upload));
        Routing.RegisterRoute("detail", typeof(Views.Detail));
        Routing.RegisterRoute("help", typeof(Views.Help));
        Routing.RegisterRoute("profile", typeof(Views.Profile));
        Routing.RegisterRoute("about", typeof(Views.About));
        Routing.RegisterRoute("settings", typeof(Views.Settings));
        
        _viewModel = new ShellViewModel
        {
            NavigationHandler = this
        };

        this.GoToAsync("login");

        //_webView = new WebView();
        //_webView.Navigated += WebView_OnNavigated;
    }

	public void LoadView(ViewType viewType)
    {
        switch (viewType)
        {
            case ViewType.Home:
                GoToAsync("///home");
                break;
            case ViewType.Upload:
                GoToAsync("///upload");
                break;
            case ViewType.Detail:
                GoToAsync("///detail");
                break;
            case ViewType.Profile:
                GoToAsync("///profile");
                break;
            case ViewType.Help:
                GoToAsync("///help");
                break;
            case ViewType.Settings:
                GoToAsync("///settings");
                break;
            case ViewType.About:
                GoToAsync("///about");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(viewType), viewType, null);
        }
    }

    void TapGestureRecognizer_Tapped(System.Object sender, System.EventArgs e)
    {
        GoToAsync("///settings");
    }

    protected override bool OnBackButtonPressed()
    {
        // TODO Replace SideDrawer
        //if (SideDrawer.MainContent.GetType() != typeof(HomeView))
        //{
        //	SideDrawer.MainContent = new HomeView();
        //	return true;
        //}

        return base.OnBackButtonPressed();
	}
}

