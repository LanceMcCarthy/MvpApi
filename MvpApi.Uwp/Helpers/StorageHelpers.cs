using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Security.Credentials;
using Windows.Storage;

namespace MvpApi.Uwp.Helpers
{
    public static class StorageHelpers
    {
        private static readonly StorageFolder localFolder;
        private static readonly StorageFolder roamingFolder;
        private static readonly StorageFolder tempFolder;
        private static readonly ApplicationDataContainer localSettings;
        private static readonly ApplicationDataContainer roamingSettings;

        static StorageHelpers()
        {
            if (!DesignMode.DesignModeEnabled)
            {
                localFolder = ApplicationData.Current.LocalFolder;
                roamingFolder = ApplicationData.Current.RoamingFolder;
                tempFolder = ApplicationData.Current.TemporaryFolder;
                localSettings = ApplicationData.Current.LocalSettings;
                roamingSettings = ApplicationData.Current.RoamingSettings;
            }
        }

        #region StorageFile helpers

        public static async Task<StorageFile> SaveImageFileAsync(byte[] imageBytes, string fileName, CreationCollisionOption collisionOption = CreationCollisionOption.ReplaceExisting)
        {
            try
            {
                var imageFile = await localFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

                using (var ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
                using (var fileStream = await imageFile.OpenStreamForWriteAsync())
                {
                    ms.CopyTo(fileStream);
                }

                return imageFile;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }

        #endregion

        #region Settings

        public static void SaveLocalSetting(string key, object value)
        {
            try
            {
                if(localSettings != null)
                    localSettings.Values[key] = value;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        public static T LoadLocalSetting<T>(string key)
        {
            try
            {
                object obj;
                var settingValue = default(T);

                if (localSettings != null && localSettings.Values.TryGetValue(key, out obj))
                {
                    settingValue = (T)obj;
                }

                return settingValue;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return default(T);
            }
        }

        public static void SaveRoamingSetting(string key, object value)
        {
            try
            {
                if (localSettings != null)
                    localSettings.Values[key] = value;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        public static T LoadRoamingSetting<T>(string key)
        {
            try
            {
                object obj;
                var settingValue = default(T);

                if (roamingSettings != null && roamingSettings.Values.TryGetValue(key, out obj))
                {
                    settingValue = (T)obj;
                }

                return settingValue;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return default(T);
            }
        }

        #endregion

        #region Secure Storage

        public static void StoreToken(string key, string value)
        {
            try
            {
                var vault = new PasswordVault();
                vault.Add(new PasswordCredential("MvpApiApp", key, value));
            }
            catch (Exception e)
            {
                Debug.WriteLine($"StoreToken Exception: {e}");
            }
        }

        public static string LoadToken(string key)
        {
            try
            {
                var vault = new PasswordVault();
                var credential = vault.Retrieve("MvpApiApp", key);
                return credential.Password;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"LoadToken Exception: {e}");
                return null;
            }
        }

        public static void DeleteToken(string key)
        {
            try
            {
                var vault = new PasswordVault();
                var credential = vault.Retrieve("MvpApiApp", key);
                vault.Remove(credential);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"DeleteToken Exception: {e}");
            }
        }

        #endregion
    }
}
