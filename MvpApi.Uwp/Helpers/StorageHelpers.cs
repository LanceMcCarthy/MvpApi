using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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
        private static readonly byte[] _symmetricKey;
        private static readonly byte[] _initializationVector;

        static StorageHelpers()
        {
            if (!DesignMode.DesignModeEnabled)
            {
                localFolder = ApplicationData.Current.LocalFolder;
                roamingFolder = ApplicationData.Current.RoamingFolder;
                tempFolder = ApplicationData.Current.TemporaryFolder;
                localSettings = ApplicationData.Current.LocalSettings;
                roamingSettings = ApplicationData.Current.RoamingSettings;

                //Note: Generate your own private encryption key instead of the sample one: b78BfJKEs7g
                var keyGenerator = new Rfc2898DeriveBytes("b78BfJKEs7g", Encoding.ASCII.GetBytes("b78BfJKEs7g"));
                _symmetricKey = keyGenerator.GetBytes(32);
                _initializationVector = keyGenerator.GetBytes(16);
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

        #region Encryption methods

        private static string EncryptString(string inputText)
        {
            var textBytes = Encoding.Unicode.GetBytes(inputText);
            var encryptedBytes = EncryptBytes(textBytes);

            Debug.WriteLine($"EncryptString complete: {encryptedBytes.Length} bytes");
            return Convert.ToBase64String(encryptedBytes);
        }

        private static string DecryptString(string encryptedText)
        {
            // NOTE: This string is encrypted first, THEN converted to Base64 (not just obfuscated as Base64)
            var encryptedBytes = Convert.FromBase64String(encryptedText);
            var decryptedBytes = DecryptBytes(encryptedBytes);

            Debug.WriteLine($"DecryptString complete: {decryptedBytes.Length} bytes");
            return Encoding.Unicode.GetString(decryptedBytes, 0, decryptedBytes.Length);
        }

        private static byte[] EncryptBytes(byte[] unencryptedData)
        {
            // I chose Rijndael instead of AES because of it's support for larger block size (AES only support 128)
            using (var cipher = new RijndaelManaged { Key = _symmetricKey, IV = _initializationVector })
            using (var cryptoTransform = cipher.CreateEncryptor())
            using (var memoryStream = new MemoryStream())
            using (var cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write))
            {
                cryptoStream.Write(unencryptedData, 0, unencryptedData.Length);
                cryptoStream.FlushFinalBlock();
                var encryptedBytes = memoryStream.ToArray();
                Debug.WriteLine($"EncryptBytes complete: {encryptedBytes.Length} bytes");
                return encryptedBytes;
            }
        }

        private static byte[] DecryptBytes(byte[] encryptedBytes)
        {
            using (var cipher = new RijndaelManaged())
            using (var cryptoTransform = cipher.CreateDecryptor(_symmetricKey, _initializationVector))
            using (var memoryStream = new MemoryStream(encryptedBytes))
            using (var cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Read))
            {
                byte[] decryptedBytes = new byte[encryptedBytes.Length];
                int bytesRead = cryptoStream.Read(decryptedBytes, 0, decryptedBytes.Length);

                // Note - I'm using Take() to clean up junk bytes at the end of the array
                return decryptedBytes.Take(bytesRead).ToArray();
            }
        }

        #endregion
    }
}
