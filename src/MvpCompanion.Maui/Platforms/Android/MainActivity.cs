using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Widget;

namespace MvpCompanion.Maui;

[IntentFilter(new[] { Platform.Intent.ActionAppAction }, Categories = new[] { Intent.CategoryDefault })]
[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        
        Platform.Init(this, savedInstanceState);

        //Platform.ActivityStateChanged += Platform_ActivityStateChanged;
    }

    protected override void OnResume()
    {
        base.OnResume();

        Platform.OnResume(this);
    }

    protected override void OnNewIntent(Intent intent)
    {
        base.OnNewIntent(intent);

        Platform.OnNewIntent(intent);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        Platform.ActivityStateChanged -= Platform_ActivityStateChanged;
    }

    void Platform_ActivityStateChanged(object sender, ActivityStateChangedEventArgs e)
    {
        Toast.MakeText(this, e.State.ToString(), ToastLength.Short)?.Show();
    }

    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
    {
        Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

        base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
    }
}