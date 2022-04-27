//#if WINDOWS
using Microsoft.Toolkit.Uwp.Notifications;
using MvpCompanion.Maui.Services;

namespace MvpCompanion.Maui;

public class NotificationService_WinUI : INotificationService
{
    public void ShowNotification(string title, string body)
    {
        new ToastContentBuilder()
            .AddToastActivationInfo(null, ToastActivationType.Foreground)
            .AddAppLogoOverride(new Uri("ms-appx:///Assets/Square150x150Logo.scale-100.png"))
            .AddText(title, hintStyle: AdaptiveTextStyle.Header)
            .AddText(body, hintStyle: AdaptiveTextStyle.Body)
            .Show();
    }
}
//#endif
