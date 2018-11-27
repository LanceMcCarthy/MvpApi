using Microsoft.VisualStudio.TestTools.UnitTesting;
using MvpApi.Services.Apis;

namespace MvpApi.Tests.ServicesTests
{
    [TestClass]
    public class MvpApiServiceTests
    {
        [TestMethod]
        public void Instantiation()
        {
            // Arrange
            MvpApiService client = null;

            // Act
            client = new MvpApiService("12345");

            // Assert
            Assert.IsNotNull(client);
        }

        [TestMethod]
        public void Initialization()
        {
            // Arrange
            MvpApiService client = null;
            client = new MvpApiService("12345");

            // Act
            client.InitializeAsync().GetAwaiter().GetResult();

            // Assert
            Assert.IsTrue(client.IsInitialized, "Service was not initialized");
        }

        [TestMethod]
        public void GetProfileImage()
        {
            // Todo write test
            Assert.Inconclusive("Test not written");
        }

        [TestMethod]
        public void DownloadAndSaveProfileImage()
        {
            // Todo write test
            Assert.Inconclusive("Test not written");
        }

        [TestMethod]
        public void GetContributions()
        {
            // Todo write test
            Assert.Inconclusive("Test not written");
        }

        [TestMethod]
        public void SubmitContributions()
        {
            // Todo write test
            Assert.Inconclusive("Test not written");
        }

        [TestMethod]
        public void DeleteContributions()
        {
            // Todo write test
            Assert.Inconclusive("Test not written");
        }

        [TestMethod]
        public void GetContributionAreas()
        {
            // Todo write test
            Assert.Inconclusive("Test not written");
        }

        [TestMethod]
        public void GetContributionTypes()
        {
            // Todo write test
            Assert.Inconclusive("Test not written");
        }

        [TestMethod]
        public void GetVisibilities()
        {
            // Todo write test
            Assert.Inconclusive("Test not written");
        }
    }
}
