using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;

namespace MvpApi.UwpBackgroundTasks
{
    public sealed class UpdateTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var toastnotifier = ToastNotificationManager.CreateToastNotifier();
            var toastDescriptor = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);

            var txtNodes = toastDescriptor.GetElementsByTagName("text");
            txtNodes[0].AppendChild(toastDescriptor.CreateTextNode("MVP Companion app updated!"));
            txtNodes[1].AppendChild(toastDescriptor.CreateTextNode("The companion app has been updated for 2018-2019 use. Lots of fixes and workflow improvements, thank you for the great feedback!"));

            var toast = new ToastNotification(toastDescriptor);

            toastnotifier.Show(toast);
        }
    }
}