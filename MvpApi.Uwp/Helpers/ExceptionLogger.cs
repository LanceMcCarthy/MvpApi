using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Popups;

namespace MvpApi.Uwp.Helpers
{
    public static class ExceptionLogger
    {
        public static async Task LogExceptionAsync(this Exception currentException)
        {
            var exceptionMessage = CreateErrorMessage(currentException);

            await LogFileWriteAsync(exceptionMessage);
        }

        /// <summary>
        /// Easy to use Exception logger that shows the user an error message with the following MessageDialog
        /// Title: Unexpected Error
        /// Message: Sorry, there has been an unexpected error. If you'd like to send a technical summary to the app development team, click Yes.
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static async Task LogExceptionWithUserMessage(this Exception exception)
        {
            if(exception == null)
                throw new ArgumentNullException(nameof(exception));

            var exceptionMessage = CreateErrorMessage(exception);

            await LogFileWriteAsync(exceptionMessage);

            var md = new MessageDialog(
                "Sorry, there has been an unexpected error. If you'd like to send a technical summary to the app development team, click Yes.", 
                "Unexpected Error");

            md.Commands.Add(new UICommand("yes (summary)"));
            md.Commands.Add(new UICommand("yes (full)"));
            md.Commands.Add(new UICommand("no"));

            var result = await md.ShowAsync();

            if (result.Label == "yes (summary)")
            {
                await FeedbackHelpers.Current.EmailErrorMessageAsync(exceptionMessage);
            }
            else if (result.Label == "yes (full)")
            {
                var text = await DiagnosticsHelper.DumpAsync(exception);
                await FeedbackHelpers.Current.EmailErrorMessageAsync(exceptionMessage + "\r\n\n" + text);
            }
        }

        /// <summary>
        /// Easy to use Exception logger that shows the user a custom error message
        /// </summary>
        /// <param name="exception">Exception that occurs</param>
        /// <param name="dialogTitle">MessageDialog's title</param>
        /// <param name="dialogMessage">MessageDialog's message</param>
        /// <returns>Task</returns>
        public static async Task LogExceptionWithUserMessage(this Exception exception, string dialogMessage, string dialogTitle)
        {
            if(exception == null)
                throw new ArgumentNullException(nameof(exception));

            if (string.IsNullOrEmpty(dialogTitle))
                throw new ArgumentNullException(nameof(dialogTitle));

            if (string.IsNullOrEmpty(dialogMessage))
                throw new ArgumentNullException(nameof(dialogMessage));

            if (string.IsNullOrEmpty(dialogMessage))
                dialogMessage = "Sorry, there has been an unexpected error. If you'd like to send a technical summary to the app development team, click Yes.";

            var exceptionMessage = CreateErrorMessage(exception);
            
            // Manages and saves local log files
            await LogFileWriteAsync(exceptionMessage);
            
            var md = new MessageDialog(dialogMessage, dialogTitle);

            md.Commands.Add(new UICommand("yes (summary)"));
            md.Commands.Add(new UICommand("yes (full)"));
            md.Commands.Add(new UICommand("no"));

            var result = await md.ShowAsync();

            if (result.Label == "yes (summary)")
            {
                await FeedbackHelpers.Current.EmailErrorMessageAsync(exceptionMessage);
            }
            else if (result.Label == "yes (full)")
            {
                var text = await DiagnosticsHelper.DumpAsync(exception);
                await FeedbackHelpers.Current.EmailErrorMessageAsync(exceptionMessage + "\r\n\n" + text);
            }
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

                messageBuilder.AppendLine(" ");

                return messageBuilder.ToString();
            }
            catch
            {
                messageBuilder.AppendLine("Exception:: Unknown Exception.");
                return messageBuilder.ToString();
            }
        }
        
        private static async Task LogFileWriteAsync(string exceptionMessage)
        {
            try
            {
                await PurgeLogFilesAsync();

                var fileName = "MVPCompanion_ErrorLog" + "_" + DateTime.Today.ToString("yyyyMMdd") + "." + "log";
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
#if DEBUG
                Debugger.Break();
#endif
            }
        }
        
        private static async Task PurgeLogFilesAsync()
        {
            var localFolder = ApplicationData.Current.LocalFolder;
            var settingsFolder = ApplicationData.Current.RoamingSettings;

            try
            {
                var daysToKeepLog = 5;

                if (settingsFolder.Values.TryGetValue("DaysToKeepErrorLogs", out object daysValue))
                {
                    daysToKeepLog = (int) daysValue;
                }
                else
                {
                    settingsFolder.Values["DaysToKeepErrorLogs"] = daysToKeepLog;
                }

                var todaysDate = DateTime.Now.Date;
                
                var files = await localFolder.GetFilesAsync();

                if (files.Count < 1) 
                    return;

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
#if DEBUG
                Debugger.Break();
#endif
            }
        }
    }
}