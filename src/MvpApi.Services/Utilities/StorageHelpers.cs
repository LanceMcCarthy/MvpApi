using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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
                e.LogException();
                Debug.WriteLine(e);
                return null;
            }
        }
        
        public bool StoreToken(string key, string value)
        {
            try
            {
                var filePath = Path.Combine(_appDataFolder, $"{key}.txt");
                var encryptedToken = EncryptString(value);

                File.WriteAllText(filePath, encryptedToken);

                return true;
            }
            catch (Exception e)
            {
                e.LogException();
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
                    var decryptedToken = DecryptString(storedValue);
                    return decryptedToken;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                e.LogException();
                Debug.WriteLine($"LoadToken Exception: {e}");
                return null;
            }
        }

        public bool DeleteToken(string key)
        {
            try
            {
                var filePath = Path.Combine(_appDataFolder, $"{key}.txt");

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                return true;
            }
            catch (Exception e)
            {
                e.LogException();
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

        public bool SaveEncrypted(string key, byte [] unencryptedBytes)
        {
            try
            {
                var encryptedBytes = EncryptBytes(unencryptedBytes);
                
                var filePath = Path.Combine(_appDataFolder, $".{key}.bin");

                File.WriteAllBytes(filePath, encryptedBytes);

                return true;
            }
            catch (Exception e)
            {
                e.LogException();
                return false;
            }
        }

        public byte[] LoadDecrypted(string key)
        {
            try
            {
                var filePath = Path.Combine(_appDataFolder, $".{key}.bin");

                if (File.Exists(filePath))
                {
                    var encryptedBytes = File.ReadAllBytes(filePath);
                    var decryptedBytes = DecryptBytes(encryptedBytes);
                    return decryptedBytes;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                e.LogException();
                return null;
            }
        }

        public static bool AppendToLogFile(string logContent, string filePath)
        {
            try
            {
                logContent = $"--****-- {DateTime.Now:G} --****-- \r\n";

                if (File.Exists(filePath))
                {
                    File.AppendAllText(filePath, logContent);
                }
                else
                {
                    File.WriteAllText(filePath, logContent);
                }

                return true;
            }
            catch(Exception ex)
            {
                Trace.WriteLine($"StorageHelpers.AppendToLogFile Exception: {ex}");
                return false;
            }
        }

        #endregion

        #region Encryption methods
        
        private string EncryptString(string inputText)
        {
            var textBytes = Encoding.Unicode.GetBytes(inputText);
            var encryptedBytes = EncryptBytes(textBytes);

            Debug.WriteLine($"EncryptString complete: {encryptedBytes.Length} bytes");
            return Convert.ToBase64String(encryptedBytes);
        }

        private string DecryptString(string encryptedText)
        {
            // NOTE: This string is encrypted first, THEN converted to Base64 (not just obfuscated as Base64)
            var encryptedBytes = Convert.FromBase64String(encryptedText);
            var decryptedBytes = DecryptBytes(encryptedBytes);

            Debug.WriteLine($"DecryptString complete: {decryptedBytes.Length} bytes");
            return Encoding.Unicode.GetString(decryptedBytes, 0, decryptedBytes.Length);
        }

        private byte[] EncryptBytes(byte[] unencryptedData)
        {
            // I chose Rijndael instead of AES because of it's support for larger block size (AES only support 128)
            using var cipher = new RijndaelManaged { Key = _symmetricKey, IV = _initializationVector };
            using var cryptoTransform = cipher.CreateEncryptor();
            using var memoryStream = new MemoryStream();
            using var cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write);

            cryptoStream.Write(unencryptedData, 0, unencryptedData.Length);
            cryptoStream.FlushFinalBlock();
            var encryptedBytes = memoryStream.ToArray();
            Debug.WriteLine($"EncryptBytes complete: {encryptedBytes.Length} bytes");
            return encryptedBytes;
        }

        private byte[] DecryptBytes(byte[] encryptedBytes)
        {
            using var cipher = new RijndaelManaged();
            using var cryptoTransform = cipher.CreateDecryptor(_symmetricKey, _initializationVector);
            using var memoryStream = new MemoryStream(encryptedBytes);
            using var cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Read);

            byte[] decryptedBytes = new byte[encryptedBytes.Length];
            int bytesRead = cryptoStream.Read(decryptedBytes, 0, decryptedBytes.Length);

            // Note - I'm using Take() to clean up junk bytes at the end of the array
            return decryptedBytes.Take(bytesRead).ToArray();
        }

        // TODO for move to .NET 6
        //public static byte[] Encrypt(byte[] decryptedBytes, string password, int keySize, int blockSize, int iterations)
        //{
        //    if (blockSize != 128)
        //        throw new ArgumentOutOfRangeException(nameof(blockSize), "Block size must be 128 bits.");

        //    ReadOnlySpan<byte> salt = GenerateRandomEntropy(keySize / 8); // Generate salt based
        //    ReadOnlySpan<byte> iv = GenerateRandomEntropy(blockSize / 8); // Generate iv, has to be the same as the block size

        //    byte[] key = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA1, keySize / 8);

        //    using Aes aes = Aes.Create();
        //    aes.Key = key;

        //    int encryptedSize = aes.GetCiphertextLengthCbc(decryptedBytes.Length);
        //    byte[] cipher = new byte[salt.Length + iv.Length + encryptedSize];
        //    Span<byte> cipherSpan = cipher;
        //    salt.CopyTo(cipherSpan);
        //    iv.CopyTo(cipherSpan.Slice(salt.Length));
        //    int encrypted = aes.EncryptCbc(decryptedBytes, iv, cipherSpan.Slice(salt.Length + iv.Length));

        //    Debug.Assert(encryptedSize == encrypted);
        //    return cipher;
        //}

        //public static byte[] Decrypt(byte[] encryptedBytesWithSaltAndIV, string password, int keySize, int blockSize, int iterations)
        //{
        //    if (blockSize != 128)
        //        throw new ArgumentOutOfRangeException(nameof(blockSize), "Block size must be 128 bits.");

        //    ReadOnlySpan<byte> salt = encryptedBytesWithSaltAndIV.AsSpan(0, keySize / 8); // Take salt bytes
        //    ReadOnlySpan<byte> iv = encryptedBytesWithSaltAndIV.AsSpan(keySize / 8, blockSize / 8); // Skip salt bytes, take iv bytes
        //    ReadOnlySpan<byte> cipher = encryptedBytesWithSaltAndIV.AsSpan((keySize / 8) + (blockSize / 8));

        //    byte[] key = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA1, keySize / 8);

        //    using Aes aes = Aes.Create();
        //    aes.Key = key;
        //    return aes.DecryptCbc(cipher, iv);
        //}

        //private static byte[] GenerateRandomEntropy(int size)
        //{
        //    return RandomNumberGenerator.GetBytes(size);
        //}

        #endregion
    }
}
