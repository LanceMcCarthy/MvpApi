using MvpApi.Services.Apis;

namespace MvpCompanion.Maui;

public partial class App : Application
{
	public static MvpApiService ApiService { get; set; }

	public App()
	{
		InitializeComponent();
        
        UserAppTheme = Preferences.Get("IsDarkMode", false) ? OSAppTheme.Dark : OSAppTheme.Light;

		MainPage = new ShellPage();
    }

    protected override void OnStart()
    {
    }

    protected override void OnSleep()
    {
    }

    protected override void OnResume()
    {
    }
}
