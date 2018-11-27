using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Identity.Client;

namespace MvpApi.Services.Utilities
{
    public class OAuthHelper
    {
        // NOTE System.ArgumentException: MSAL always sends the scopes 'openid profile offline_access', do not include in scopes parameters
        string[] scopes = new string[] { "user.read", "user.readbasic.all", "user.readwrite", "email" };

        public OAuthHelper()
        {
            // MSAL test
            // - for any Work or School accounts, or Microsoft personal account, use "common"
            // - for Microsoft Personal account, use "consumers"
            string tenant = "common";
            string authority = $"https://login.microsoftonline.com/{tenant}";

            PublicClientApp = new PublicClientApplication("090fa1d9-3d6f-4f6f-a733-a8b8a3fe16ff", authority);
        }

        public PublicClientApplication PublicClientApp { get; }

        public async Task<AuthenticationResult> LogInAsync()
        {
            AuthenticationResult authResult = null;

            var accounts = await PublicClientApp.GetAccountsAsync();

            try
            {
                authResult = await PublicClientApp.AcquireTokenSilentAsync(scopes, accounts.FirstOrDefault());
            }
            catch (MsalUiRequiredException ex)
            {
                // A MsalUiRequiredException happened on AcquireTokenSilentAsync, this indicates you need to call AcquireTokenAsync to acquire a token
                Debug.WriteLine($"MsalUiRequiredException: {ex.Message}");

                try
                {
                    authResult = await PublicClientApp.AcquireTokenAsync(scopes);
                }
                catch (MsalException msalex)
                {
                    Debug.WriteLine($"Error Acquiring Token:{Environment.NewLine}{msalex}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error Acquiring Token Silently:{Environment.NewLine}{ex}");
                return null;
            }

            if (authResult != null)
            {
                Debug.WriteLine($"{authResult.Account.Username} - Signed In. Expires {authResult.ExpiresOn.ToLocalTime()}{Environment.NewLine}");
                Debug.WriteLine(DisplayBasicTokenInfo(authResult));
                StorageHelpers.Instance.StoreToken("access_token", authResult.AccessToken);
                StorageHelpers.Instance.StoreToken("expires_on", authResult.ExpiresOn.ToString());
                return authResult;
            }
            else
            {
                Debug.WriteLine("user not signed in");
                return null;
            }
        }

        public async Task<string> LogOutAsync()
        {
            var accounts = await PublicClientApp.GetAccountsAsync();

            if (accounts.Any())
            {
                try
                {
                    await PublicClientApp.RemoveAsync(accounts.FirstOrDefault());
                    return "User has signed-out";
                }
                catch (MsalException ex)
                {
                    return $"Error signing-out user: {ex.Message}";
                }
            }

            return "There were no accounts to sign out of";
        }

        public string DisplayBasicTokenInfo(AuthenticationResult authResult)
        {
            var tokenInfoText = "";

            if (authResult != null)
            {
                tokenInfoText += $"Username: {authResult.Account.Username}" + Environment.NewLine;
                tokenInfoText += $"Token Expires: {authResult.ExpiresOn.ToLocalTime()}" + Environment.NewLine;
                tokenInfoText += $"Access Token: {authResult.AccessToken}" + Environment.NewLine;
            }

            return tokenInfoText;
        }
    }
}
