using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;
using Microsoft.Toolkit.Uwp.Notifications;

namespace MvpApi.UwpBackgroundTasks
{
    public sealed class UpdateTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var toastContent = new ToastContent
            {
                Visual = new ToastVisual
                {
                    BindingGeneric = new ToastBindingGeneric
                    {
                        Children =
                        {
                            new AdaptiveText
                            {
                                Text = "MVP Companion update!"
                            },
                            new AdaptiveText
                            {
                                Text = "Brand new authentication system! It will automatically sign you in, or show a simple popup to authenticate."
                            },
                            new AdaptiveImage
                            {
                                Source = "https://dvlup.blob.core.windows.net/general-app-files/MVP%20Companion/MVP_Companion_1.7_update.gif"
                            }
                        }
                    }
                }
            };
            
            var toast = new ToastNotification(toastContent.GetXml());

            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }
    }
}