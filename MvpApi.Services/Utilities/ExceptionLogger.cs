using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvpApi.Services.Utilities
{
    public static class ExceptionLogger
    {
        public static async Task LogExceptionAsync(this Exception exception)
        {
            await Task.Run(() =>
            {
                var logContent = BuildLogMessage(exception);

                WriteLogFile(logContent);

                PurgeOldLogFiles();
            });
        }

        public static void LogException(this Exception exception)
        {
            var logContent = BuildLogMessage(exception);

            WriteLogFile(logContent);

            PurgeOldLogFiles();
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

        private static void WriteLogFile(string logContent)
        {
            // Write to file
            try
            {
                var fileName = "MVPCompanion_ErrorLog" + "_" + DateTime.Today.ToString("yyyyMMdd") + "." + "log";
                var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var filePath = Path.Combine(appDataFolder, fileName);

                if (File.Exists(filePath))
                {
                    File.AppendAllText(filePath, logContent);
                }
                else
                {
                    File.WriteAllText(filePath, logContent);
                }
            }
            catch (Exception e)
            {
#if DEBUG
                Debugger.Break();
#endif
            }
        }

        private static void PurgeOldLogFiles()
        {
            // Delete any outdated log files
            try
            {
                var daysToKeepLog = 5;

                var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var filePaths = Directory.GetFiles(appDataFolder).ToList();

                if (filePaths.Count < 1)
                    return;

                foreach (var filePath in filePaths)
                {
                    var fileExtension = Path.GetExtension(filePath)?.ToLower();

                    if (fileExtension == ".log")
                    {
                        var created = File.GetCreationTime(filePath);

                        if (DateTime.Compare(DateTime.Today.Date, created.AddDays(daysToKeepLog).Date) >= 0)
                        {
                            File.Delete(filePath);
                        }
                    }
                }
            }
            catch (Exception e)
            {
#if DEBUG
                Debugger.Break();
#endif
            }
        }
    }
}