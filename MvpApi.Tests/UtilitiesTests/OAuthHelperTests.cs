using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MvpApi.Services.Utilities;

namespace MvpApi.Tests.UtilitiesTests
{
    [TestClass]
    public class OAuthHelperTests
    {
        [TestMethod]
        public void EnsurePublicClientApp()
        {
            // Arrange
            OAuthHelper helper = null;

            // Act
            helper = new OAuthHelper();
            
            // Assert
            Assert.IsNotNull(helper.PublicClientApp, "PublicClientApp was null");
        }
    }
}
