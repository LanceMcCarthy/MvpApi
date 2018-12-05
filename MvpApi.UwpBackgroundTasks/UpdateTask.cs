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
                                Text = "MVP Companion Updated!"
                            },
                            new AdaptiveText
                            {
                                Text = "Now using Rijndael encryption, login bug fixes and new acrylic accents. Profile editing coming soon!"
                            },
                            new AdaptiveImage
                            {
                                Source = "https://dvlup.blob.core.windows.net/general-app-files/MVP%20Companion/MVP_Companion_1.8.1.png"
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