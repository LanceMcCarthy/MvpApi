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
        public void RequestBearerToken()
        {
            // Arrange
            var clientId = "090fa1d9-3d6f-4f6f-a733-a8b8a3fe16ff";
            var clientSecret = "JuUCyRquexio3oVjah4jWD7";

            var accessToken = "";
            var scope = "wl.emails%20wl.basic%20wl.offline_access%20wl.signin";
            var responseType = "code";
            var grantType = "refresh_token";
            var tokenUrl = "https://login.live.com/oauth20_token.srf";
            var loginUrl = $"https://login.live.com/oauth20_authorize.srf?client_id=" + clientId +"&redirect_uri=" + tokenUrl + "&scope=" + scope + "&response_type=" + responseType;
            var refreshUrl = $"https://login.live.com/oauth20_token.srf?client_id= " + clientId +"&client_secret=" +clientSecret +"&redirect_uri="+ tokenUrl + "&grant_type="+ grantType+"&refresh_token=";

            // Act
            var task = OAuthHelper.RequestAuthorizationAsync(
                clientId,
                accessToken,
                loginUrl,
                tokenUrl);

            var token = task.Result;
            
            // Assert
            Assert.IsNotNull(token, "Bearer token was null");
        }
    }
}
