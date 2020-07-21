using System;
using System.Globalization;
using System.IO.Packaging;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MvpApi.Wpf.Helpers
{
    public static class DiagnosticsHelper
    {
        public static async Task<string> DumpAsync(Exception e, bool shouldDumpCompleteDeviceInfos = false)
        {
            var builder = new StringBuilder();
            var packageId = Package.Current.Id;
            var clientDeviceInformation = new EasClientDeviceInformation();
            var displayInformation = DisplayInformation.GetForCurrentView();
            var touchCapabilities = new TouchCapabilities();
            
            builder.AppendLine("***** Diagnostic Information *****\r\n\n");
#if DEBUG
            builder.AppendLine("DEBUG\r\n");
#endif
            builder.AppendLine("MVP Companion Exception\r\n");
            builder.AppendFormat(e.Message + $"{e.Message}");

            builder.AppendFormat($"Time: {DateTime.Now.ToUniversalTime():r}\r\n");
            builder.AppendFormat($"App Name: {packageId.Name}\r\n");
            builder.AppendFormat($"App Version: {packageId.Version.Major}.{packageId.Version.Minor}.{packageId.Version.Build}.{packageId.Version.Revision}\r\n");
            builder.AppendFormat($"App Publisher: {packageId.Publisher}\r\n");
            builder.AppendFormat($"Supported Package Architecture: {packageId.Architecture}\r\n");
            builder.AppendFormat($"Store App Id: {CurrentApp.AppId}\r\n");
            builder.AppendFormat($"Culture: {CultureInfo.CurrentCulture}\r\n");
            builder.AppendFormat($"OS: {clientDeviceInformation.OperatingSystem}\r\n");
            builder.AppendFormat($"System Manufacturer: {clientDeviceInformation.SystemManufacturer}\r\n");
            builder.AppendFormat($"System Product Name: {clientDeviceInformation.SystemProductName}\r\n");
            builder.AppendFormat($"Friendly System Name: {clientDeviceInformation.FriendlyName}\r\n");
            builder.AppendFormat($"Friendly System ID: {clientDeviceInformation.Id}\r\n");
            builder.AppendFormat($"Current Memory Usage: {GC.GetTotalMemory(false) / 1024f / 1024f:f3} MB\r\n");
            builder.AppendFormat($"Window Bounds: {Window.Current.Bounds.Width} x {Window.Current.Bounds.Height}\r\n");
            builder.AppendFormat($"Logical DPI: {displayInformation.LogicalDpi}\r\n");
            builder.AppendFormat($"Resolution Scale: {displayInformation.ResolutionScale}\r\n");
            builder.AppendFormat($"Current Orientation: {displayInformation.CurrentOrientation}\r\n");
            builder.AppendFormat($"Native Orientation: {displayInformation.NativeOrientation}\r\n");
            builder.AppendFormat($"Is Stereo Enabled: {displayInformation.StereoEnabled}\r\n");
            builder.AppendFormat($"Supports Keyboard: {new KeyboardCapabilities().KeyboardPresent == 1}\r\n");
            builder.AppendFormat($"Supports Mouse: {new MouseCapabilities().MousePresent == 1}\r\n");
            builder.AppendFormat($"Supports Touch (contacts): {touchCapabilities.TouchPresent == 1} ({touchCapabilities.Contacts})\r\n");
            builder.AppendFormat($"Is Network Available: {NetworkInterface.GetIsNetworkAvailable()}\r\n");
            builder.AppendFormat($"Is Internet Connection Available: {NetworkInformation.GetInternetConnectionProfile() != null}\r\n");
            
            if (shouldDumpCompleteDeviceInfos)
            {
                builder.AppendFormat($"Installed Location: {Package.Current.InstalledLocation.Path}\r\n");
                builder.AppendFormat($"App Temp  Folder: {ApplicationData.Current.TemporaryFolder.Path}\r\n");
                builder.AppendFormat($"App Local Folder: {ApplicationData.Current.LocalFolder.Path}\r\n");
                builder.AppendFormat($"App Roam  Folder: {ApplicationData.Current.RoamingFolder.Path}\r\n\n");  

                builder.AppendFormat("Network Host Names:\r\n");

                foreach (var hostName in NetworkInformation.GetHostNames())
                {
                    builder.AppendFormat($"{hostName.DisplayName} ({hostName.Type}), \r\n");
                }
                
                var devInfos = await DeviceInformation.FindAllAsync();

                builder.AppendLine("\r\n\nComplete Device Infos:\r\n");

                foreach (var devInfo in devInfos)
                {
                    builder.AppendFormat($"Name: {devInfo.Name} Id: {devInfo.Id} - Properties: \r\n");

                    foreach (var pair in devInfo.Properties)
                    {
                        builder.AppendFormat($"{pair.Key} = {pair.Value}, \r\n");
                    }
                }
            }
            
            return builder.ToString();
        }
    }
}