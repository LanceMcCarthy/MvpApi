using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Identity.Client;
using Newtonsoft.Json;

namespace MvpApi.Services.Utilities
{
    public class StorageHelpers
    {
        #region Singleton members

        private static StorageHelpers _instance;
        public static StorageHelpers Instance => _instance ?? (_instance = new StorageHelpers());

        #endregion

        #region Instance members

        private readonly string _appDataFolder;
        private readonly byte[] _symmetricKey;
        private readonly byte[] _initializationVector;

        public StorageHelpers()
        {
            _appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            //Note: Generate your own private encryption key instead of the sample one: b78BfJKEs7g
            var keyGenerator = new Rfc2898DeriveBytes("b78BfJKEs7g", Encoding.ASCII.GetBytes("b78BfJKEs7g"));
            _symmetricKey = keyGenerator.GetBytes(32);
            _initializationVector = keyGenerator.GetBytes(16);
        }
        
        public string SaveImage(byte[] imageBytes, string fileName)
        {
            try
            {
                var filePath = Path.Combine(_appDataFolder, fileName);
                File.WriteAllBytes(filePath, imageBytes);
                return filePath;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }
        
        public bool StoreToken(string key, string value)
        {
            try
            {
                var filePath = Path.Combine(_appDataFolder, $"{key}.txt");
                
                var encryptedToken = Encrypt(value);

                File.WriteAllText(filePath, encryptedToken);
                
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"StoreToken Exception: {e}");
                return false;
            }
        }

        public string LoadToken(string key)
        {
            try
            {
                var filePath = Path.Combine(_appDataFolder, $"{key}.txt");

                if (File.Exists(filePath))
                {
                    var storedValue = File.ReadAllText(filePath);

                    var decryptedToken = Decrypt(storedValue);

                    return decryptedToken;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"LoadToken Exception: {e}");
                return null;
            }
        }

        public bool DeleteToken(string key)
        {
            try
            {
                var filePath = Path.Combine(_appDataFolder, $"{key}.txt");
                File.Delete(filePath);
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"DeleteToken Exception: {e}");
                return false;
            }
        }

        public bool SaveSetting(string key, string value)
        {
            try
            {
                var filePath = Path.Combine(_appDataFolder, "settings.json");

                Dictionary<string, string> settings = null;

                if(File.Exists(filePath))
                {
                    var json = "";
                    json = File.ReadAllText(filePath);
                    settings = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                }
                else
                {
                    settings = new Dictionary<string, string>();
                }
                
                settings[key] = value;

                var updatedJson = JsonConvert.SerializeObject(settings);

                File.WriteAllText(filePath, updatedJson);
                return true;
            }
            catch (Exception e)
            {
                e.LogException();
                return false;
            }
        }

        public string LoadSetting(string key)
        {
            try
            {
                var filePath = Path.Combine(_appDataFolder, "settings.json");
                
                if (File.Exists(filePath))
                {
                    var json = File.ReadAllText(filePath);
                    var settings = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                    return settings.ContainsKey(key) ? settings[key] : null;
                }
                else
                {
                    var settings = new Dictionary<string, string>();
                    var json = JsonConvert.SerializeObject(settings);
                    File.WriteAllText(filePath, json);
                    return null;
                }
            }
            catch (Exception e)
            {
                e.LogException();
                return null;
            }
        }

        #endregion

        #region Encryption methods

        public string Encrypt(string inputText)
        {
            var textBytes = Encoding.Unicode.GetBytes(inputText);

            using(var cipher = new RijndaelManaged { Key = _symmetricKey, IV = _initializationVector })
            using (var cryptoTransform = cipher.CreateEncryptor())
            using (var memoryStream = new MemoryStream())
            using (var cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write))
            {
                cryptoStream.Write(textBytes, 0, textBytes.Length);
                cryptoStream.FlushFinalBlock();

                // Convert encrypted string to base64 for easier storage
                return Convert.ToBase64String(memoryStream.ToArray());
            }
        }

        public string Decrypt(string encryptedText)
        {
            // The text is encrypted first, then converted to base64
            var encryptedBytes = Convert.FromBase64String(encryptedText);

            using (var cipher = new RijndaelManaged())
            using (var cryptoTransform = cipher.CreateDecryptor(_symmetricKey, _initializationVector))
            using (var memoryStream = new MemoryStream(encryptedBytes))
            using (var cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Read))
            {
                byte[] decryptedBytes = new byte[encryptedBytes.Length];
                int decryptedCount = cryptoStream.Read(decryptedBytes, 0, decryptedBytes.Length);
                return Encoding.Unicode.GetString(decryptedBytes, 0, decryptedCount);
            }
        }

        public void StoreEncrypted(byte[] unencryptedData, string fileName = ".msalcache.bin")
        {
            using (var cipher = new RijndaelManaged { Key = _symmetricKey, IV = _initializationVector })
            using (var cryptoTransform = cipher.CreateEncryptor())
            using (var memoryStream = new MemoryStream())
            using (var cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write))
            {
                cryptoStream.Write(unencryptedData, 0, unencryptedData.Length);
                cryptoStream.FlushFinalBlock();
                var encryptedBytes = memoryStream.ToArray();
                
                var filePath = Path.Combine(_appDataFolder, fileName);
                File.WriteAllBytes(filePath, encryptedBytes);
            }
        }

        public byte[] LoadDecrypted(string fileName = ".msalcache.bin")
        {
            var filePath = Path.Combine(_appDataFolder, fileName);
            var encryptedBytes = File.ReadAllBytes(filePath);

            using (var cipher = new RijndaelManaged())
            using (var cryptoTransform = cipher.CreateDecryptor(_symmetricKey, _initializationVector))
            using (var memoryStream = new MemoryStream(encryptedBytes))
            using (var cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Read))
            {
                byte[] decryptedBytes = new byte[encryptedBytes.Length];
                cryptoStream.Read(decryptedBytes, 0, decryptedBytes.Length);
                return decryptedBytes;
            }
        }

        #endregion
    }
}
