using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using MvpApi.Common.Models;
using MvpApi.Services.Utilities;
using Newtonsoft.Json;

namespace MvpApi.Services.Apis
{
    public class MvpApiService : IDisposable
    {
        private static readonly string redirectUrl = "https://login.live.com/oauth20_desktop.srf";
        private readonly HttpClient _client;

        /// <summary>
        /// Service that interacts with the MVP API
        /// </summary>
        /// <param name="apiKey">the Ocp-Apim-Subscription-Key you got from your MVP API portal</param>
        /// <param name="authorizationHeader">Authorization header. Example: "Bearer AccessTokenGoesHere"</param>
        public MvpApiService(string authorizationHeader)
        {
            var handler = new HttpClientHandler();
            if (handler.SupportsAutomaticDecompression)
                handler.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

            var subscriptionKey = ApiKeyManager.Instance.SubscriptionKey;
            
            _client = new HttpClient(handler);
            _client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
            _client.DefaultRequestHeaders.Add("Authorization", authorizationHeader);
        }

        #region API usage

        /// <summary>
        /// Returns the profile data of the currently signed in MVP
        /// </summary>
        /// <returns>The MVP's profile information</returns>
        public async Task<ProfileViewModel> GetProfileAsync(string accessTokenUrl, string refreshToken)
        {
            try
            {
                using (var response = await _client.GetAsync("https://mvpapi.azure-api.net/mvp/api/profile"))
                {
                    if(response.StatusCode == HttpStatusCode.NotFound)
                    {
                        await RequestAuthorizationAsync(accessTokenUrl, StorageHelpers.Instance.LoadToken("refresh_token"), true);
                        return null;
                    }

                    var json = await response.Content.ReadAsStringAsync();

                    return JsonConvert.DeserializeObject<ProfileViewModel>(json);
                }
            }
            catch (HttpRequestException e)
            {
                await e.LogExceptionAsync();

                if (e.Message.Contains("401"))
                {
                    //TODO Try refresh token, access token is only valid for 60 minutes
                }

                if (e.Message.Contains("500"))
                {

                }

                Debug.WriteLine($"GetProfileAsync HttpRequestException: {e}");
                return null;
            }
            catch (Exception e)
            {
                await e.LogExceptionAsync();

                Debug.WriteLine($"GetProfileAsync Exception: {e}");
                return null;
            }
        }

        /// <summary>
        /// Get the profile picture of the currently signed in MVP
        /// </summary>
        /// <returns>JPG image byte array</returns>
        public async Task<byte[]> GetProfileImageAsync()
        {
            try
            {
                // the result is Detected mime type: image/jpeg; charset=binary
                using (var response = await _client.GetAsync("https://mvpapi.azure-api.net/mvp/api/profile/photo"))
                {
                    var base64String = await response.Content.ReadAsStringAsync();

                    try
                    {
                        if (string.IsNullOrEmpty(base64String))
                        {
                            return null;
                        }

                        base64String = base64String.TrimStart('"').TrimEnd('"');

                        var imgBytes = Convert.FromBase64String(base64String);
                        
                        Debug.WriteLine($"Image Decoded: {imgBytes?.Length} bytes");

                        return imgBytes;
                    }
                    catch(Exception ex)
                    {
                        return null;
                    }
                }
            }
            catch (HttpRequestException e)
            {
                await e.LogExceptionAsync();

                if (e.Message.Contains("401"))
                {
                    //TODO Try refresh token, access token is only valid for 60 minutes
                }

                if (e.Message.Contains("500"))
                {

                }

                Debug.WriteLine($"GetProfileImageAsync HttpRequestException: {e}");
                return null;
            }
            catch (Exception e)
            {
                await e.LogExceptionAsync();
                Debug.WriteLine($"GetProfileImageAsync Exception: {e}");
                return null;
            }
        }

        /// <summary>
        /// Downloads and saves the image file to LocalFolder
        /// </summary>
        /// <returns>File path</returns>
        public async Task<string> DownloadAndSaveProfileImage()
        {
            try
            {
                // the result is Detected mime type: image/jpeg; charset=binary
                using (var response = await _client.GetAsync("https://mvpapi.azure-api.net/mvp/api/profile/photo"))
                {
                    var base64String = await response.Content.ReadAsStringAsync();

                    try
                    {
                        if (string.IsNullOrEmpty(base64String))
                        {
                            return null;
                        }

                        base64String = base64String.TrimStart('"').TrimEnd('"');

                        // determine file type
                        var data = base64String.Substring(0, 5);

                        var fileExtension = string.Empty;

                        switch (data.ToUpper())
                        {
                            case "IVBOR":
                                fileExtension = "png";
                                break;
                            case "/9J/4":
                                fileExtension = "jpg";
                                break;
                        }
                        
                        var imgBytes = Convert.FromBase64String(base64String);

                        var filePath = StorageHelpers.Instance.SaveImage(imgBytes, $"ProfilePicture.{fileExtension}");
                        return filePath;
                    }
                    catch (Exception e)
                    {
                        await e.LogExceptionAsync();
                        Debug.WriteLine($"DownloadAndSaveProfileImage Exception: {e}");
                        return null;
                    }
                }
            }
            catch (HttpRequestException e)
            {
                await e.LogExceptionAsync();

                if (e.Message.Contains("401"))
                {
                    //TODO Try refresh token, access token is only valid for 60 minutes
                }

                if (e.Message.Contains("500"))
                {

                }

                Debug.WriteLine($"GetProfileImageAsync HttpRequestException: {e}");
                return null;
            }
            catch (Exception e)
            {
                await e.LogExceptionAsync();
                Debug.WriteLine($"GetProfileImageAsync Exception: {e}");
                return null;
            }
        }

        /// <summary>
        /// Gets the MVPs activities, depending on the offset (page) and the limit (number of items per-page)
        /// </summary>
        /// <param name="offset">page to return</param>
        /// <param name="limit">number of items for the page</param>
        /// <returns></returns>
        public async Task<ContributionViewModel> GetContributionsAsync(int? offset, int limit)
        {
            if (offset == null)
                offset = 0;

            try
            {
                using (var response = await _client.GetAsync($"https://mvpapi.azure-api.net/mvp/api/contributions/{offset}/{limit}"))
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<ContributionViewModel>(json);
                }
            }
            catch (HttpRequestException e)
            {
                await e.LogExceptionAsync();

                if (e.Message.Contains("401"))
                {
                    //TODO Try refresh token, access token is only valid for 60 minutes
                }

                if (e.Message.Contains("500"))
                {

                }

                Debug.WriteLine($"GetContributionsAsync HttpRequestException: {e}");
                return null;
            }
            catch (Exception e)
            {
                await e.LogExceptionAsync();

                Debug.WriteLine($"GetContributionsAsync Exception: {e}");
                return null;
            }
        }

        /// <summary>
        /// Submits a new contribution to the currently sign-in MVP profile
        /// </summary>
        /// <param name="contribution">The contribution to be submitted</param>
        /// <returns>Contribution submitted. This object should now have a valid ID and be added to the app's Contributions collection</returns>
        public async Task<ContributionsModel> SubmitContributionAsync(ContributionsModel contribution)
        {
            if (contribution == null)
                throw new NullReferenceException("The contribution parameter was null.");

            try
            {
                var serializedContribution = JsonConvert.SerializeObject(contribution);
                byte[] byteData = Encoding.UTF8.GetBytes(serializedContribution);
                
                using (var content = new ByteArrayContent(byteData))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    
                    using (var response = await _client.PostAsync("https://mvpapi.azure-api.net/mvp/api/contributions?", content))
                    {
                        var json = await response.Content.ReadAsStringAsync();

                        Debug.WriteLine($"Submission Save JSON: {json}");

                        var result = JsonConvert.DeserializeObject<ContributionsModel>(json);

                        Debug.WriteLine($"Submission Save Result: ID {result.ContributionId}");

                        return result;
                    }
                }
            }
            catch (HttpRequestException e)
            {
                await e.LogExceptionAsync();

                if (e.Message.Contains("401"))
                {
                    //TODO Try refresh token, access token is only valid for 60 minutes
                }

                if (e.Message.Contains("500"))
                {
                    
                }

                Debug.WriteLine($"SubmitContributionAsync HttpRequestException: {e}");
                return null;
            }
            catch (Exception e)
            {
                await e.LogExceptionAsync();

                Debug.WriteLine($"SubmitContributionAsync Exception: {e}");

                return null;
            }
        }

        /// <summary>
        /// Updates an existing contribution, identified by the contribution ID
        /// </summary>
        /// <param name="contribution">Contribution to be updated</param>
        /// <returns>Bool to denote update success or failure</returns>
        public async Task<bool?> UpdateContributionAsync(ContributionsModel contribution)
        {
            if(contribution == null)
                throw new NullReferenceException("The contribution parameter was null.");

            try
            {
                // Request body
                var serializedContribution = JsonConvert.SerializeObject(contribution);
                byte[] byteData = Encoding.UTF8.GetBytes(serializedContribution);

                using (var content = new ByteArrayContent(byteData))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    using (var response = await _client.PutAsync("https://mvpapi.azure-api.net/mvp/api/contributions", content))
                    {
                        return response.IsSuccessStatusCode;
                    }
                }
            }
            catch (HttpRequestException e)
            {
                await e.LogExceptionAsync();

                if (e.Message.Contains("401"))
                {
                    //TODO Try refresh token, access token is only valid for 60 minutes
                }

                if (e.Message.Contains("500"))
                {
                    
                }

                Debug.WriteLine($"UpdateContributionAsync HttpRequestException: {e}");
                return null;
            }
            catch (Exception e)
            {
                await e.LogExceptionAsync();

                Debug.WriteLine($"GetProfileAsync Exception: {e}");
                return null;
            }
        }

        /// <summary>
        /// Delete contribution
        /// </summary>
        /// <param name="contribution">Item to delete</param>
        /// <returns>Success or failure</returns>
        public async Task<bool?> DeleteContributionAsync(ContributionsModel contribution)
        {
            if (contribution == null)
                throw new NullReferenceException("The contribution parameter was null.");

            try
            {
                using (var response = await _client.DeleteAsync($"https://mvpapi.azure-api.net/mvp/api/contributions?id={contribution.ContributionId}"))
                {
                    return response.IsSuccessStatusCode;
                }
            }
            catch (HttpRequestException e)
            {
                await e.LogExceptionAsync();

                if (e.Message.Contains("401"))
                {
                    //TODO Try refresh token, access token is only valid for 60 minutes
                }

                if (e.Message.Contains("500"))
                {

                }

                Debug.WriteLine($"UpdateContributionAsync HttpRequestException: {e}");
                return null;
            }
            catch (Exception e)
            {
                await e.LogExceptionAsync();

                Debug.WriteLine($"GetProfileAsync Exception: {e}");
                return null;
            }
        }

        /// <summary>
        /// This gets a list if the different contributions types
        /// </summary>
        /// <returns>List of contributions types</returns>
        public async Task<IReadOnlyList<ContributionTypeModel>> GetContributionTypesAsync()
        {
            try
            {
                using (var response = await _client.GetAsync("https://mvpapi.azure-api.net/mvp/api/contributions/contributiontypes"))
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<IReadOnlyList<ContributionTypeModel>>(json);
                }
            }
            catch (HttpRequestException e)
            {
                await e.LogExceptionAsync();

                if (e.Message.Contains("401"))
                {
                    //TODO Try refresh token, access token is only valid for 60 minutes
                }

                if (e.Message.Contains("500"))
                {

                }

                Debug.WriteLine($"GetContributionTypesAsync HttpRequestException: {e}");
                return null;
            }
            catch (Exception e)
            {
                await e.LogExceptionAsync();

                Debug.WriteLine($"GetContributionTypesAsync Exception: {e}");
                return null;
            }
        }

        /// <summary>
        /// Gets a list of the Contibution Technologies (aka Contribution Areas)
        /// </summary>
        /// <returns>A list of available contribution areas</returns>
        public async Task<IReadOnlyList<ContributionAreasRootItem>> GetContributionAreasAsync()
        {
            try
            {
                using (var response = await _client.GetAsync("https://mvpapi.azure-api.net/mvp/api/contributions/contributionareas"))
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<IReadOnlyList<ContributionAreasRootItem>>(json);
                }
            }
            catch (HttpRequestException e)
            {
                await e.LogExceptionAsync();

                if (e.Message.Contains("401"))
                {
                    //TODO Try refresh token, access token is only valid for 60 minutes
                }

                if (e.Message.Contains("500"))
                {

                }

                Debug.WriteLine($"GetContributionTechnologiesAsync HttpRequestException: {e}");
                return null;
            }
            catch (Exception e)
            {
                await e.LogExceptionAsync();

                Debug.WriteLine($"GetContributionTechnologiesAsync Exception: {e}");
                return null;
            }
        }

        /// <summary>
        /// Gets a list of contribution visibility options (aka Sharing Preferences). The traditional results are "Microsoft Only", "MVP Community" and "Everyone"
        /// </summary>
        /// <returns>A list of available visibilities</returns>
        public async Task<IReadOnlyList<VisibilityViewModel>> GetVisibilitiesAsync()
        {
            try
            {
                using (var response = await _client.GetAsync("https://mvpapi.azure-api.net/mvp/api/contributions/sharingpreferences"))
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<IReadOnlyList<VisibilityViewModel>>(json);
                }
            }
            catch (HttpRequestException e)
            {
                await e.LogExceptionAsync();

                if (e.Message.Contains("401"))
                {
                    //TODO Try refresh token, access token is only valid for 60 minutes
                }

                if (e.Message.Contains("500"))
                {

                }

                Debug.WriteLine($"GetVisibilitiesAsync HttpRequestException: {e}");
                return null;
            }
            catch (Exception e)
            {
                await e.LogExceptionAsync();

                Debug.WriteLine($"GetVisibilitiesAsync Exception: {e}");
                return null;
            }
        }
        
        #endregion

        #region Authorization
        
        public static async Task<string> RequestAuthorizationAsync(string requestUrl, string authCode, bool refresh = false)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var content = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("client_id", ApiKeyManager.Instance.ClientId),
                        new KeyValuePair<string, string>("grant_type", refresh ? "refresh_token" : "authorization_code"),
                        new KeyValuePair<string, string>(refresh ? "refresh_token" : "code", authCode.Split('&')[0]),
                        new KeyValuePair<string, string>("redirect_uri", redirectUrl)
                    };

                    var postContent = new FormUrlEncodedContent(content);

                    var responseTxt = "";
                    var authHeader = "";

                    using (var response = await client.PostAsync(new Uri(requestUrl), postContent))
                    {
                        responseTxt = await response.Content.ReadAsStringAsync();
                    }

                    var tokenData = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseTxt);

                    if (tokenData.ContainsKey("access_token"))
                    {
                        // Store the expiration time of the token, currently 3600 seconds (an hour)
                        StorageHelpers.Instance.SaveSetting("expires_in", tokenData["expires_in"]);

                        StorageHelpers.Instance.StoreToken("access_token", tokenData["access_token"]);
                        StorageHelpers.Instance.StoreToken("refresh_token", tokenData["refresh_token"]);

                        // We need to prefix the access token with the token type for the auth header. 
                        // Currently this is always "bearer", doing this to be more future proof
                        var tokenType = tokenData["token_type"];
                        var cleanedAccessToken = tokenData["access_token"].Split('&')[0];

                        authHeader = $"{tokenType} {cleanedAccessToken}";
                    }

                    return authHeader;
                }
            }
            catch (HttpRequestException e)
            {
                if (e.Message.Contains("401"))
                {
                    //TODO Try refresh token, access token is only valid for 60 minutes
                }

                Debug.WriteLine($"RequestAuthorizationAsync HttpRequestException: {e}");
                return null;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"RequestAuthorizationAsync Exception: {e}");
                return null;
            }
        }
        
        #endregion

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}