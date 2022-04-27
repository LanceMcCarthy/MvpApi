using Foundation;
using UIKit;

namespace MvpCompanion.Maui;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();


    // See https://docs.microsoft.com/en-us/xamarin/essentials/web-authenticator?context=xamarin%2Fxamarin-forms&tabs=ios
    public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
    {
        return Platform.OpenUrl(app, url, options) || base.OpenUrl(app, url, options);
    }

    public override bool ContinueUserActivity(UIApplication application, NSUserActivity userActivity, UIApplicationRestorationHandler completionHandler)
    {
        return Platform.ContinueUserActivity(application, userActivity, completionHandler) || base.ContinueUserActivity(application, userActivity, completionHandler);
    }
}
