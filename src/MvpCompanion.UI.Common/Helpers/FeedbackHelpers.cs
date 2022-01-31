using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Email;
using Windows.Storage;
using Windows.System;
using Windows.UI.ViewManagement;
using Microsoft.Toolkit;

namespace MvpCompanion.UI.Common.Helpers
{
    public sealed class FeedbackHelpers
    {
        private static volatile FeedbackHelpers _current;
        private static readonly object SyncRoot = new object();

        private FeedbackHelpers() { }

        public static FeedbackHelpers Current
        {
            get
            {
                if (_current != null)
                    return _current;

                lock (SyncRoot)
                {
                    if (_current == null)
                        _current = new FeedbackHelpers();
                }

                return _current;
            }
        }

        public async Task<bool> EmailErrorMessageAsync(string message)
        {
            // Need to escape any invalid characters
            var formattedText = Uri.EscapeUriString(message);

            // make sure we're not over the 255 character limit for url encoding
            if (formattedText.Length >= 255)
            {
                formattedText = formattedText.Truncate(254);
            }

            var uri = new Uri($"mailto:awesome.apps@outlook.com?subject=MVP%20Companion%20Error&body={formattedText}", UriKind.Absolute);

            var options = new LauncherOptions
            {
                DesiredRemainingView = ViewSizePreference.UseHalf,
                DisplayApplicationPicker = true,
                PreferredApplicationPackageFamilyName = "microsoft.windowscommunicationsapps_8wekyb3d8bbwe",
                PreferredApplicationDisplayName = "Mail"
            };

            var result = await Launcher.LaunchUriAsync(uri, options);

            // TODO log success or failure of email compose

            return result;
        }

        public async Task EmailErrorWithAttachmentAsync(string subject, string body, StorageFile file)
        {
            var emailMessage = new EmailMessage
            {
                Body = body, 
                Subject = subject
            };

            emailMessage.To.Add(new EmailRecipient("awesome.apps@outlook.com"));
            
            emailMessage.Attachments.Add(new EmailAttachment
            {
                FileName = file.DisplayName,
                Data = file
            });

            await EmailManager.ShowComposeNewEmailAsync(emailMessage);
        }

        public async Task<bool> EmailFeedbackMessageAsync()
        {
            var options = new LauncherOptions
            {
                DesiredRemainingView = ViewSizePreference.UseHalf,
                DisplayApplicationPicker = true,
                PreferredApplicationPackageFamilyName = "microsoft.windowscommunicationsapps_8wekyb3d8bbwe",
                PreferredApplicationDisplayName = "Mail"
            };

            var uri = new Uri($"mailto:awesome.apps@outlook.com?subject=MVP%20Companion%20Feedback&body=[write%20message%20here]", UriKind.Absolute);
            var result = await Launcher.LaunchUriAsync(uri, options);

            // TODO log success or failure of email compose

            return result;
        }

        public void CopyToClipboard(string textToCopy)
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(textToCopy);

            Clipboard.SetContent(dataPackage);
            Clipboard.Flush();
        }
    }
}