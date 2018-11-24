using Microsoft.VisualStudio.TestTools.UnitTesting;
using MvpApi.Services.Apis;

namespace MvpApi.Tests.ServicesTests
{
    [TestClass]
    public class ApiKeyManagerTests
    {
        [TestMethod]
        public void GetSubscriptionKey()
        {
            // Arrange
            var testKey = "3d199a7fb1c443e1985375f0572f58f8";

            // Act
            var subscriptionKey = ApiKeyManager.Instance.SubscriptionKey;
            
            // Assert
            Assert.AreEqual(testKey, subscriptionKey, "SubscriptionKey does not match expected value");
        }

        [TestMethod]
        public void GetClientId()
        {
            // Arrange
            var testId = "090fa1d9-3d6f-4f6f-a733-a8b8a3fe16ff";

            // Act
            var clientId = ApiKeyManager.Instance.ClientId;


            // Assert
            Assert.AreEqual(testId, clientId, "SubscriptionKey does not match expected value");
        }
    }
}
