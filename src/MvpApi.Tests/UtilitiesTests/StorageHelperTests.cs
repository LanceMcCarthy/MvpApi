using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MvpApi.Services.Utilities;

namespace MvpApi.Tests.UtilitiesTests
{
    [TestClass]
    public class StorageHelperTests
    {
        [TestMethod]
        public void SaveImage()
        {
            // Arrange
            var originalImageBytes = TestPngImage();

            // Act
            var imageFilePath = StorageHelpers.Instance.SaveImage(originalImageBytes, "icon.png");

            // Assert
            var loadedImageBytes = File.ReadAllBytes(imageFilePath);
            Assert.AreEqual(originalImageBytes.Length, loadedImageBytes.Length, "Image files sizes are not the same");
        }

        [TestMethod]
        public void SaveEncrypedImage()
        {
            // Arrange
            var originalImageBytes = TestPngImage();

            // Act
            StorageHelpers.Instance.SaveEncrypted("icon", originalImageBytes);
            
            // Assert
            var loadedImageBytes = StorageHelpers.Instance.LoadDecrypted("icon");
            
            Assert.AreEqual(originalImageBytes.Length, loadedImageBytes.Length, "Decrypted Image did not match original image");
        }

        [TestMethod]
        public void SettingsStorage()
        {
            SaveSetting();
            LoadSetting();
        }

        [TestMethod]
        public void TokenStorage()
        {
            StoreToken();
            LoadToken();
            DeleteToken();
        }

        #region Lifecycle methods
        
        private void SaveSetting()
        {
            // Arrange
            var key = "answer_to_everything";
            var value = "41";

            // Act
            var success = StorageHelpers.Instance.SaveSetting(key, value);

            // Assert
            Assert.IsTrue(success, "Setting was not saved");
        }
        
        private void LoadSetting()
        {
            // Arrange
            var key = "answer_to_everything";
            var value = "41";

            // Act
            var retrievedValue = StorageHelpers.Instance.LoadSetting(key);

            // Assert
            Assert.AreEqual(value, retrievedValue, "Expected setting value did not match");
        }

        
        private void StoreToken()
        {
            // Arrange
            var key = "access_token";
            var value = TestToken();

            // Act
            var success = StorageHelpers.Instance.StoreToken(key, value);

            // Assert
            Assert.IsTrue(success, "Token was not stored");
        }
        
        private void LoadToken()
        {
            // Arrange
            var key = "access_token";
            var value = TestToken();

            // Act
            var retrievedValue = StorageHelpers.Instance.LoadToken(key);

            // Assert
            Assert.AreEqual(value, retrievedValue, "Expected token value did not match");
        }
        
        private void DeleteToken()
        {
            // Arrange
            var key = "access_token";

            // Act
            var success = StorageHelpers.Instance.DeleteToken(key);

            // Assert
            Assert.IsTrue(success, "Token was not deleted");
        }

        #endregion

        #region Sample Test Data

        private byte[] TestPngImage()
        {
            var base64String = "iVBORw0KGgoAAAANSUhEUgAAADIAAAAyCAIAAACRXR/mAAAABGdBTUEAALGPC/xhBQAAAAlwSFlzAAAOwQAADsEBuJFr7QAAABl0RVh0U29mdHdhcmUAcGFpbnQubmV0IDQuMC4yMfEgaZUAAAKSSURBVFhH7Za/bxJhGMercVBHYxw16WL9FzoLiQuBQQfYGFjYFBI0IdHBxIQfwXhSiJg0R7vpUIEgAwyop0gU3cDUhkTDqMZqoJR7X5+DkzZPD+5Fe+0z9JtPyH3vnufuy/u+d3nn5iROEeyJgD0RsCcC9kTAngjYEwF7ImBPBOyJgL0w11+w3MY0gq8YapkB7MU4LvFP39lAZZtbxvQHrPWNHdvTKAr2Ypx7zLYH2pCcXOKG3P+ghT6bUVGjKNiL4SwwznlImThNV4tawbXn/zqP2IsRa2hPXXwycTDOLzNVZQ8+7mssuCmsaBiMSawPF9btmjZghtxUWK+vbvzA53dz4yW7sDzhj2Ev8dMp/uWnNhgHoK+b7NSSUTLsJX5xRdWbzJTP53275Pf79SOfLxQK6UVmmpeNJhp7iV9aFR2qbDZ7+a9SqVSz2XQ6nSPrdrv1IjNZGCudTjOmdbVaLZfLRSJWOBzudDrVarXdbiuKAsPmcDgOOVYymSyXy/Abj8czmUwikYhGo7Vazev16kVmsiRWIBAoFAqRSCSXy8VisUql0mg0isWix+PRi8xk1SRCsm63C2tLhQ8oY6VSyWazkVjywWCw1+uNMtntdjhDIhYIxgw+Y6NMoMOMVa/XYbEbSpZlvchMorEWVjjaPFnKvIwDaGAvcdi7oc2TpRhvFbEfxjrxEAPb0XEBHKOrUzBtFI21sMq625h773YKbr3GV6cAxePGO2/xVUB0Eg2XfOT9zt3Db0TfCRAUjxvv1g0a/+tNJBrLOh3FmkVHsWaRaKwzj9jv/gEl+7WlwuNQAA3sh9jX2NN1tvbZWuARV54ZZQKwJwL2RMCeCNgTAXsiYE8E7ImAPRGwJwL2FJD4H6uZTRBUmOQtAAAAAElFTkSuQmCC";
            return Convert.FromBase64String(base64String);
        }

        private string TestToken()
        {
            return "jhdhksjskjmanknknkaknknknaijijsjnjbdugjmljabakmcihoijsoihdjnbdkjbslknsmnsjhjshbslnslkmslknsjhvsbjhdhksjskjmanknknkaknknknaijijsjnjbdugjmljabakmcihoijsoihdjnbdkjbslknsmnsjhjshbslnslkmslknsjhvsbjhdhksjskjmanknknkaknknknaijijsjnjbdugjmljabakmcihoijsoihdjnbdkjbslknsmnsjhjshbslnslkmslknsjhvsbjhdhksjskjmanknknkaknknknaijijsjnjbdugjmljabakmcihoijsoihdjnbdkjbslknsmnsjhjshbslnslkmslknsjhvsb=";
        }
        
        #endregion
    }
}
