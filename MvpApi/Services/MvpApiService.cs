using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ModernHttpClient;
using MvpApi.Models;
using Newtonsoft.Json;

namespace MvpApi.Services
{
    public class MvpApiService
    {
        private readonly HttpClient client;

        /// <summary>
        /// Service that interacts with the MVP API
        /// </summary>
        /// <param name="apiKey">the Ocp-Apim-Subscription-Key you got from your MVP API portal</param>
        /// <param name="msaAccessToken">Clean access token (e.g. minus any "lc=" paramaters)</param>
        public MvpApiService(string apiKey, string msaAccessToken)
        {
            client = new HttpClient(new NativeMessageHandler());
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiKey);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {msaAccessToken}");
        }

        /// <summary>
        /// Returns the profile data of the currently signed in MVP
        /// </summary>
        /// <returns>The MVP's profile information</returns>
        public async Task<ProfileViewModel> GetProfileAsync()
        {
            using (var response = await client.GetAsync("https://mvpapi.azure-api.net/mvp/api/profile"))
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ProfileViewModel>(json);
            }
        }
        
        /// <summary>
        /// Get the profile picture of the currently signed in MVP
        /// </summary>
        /// <returns>JPG image byte array</returns>
        public async Task<byte[]> GetProfileImageAsync()
        {
            // the result is Detected mime type: image/jpeg; charset=binary
            using (var response = await client.GetAsync("https://mvpapi.azure-api.net/mvp/api/profile/photo"))
            {
                var imageAsBase64 = await response.Content.ReadAsStringAsync();
                imageAsBase64 = imageAsBase64.TrimStart('"').TrimEnd('"'); // need to trim the quotes before decoding
                return Convert.FromBase64String(imageAsBase64);
            }
        }

        /// <summary>
        /// Gets the MVPs activities, depending on the offset (page) and the limit (number of items per-page)
        /// </summary>
        /// <param name="offset">page to return</param>
        /// <param name="limit">number of items for the page</param>
        /// <returns></returns>
        public async Task<ContributionViewModel> GetContributionsAsync(int offset, int limit)
        {
            using (var response = await client.GetAsync($"https://mvpapi.azure-api.net/mvp/api/contributions/{offset}/{limit}"))
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ContributionViewModel>(json);
            }
        }
    }
}
