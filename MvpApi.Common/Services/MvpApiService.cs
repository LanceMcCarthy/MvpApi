using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using MvpApi.Common.Models;
using Newtonsoft.Json;

namespace MvpApi.Common.Services
{
    public class MvpApiService : IDisposable
    {
        private readonly HttpClient client;

        /// <summary>
        /// Service that interacts with the MVP API
        /// </summary>
        /// <param name="apiKey">the Ocp-Apim-Subscription-Key you got from your MVP API portal</param>
        /// <param name="authorizationHeader">Authorization header. Example: "Bearer AccessTokenGoesHere"</param>
        public MvpApiService(string apiKey, string authorizationHeader)
        {
            var handler = new HttpClientHandler();
            if (handler.SupportsAutomaticDecompression)
                handler.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

            client = new HttpClient(handler);
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiKey);
            client.DefaultRequestHeaders.Add("Authorization", authorizationHeader);
        }

        /// <summary>
        /// Returns the profile data of the currently signed in MVP
        /// </summary>
        /// <returns>The MVP's profile information</returns>
        public async Task<ProfileViewModel> GetProfileAsync()
        {
            try
            {
                using (var response = await client.GetAsync("https://mvpapi.azure-api.net/mvp/api/profile"))
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<ProfileViewModel>(json);
                }
            }
            catch (HttpRequestException e)
            {
                if (e.Message.Contains("401"))
                {
                    //TODO Try refresh token, access token is only valid for 60 minutes
                }

                Debug.WriteLine($"GetProfileAsync HttpRequestException: {e}");
                return null;
            }
            catch (Exception e)
            {
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
                using (var response = await client.GetAsync("https://mvpapi.azure-api.net/mvp/api/profile/photo"))
                {
                    var imageAsBase64 = await response.Content.ReadAsStringAsync();
                    imageAsBase64 = imageAsBase64.TrimStart('"').TrimEnd('"'); // need to trim the quotes before decoding
                    return Convert.FromBase64String(imageAsBase64);
                }
            }
            catch (HttpRequestException e)
            {
                if (e.Message.Contains("401"))
                {
                    //TODO Try refresh token, access token is only valid for 60 minutes
                }

                Debug.WriteLine($"GetProfileImageAsync HttpRequestException: {e}");
                return null;
            }
            catch (Exception e)
            {
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
            try
            {
                if (offset == null)
                    return null;

                using (var response = await client.GetAsync($"https://mvpapi.azure-api.net/mvp/api/contributions/{offset}/{limit}"))
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<ContributionViewModel>(json);
                }
            }
            catch (HttpRequestException e)
            {
                if (e.Message.Contains("401"))
                {
                    //TODO Try refresh token, access token is only valid for 60 minutes
                }

                Debug.WriteLine($"GetContributionsAsync HttpRequestException: {e}");
                return null;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"GetContributionsAsync Exception: {e}");
                return null;
            }
        }

        public void Dispose()
        {
            client?.Dispose();
        }
    }
}
