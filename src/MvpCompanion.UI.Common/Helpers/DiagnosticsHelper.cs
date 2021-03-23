using System;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Store;
using Windows.Devices.Enumeration;
using Windows.Devices.Input;
using Windows.Graphics.Display;
using Windows.Networking.Connectivity;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.Storage;

namespace MvpCompanion.UI.Common.Helpers
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
            builder.AppendLine(e.Message + $"{e.Message}");

            builder.AppendLine($"Time: {DateTime.Now.ToUniversalTime():r}\r\n");
            builder.AppendLine($"App Name: {packageId.Name}\r\n");
            builder.AppendLine($"App Version: {packageId.Version.Major}.{packageId.Version.Minor}.{packageId.Version.Build}.{packageId.Version.Revision}\r\n");
            builder.AppendLine($"App Publisher: {packageId.Publisher}\r\n");
            builder.AppendLine($"Supported Package Architecture: {packageId.Architecture}\r\n");
            builder.AppendLine($"Store App Id: {CurrentApp.AppId}\r\n");
            builder.AppendLine($"Culture: {CultureInfo.CurrentCulture}\r\n");
            builder.AppendLine($"OS: {clientDeviceInformation.OperatingSystem}\r\n");
            builder.AppendLine($"System Manufacturer: {clientDeviceInformation.SystemManufacturer}\r\n");
            builder.AppendLine($"System Product Name: {clientDeviceInformation.SystemProductName}\r\n");
            builder.AppendLine($"Friendly System Name: {clientDeviceInformation.FriendlyName}\r\n");
            builder.AppendLine($"Friendly System ID: {clientDeviceInformation.Id}\r\n");
            builder.AppendLine($"Current Memory Usage: {GC.GetTotalMemory(false) / 1024f / 1024f:f3} MB\r\n");
            builder.AppendLine($"Logical DPI: {displayInformation.LogicalDpi}\r\n");
            builder.AppendLine($"Resolution Scale: {displayInformation.ResolutionScale}\r\n");
            builder.AppendLine($"Current Orientation: {displayInformation.CurrentOrientation}\r\n");
            builder.AppendLine($"Native Orientation: {displayInformation.NativeOrientation}\r\n");
            builder.AppendLine($"Is Stereo Enabled: {displayInformation.StereoEnabled}\r\n");
            builder.AppendLine($"Supports Keyboard: {new KeyboardCapabilities().KeyboardPresent == 1}\r\n");
            builder.AppendLine($"Supports Mouse: {new MouseCapabilities().MousePresent == 1}\r\n");
            builder.AppendLine($"Supports Touch (contacts): {touchCapabilities.TouchPresent == 1} ({touchCapabilities.Contacts})\r\n");
            builder.AppendLine($"Is Network Available: {NetworkInterface.GetIsNetworkAvailable()}\r\n");
            builder.AppendLine($"Is Internet Connection Available: {NetworkInformation.GetInternetConnectionProfile() != null}\r\n");
            
            if (shouldDumpCompleteDeviceInfos)
            {
                builder.AppendLine($"Installed Location: {Package.Current.InstalledLocation.Path}\r\n");
                builder.AppendLine($"App Temp  Folder: {ApplicationData.Current.TemporaryFolder.Path}\r\n");
                builder.AppendLine($"App Local Folder: {ApplicationData.Current.LocalFolder.Path}\r\n");
                builder.AppendLine($"App Roam  Folder: {ApplicationData.Current.RoamingFolder.Path}\r\n\n");  

                builder.AppendLine("Network Host Names:\r\n");

                foreach (var hostName in NetworkInformation.GetHostNames())
                {
                    builder.AppendLine($"{hostName.DisplayName} ({hostName.Type}), \r\n");
                }
                
                var devInfos = await DeviceInformation.FindAllAsync();

                builder.AppendLine("\r\n\nComplete Device Infos:\r\n");

                foreach (var devInfo in devInfos)
                {
                    builder.AppendLine($"Name: {devInfo.Name} Id: {devInfo.Id} - Properties: \r\n");

                    foreach (var pair in devInfo.Properties)
                    {
                        builder.AppendLine($"{pair.Key} = {pair.Value}, \r\n");
                    }
                }
            }
            
            return builder.ToString();
        }
    }
}