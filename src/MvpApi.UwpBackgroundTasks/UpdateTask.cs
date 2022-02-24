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
                                Text = "MVP Companion Updated"
                            },
                            new AdaptiveText
                            {
                                Text = "You have the latest and greatest version. MVP Companion helps you keep your contributions updated throughout the year (or in the last 2 weeks of panic 😁)."
                            },
                            new AdaptiveImage
                            {
                                // New Home page experience screenshot
                                Source = "https://user-images.githubusercontent.com/3520532/153973369-b3a44f1d-024e-4243-a363-51054454cd09.png"
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