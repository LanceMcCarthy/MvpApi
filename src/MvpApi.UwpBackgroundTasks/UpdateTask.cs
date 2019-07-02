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
                                Text = "Welcome to the 2019-2020 MVP year! The MVP Companion is here to help keep your contributions updated."
                            },
                            new AdaptiveImage
                            {
                                Source = "https://dvlup.blob.core.windows.net/general-app-files/MVP%20Companion/MVPCompanion_1.9.2.png"
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