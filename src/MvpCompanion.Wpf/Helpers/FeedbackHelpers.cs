using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace MvpCompanion.Wpf.Helpers
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
                    _current ??= new FeedbackHelpers();
                }

                return _current;
            }
        }

        public void EmailErrorMessage(string message)
        {
            // Need to escape any invalid characters
            var formattedText = Uri.EscapeUriString(message);

            // make sure we're not over the 255 character limit for url encoding
            if (formattedText.Length >= 255)
            {
                formattedText = formattedText.Substring(0,254);
            }

            var uri = new Uri($"mailto:awesome.apps@outlook.com?subject=MVP%20Companion%20Error&body={formattedText}", UriKind.Absolute);

            Process.Start($"explorer.exe {uri}");
        }

        public void EmailFeedbackMessage()
        {
            var uri = new Uri($"mailto:awesome.apps@outlook.com?subject=MVP%20Companion%20Feedback&body=[write%20message%20here]", UriKind.Absolute);

            Process.Start($"explorer.exe {uri}");
        }

        public void CopyToClipboard(string textToCopy)
        {
            Clipboard.SetData(DataFormats.Text, textToCopy);
            Clipboard.Flush();
        }
    }
}