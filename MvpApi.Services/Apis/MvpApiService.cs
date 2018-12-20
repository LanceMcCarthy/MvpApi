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
        private readonly HttpClient _client;

        /// <summary>
        /// Service that interacts with the MVP API
        /// </summary>
        /// <param name="authorizationHeaderContent">OAuth 2.0 AccessToken from Live SDK or MS Graph via Azure AD v2 endpoint
        /// IMPORTANT: 'Bearer' prefix needed before the token code</param>
        public MvpApiService(string authorizationHeaderContent)
        {
            var handler = new HttpClientHandler();
            if (handler.SupportsAutomaticDecompression)
                handler.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

            _client = new HttpClient(handler);
            _client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "3d199a7fb1c443e1985375f0572f58f8");
            _client.DefaultRequestHeaders.Add("Authorization", authorizationHeaderContent);
        }

        /// <summary>
        /// This event will fire when there is a 401 or 403 returned from an API call. This indicates that a new Access Token is needed.
        /// Use this event to use the refresh token to get a new access token automatically.
        /// </summary>
        public event EventHandler<EventArgs> AccessTokenExpired;

        /// <summary>
        /// This event fires when the API call results in a HttpStatusCode 500 result is obtained.
        /// </summary>
        public event EventHandler<EventArgs> RequestErrorOccurred;

        #region API Endpoints
    
        /// <summary>
        /// Returns the profile data of the currently signed in MVP
        /// </summary>
        /// <returns>The MVP's profile information</returns>
        public async Task<ProfileViewModel> GetProfileAsync()
        {
            try
            {
                using (var response = await _client.GetAsync("https://mvpapi.azure-api.net/mvp/api/profile"))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<ProfileViewModel>(json);
                    }
                    else
                    {
                        if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
                        {
                            AccessTokenExpired?.Invoke(this, new EventArgs());
                        }
                    }
                }
            }
            catch (HttpRequestException e)
            {
                await e.LogExceptionAsync();
                
                if (e.Message.Contains("500"))
                {
                    RequestErrorOccurred?.Invoke(this, new EventArgs());
                }

                Debug.WriteLine($"GetProfileAsync HttpRequestException: {e}");
            }
            catch (Exception e)
            {
                await e.LogExceptionAsync();

                Debug.WriteLine($"GetProfileAsync Exception: {e}");
            }

            return null;
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
                    if (response.IsSuccessStatusCode)
                    {
                        var base64String = await response.Content.ReadAsStringAsync();

                        try
                        {
                            if (string.IsNullOrEmpty(base64String))
                            {
                                return null;
                            }

                            base64String = base64String.TrimStart('"').TrimEnd('"');
                            
                            return Convert.FromBase64String(base64String);
                        }
                        catch (Exception e)
                        {
                            await e.LogExceptionAsync();
                            Debug.WriteLine($"GetProfileImageAsync Image Conversion Exception: {e}");
                        }
                    }
                    else
                    {
                        if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
                        {
                            AccessTokenExpired?.Invoke(this, new EventArgs());
                        }
                    }
                }
            }
            catch (HttpRequestException e)
            {
                await e.LogExceptionAsync();
                
                if (e.Message.Contains("500"))
                {
                    RequestErrorOccurred?.Invoke(this, new EventArgs());
                }

                Debug.WriteLine($"GetProfileImageAsync HttpRequestException: {e}");
            }
            catch (Exception e)
            {
                await e.LogExceptionAsync();
                Debug.WriteLine($"GetProfileImageAsync Exception: {e}");
            }

            return null;
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
                    if (response.IsSuccessStatusCode)
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
                        }
                    }
                    else
                    {
                        if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
                        {
                            AccessTokenExpired?.Invoke(this, new EventArgs());
                        }
                    }
                }
            }
            catch (HttpRequestException e)
            {
                await e.LogExceptionAsync();
                
                if (e.Message.Contains("500"))
                {
                    RequestErrorOccurred?.Invoke(this, new EventArgs());
                }

                Debug.WriteLine($"GetProfileImageAsync HttpRequestException: {e}");
            }
            catch (Exception e)
            {
                await e.LogExceptionAsync();
                Debug.WriteLine($"GetProfileImageAsync Exception: {e}");
            }

            return null;
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
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<ContributionViewModel>(json);
                    }
                    else
                    {
                        if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
                        {
                            AccessTokenExpired?.Invoke(this, new EventArgs());
                        }
                    }
                }
            }
            catch (HttpRequestException e)
            {
                await e.LogExceptionAsync();

                if (e.Message.Contains("500"))
                {
                    RequestErrorOccurred?.Invoke(this, new EventArgs());
                }

                Debug.WriteLine($"GetContributionsAsync HttpRequestException: {e}");
            }
            catch (Exception e)
            {
                await e.LogExceptionAsync();

                Debug.WriteLine($"GetContributionsAsync Exception: {e}");
            }

            return null;
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
                        if (response.IsSuccessStatusCode)
                        {
                            var json = await response.Content.ReadAsStringAsync();
                            return JsonConvert.DeserializeObject<ContributionsModel>(json);
                        }
                        else
                        {
                            if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
                            {
                                AccessTokenExpired?.Invoke(this, new EventArgs());
                            }
                        }
                    }
                }
            }
            catch (HttpRequestException e)
            {
                await e.LogExceptionAsync();

                if (e.Message.Contains("500"))
                {
                    RequestErrorOccurred?.Invoke(this, new EventArgs());
                }

                Debug.WriteLine($"SubmitContributionAsync HttpRequestException: {e}");
            }
            catch (Exception e)
            {
                await e.LogExceptionAsync();
                Debug.WriteLine($"SubmitContributionAsync Exception: {e}");
            }

            return null;
        }

        /// <summary>
        /// Updates an existing contribution, identified by the contribution ID
        /// </summary>
        /// <param name="contribution">Contribution to be updated</param>
        /// <returns>Bool to denote update success or failure</returns>
        public async Task<bool?> UpdateContributionAsync(ContributionsModel contribution)
        {
            if (contribution == null)
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
                        if (response.IsSuccessStatusCode)
                        {
                            return true;
                        }
                        else
                        {
                            if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
                            {
                                AccessTokenExpired?.Invoke(this, new EventArgs());
                            }
                        }
                    }
                }
            }
            catch (HttpRequestException e)
            {
                await e.LogExceptionAsync();

                if (e.Message.Contains("500"))
                {
                    RequestErrorOccurred?.Invoke(this, new EventArgs());
                }

                Debug.WriteLine($"UpdateContributionAsync HttpRequestException: {e}");
                return null;
            }
            catch (Exception e)
            {
                await e.LogExceptionAsync();

                Debug.WriteLine($"GetProfileAsync Exception: {e}");
            }

            return null;
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
                    if (response.IsSuccessStatusCode)
                    {
                        return true;
                    }
                    else
                    {
                        if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
                        {
                            AccessTokenExpired?.Invoke(this, new EventArgs());
                        }
                    }
                }
            }
            catch (HttpRequestException e)
            {
                await e.LogExceptionAsync();

                if (e.Message.Contains("500"))
                {
                    RequestErrorOccurred?.Invoke(this, new EventArgs());
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

            return null;
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
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<IReadOnlyList<ContributionTypeModel>>(json);
                    }
                    else
                    {
                        if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
                        {
                            AccessTokenExpired?.Invoke(this, new EventArgs());
                        }
                    }
                }
            }
            catch (HttpRequestException e)
            {
                await e.LogExceptionAsync();

                if (e.Message.Contains("500"))
                {
                    RequestErrorOccurred?.Invoke(this, new EventArgs());
                }

                Debug.WriteLine($"GetContributionTypesAsync HttpRequestException: {e}");
            }
            catch (Exception e)
            {
                await e.LogExceptionAsync();

                Debug.WriteLine($"GetContributionTypesAsync Exception: {e}");
            }

            return null;
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
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<IReadOnlyList<ContributionAreasRootItem>>(json);
                    }
                    else
                    {
                        if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
                        {
                            AccessTokenExpired?.Invoke(this, new EventArgs());
                        }
                    }
                }
            }
            catch (HttpRequestException e)
            {
                await e.LogExceptionAsync();

                if (e.Message.Contains("500"))
                {
                    RequestErrorOccurred?.Invoke(this, new EventArgs());
                }

                Debug.WriteLine($"GetContributionTechnologiesAsync HttpRequestException: {e}");
            }
            catch (Exception e)
            {
                await e.LogExceptionAsync();

                Debug.WriteLine($"GetContributionTechnologiesAsync Exception: {e}");
            }

            return null;
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
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<IReadOnlyList<VisibilityViewModel>>(json);

                    }
                    else
                    {
                        if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
                        {
                            AccessTokenExpired?.Invoke(this, new EventArgs());
                        }
                    }
                }
            }
            catch (HttpRequestException e)
            {
                await e.LogExceptionAsync();

                if (e.Message.Contains("500"))
                {
                    RequestErrorOccurred?.Invoke(this, new EventArgs());
                }

                Debug.WriteLine($"GetVisibilitiesAsync HttpRequestException: {e}");
            }
            catch (Exception e)
            {
                await e.LogExceptionAsync();

                Debug.WriteLine($"GetVisibilitiesAsync Exception: {e}");
            }

            return null;
        }

        public async Task<IReadOnlyList<OnlineIdentityViewModel>> GetOnlineIdentitiesAsync()
        {
            try
            {
                using (var response = await _client.GetAsync("https://mvpapi.azure-api.net/mvp/api/onlineidentities"))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<IReadOnlyList<OnlineIdentityViewModel>>(json);
                    }
                    else
                    {
                        if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
                        {
                            AccessTokenExpired?.Invoke(this, new EventArgs());
                        }
                    }
                }
            }
            catch (HttpRequestException e)
            {
                await e.LogExceptionAsync();

                if (e.Message.Contains("500"))
                {
                    RequestErrorOccurred?.Invoke(this, new EventArgs());
                }

                Debug.WriteLine($"GetOnlineIdentitiesAsync HttpRequestException: {e}");
            }
            catch (Exception e)
            {
                await e.LogExceptionAsync();
                Debug.WriteLine($"GetOnlineIdentitiesAsync Exception: {e}");
            }

            return null;
        }

        // TODO MVP API doesn't yet have support for submitting a new Online Identity
        //public async Task<OnlineIdentity> SubmitOnlineIdentityAsync(OnlineIdentityViewModel onlineIdentity)
        //{
        //    if (onlineIdentity == null)
        //        throw new NullReferenceException("The OnlineIdentity parameter was null.");

        //    try
        //    {
        //        var serializedOnlineIdentity = JsonConvert.SerializeObject(onlineIdentity);
        //        byte[] byteData = Encoding.UTF8.GetBytes(serializedOnlineIdentity);

        //        using (var content = new ByteArrayContent(byteData))
        //        {
        //            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        //            using (var response = await _client.PostAsync("https://mvpapi.azure-api.net/mvp/api/onlineidentities?", content))
        //            {
        //                if (response.IsSuccessStatusCode)
        //                {
        //                    var json = await response.Content.ReadAsStringAsync();
        //                    Debug.WriteLine($"OnlineIdentity Save JSON: {json}");

        //                    var result = JsonConvert.DeserializeObject<OnlineIdentity>(json);
        //                    Debug.WriteLine($"OnlineIdentity Save Result: ID {result.PrivateSiteId}");

        //                    return result;
        //                }
        //                else
        //                {
        //                    if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
        //                    {
        //                        AccessTokenExpired?.Invoke(this, new EventArgs());
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (HttpRequestException e)
        //    {
        //        await e.LogExceptionAsync();

        //        if (e.Message.Contains("500"))
        //        {
        //            RequestErrorOccurred?.Invoke(this, new EventArgs());
        //        }

        //        Debug.WriteLine($"SubmitOnlineIdentitiesAsync HttpRequestException: {e}");
        //    }
        //    catch (Exception e)
        //    {
        //        await e.LogExceptionAsync();

        //        Debug.WriteLine($"SubmitOnlineIdentitiesAsync Exception: {e}");
        //    }

        //    return null;
        //}

        public async Task<bool> DeleteOnlineIdentityAsync(OnlineIdentityViewModel onlineIdentity)
        {
            if (onlineIdentity == null)
                throw new NullReferenceException("The OnlineIdentity parameter was null.");

            try
            {
                using (var response = await _client.DeleteAsync($"https://mvpapi.azure-api.net/mvp/api/onlineidentities?id={onlineIdentity.PrivateSiteId}"))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        return true;
                    }
                    else
                    {
                        if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
                        {
                            AccessTokenExpired?.Invoke(this, new EventArgs());
                        }
                    }
                }
            }
            catch (HttpRequestException e)
            {
                await e.LogExceptionAsync();

                if (e.Message.Contains("500"))
                {
                    RequestErrorOccurred?.Invoke(this, new EventArgs());
                }

                Debug.WriteLine($"SubmitOnlineIdentitiesAsync HttpRequestException: {e}");
            }
            catch (Exception e)
            {
                await e.LogExceptionAsync();

                Debug.WriteLine($"SubmitOnlineIdentitiesAsync Exception: {e}");
            }

            return false;
        }

        #endregion

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}