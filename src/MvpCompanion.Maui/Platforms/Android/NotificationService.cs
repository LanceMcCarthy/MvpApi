//#if ANDROID
using MvpCompanion.Maui.Services;
using Android.Widget;
using Android.Content;

namespace MvpCompanion.Maui
{

    public class NotificationService_Android : INotificationService
    {
        public void ShowNotification(string title, string body)
        {
            Toast.MakeText(Android.App.Application.Context, body, ToastLength.Long)?.Show();
        }
    }
}
//#endif
