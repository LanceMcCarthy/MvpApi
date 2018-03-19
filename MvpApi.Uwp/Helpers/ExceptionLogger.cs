using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;

namespace MvpApi.Uwp.Helpers
{
    public static class ExceptionLogger
    {
        public static void LogException(this Exception currentException)
        {
            var exceptionMessage = CreateErrorMessage(currentException);

            PurgeLogFiles();

            LogFileWrite(exceptionMessage);
        }

        public static async Task LogExceptionWithUserMessage(this Exception exception, string dialogTitle, string dialogMessage)
        {
            var exceptionMessage = CreateErrorMessage(exception);

            PurgeLogFiles();

            LogFileWrite(exceptionMessage);
            
            var md = new MessageDialog(dialogMessage, dialogTitle);

            md.Commands.Add(new UICommand("yes"));

            var result = await md.ShowAsync();

            if (result.Label == "yes")
            {
#if DEBUG
                var text = await DiagnosticsHelper.DumpAsync(exception, true);
#else

                var text = await DiagnosticsHelper.DumpAsync(e.Exception);
#endif

                await ReportErrorMessage(text);
            }
        }

        private static async Task<bool> ReportErrorMessage(string detailedErrorMessage)
        {
            var uri = new Uri(string.Format("mailto:awesome.apps@outlook.com?subject=MVP_Companion&body={0}", detailedErrorMessage), UriKind.Absolute);

            var options = new Windows.System.LauncherOptions
            {
                DesiredRemainingView = ViewSizePreference.UseHalf, DisplayApplicationPicker = true, PreferredApplicationPackageFamilyName = "microsoft.windowscommunicationsapps_8wekyb3d8bbwe", PreferredApplicationDisplayName = "Mail"
            };

            return await Windows.System.Launcher.LaunchUriAsync(uri, options);
        }
        
        private static string CreateErrorMessage(Exception currentException)
        {
            var messageBuilder = new StringBuilder();

            try
            {
                messageBuilder.AppendLine("-----------------------------------------------------------------");
                messageBuilder.AppendLine("Source: " + currentException.Source.Trim());
                messageBuilder.AppendLine("Date Time: " + DateTime.Now);
                messageBuilder.AppendLine("-----------------------------------------------------------------");
                messageBuilder.AppendLine("Method: " + currentException.Message.Trim());
                messageBuilder.AppendLine("Exception :: " + currentException);

                if(currentException.InnerException != null)
                {
                    messageBuilder.AppendLine("InnerException :: " + currentException.InnerException);
                }

                messageBuilder.AppendLine("");

                return messageBuilder.ToString();
            }
            catch
            {
                messageBuilder.AppendLine("Exception:: Unknown Exception.");
                return messageBuilder.ToString();
            }
        }
        
        private static async void LogFileWrite(string exceptionMessage)
        {
            try
            {
                var fileName = "VideoDiaryError-Log" + "-" + DateTime.Today.ToString("yyyyMMdd") + "." + "log";
                var localFolder = ApplicationData.Current.LocalFolder;
                var logFolder = await localFolder.CreateFolderAsync("Logs", CreationCollisionOption.OpenIfExists);
                var logFile = await logFolder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);

                if(!string.IsNullOrEmpty(exceptionMessage))
                {
                    await FileIO.AppendTextAsync(logFile, exceptionMessage);
                }
            }
            catch(Exception ex)
            {
                Debugger.Break();
            }
        }
        
        public static async void PurgeLogFiles()
        {
            var logFolder = ApplicationData.Current.LocalFolder;

            try
            {
                var daysToKeepLog = 5;
                var todaysDate = DateTime.Now.Date;

                logFolder = await logFolder.GetFolderAsync("Logs");
                IReadOnlyList<StorageFile> files = await logFolder.GetFilesAsync();

                if (files.Count < 1) return;

                foreach (var file in files)
                {
                    var basicProperties = await file.GetBasicPropertiesAsync();

                    if (file.FileType == ".log")
                    {
                        if (DateTime.Compare(todaysDate, basicProperties.DateModified.AddDays(daysToKeepLog).DateTime.Date) >= 0)
                        {
                            await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
                        }
                    }
                }
            }
            catch (Exception)
            {
                Debugger.Break();
            }
        }
    }
}
