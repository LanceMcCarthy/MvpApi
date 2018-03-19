using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Windows.Storage;

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
