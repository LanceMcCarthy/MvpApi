using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvpApi.Services.Utilities
{
    public static class ExceptionLogger
    {
        private static bool _isWriting;
        private static readonly string AppDataFolder;

        static ExceptionLogger()
        {
            AppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        }

        public static Task LogExceptionAsync(this Exception exception)
        {
            LogException(exception);

            return Task.CompletedTask;
        }

        public static void LogException(this Exception exception)
        {
            if (_isWriting)
            {
                return;
            }

            var logContent = BuildLogMessage(exception);
            var fileName = "ErrorLog" + "_" + DateTime.Today.ToString("yyyyMMdd") + "." + "log";
            var filePath = Path.Combine(AppDataFolder, fileName);

            StorageHelpers.AppendToLogFile(logContent, filePath);

            PurgeOldLogFiles();

            _isWriting = false;

        }

        private static string BuildLogMessage(Exception currentException)
        {
            // Build log file text
            var messageBuilder = new StringBuilder();

            try
            {
                messageBuilder.AppendLine("-----------------------------------------------------------------");
                messageBuilder.AppendLine("Source: " + currentException.Source.Trim());
                messageBuilder.AppendLine("Date Time: " + DateTime.Now);
                messageBuilder.AppendLine("-----------------------------------------------------------------");
                messageBuilder.AppendLine("Method: " + currentException.Message.Trim());
                messageBuilder.AppendLine("Exception :: " + currentException);

                if (currentException.InnerException != null)
                {
                    messageBuilder.AppendLine("InnerException :: " + currentException.InnerException);
                }

                messageBuilder.AppendLine(" ");
            }
            catch
            {
                messageBuilder.AppendLine("Exception:: Unknown Exception.");
            }

            return messageBuilder.ToString();
        }
        
        public static void PurgeOldLogFiles()
        {
            // Delete any outdated log files
            try
            {
                var daysToKeepLog = 30;
                
                var filePaths = Directory.GetFiles(AppDataFolder, "*.log").ToList();

                if (filePaths.Count < 1)
                    return;

                foreach (var filePath in filePaths)
                {
                    DateTime created = File.GetCreationTime(filePath);
                    if (DateTime.Compare(DateTime.Today.Date, created.AddDays(daysToKeepLog).Date) >= 0) File.Delete(filePath);
                }
            }
            catch (Exception)
            {
//#if DEBUG
//                Debugger.Break();
//#endif
            }
        }
    }
}