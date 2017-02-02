using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using MvpApi.Models;
using Newtonsoft.Json;

namespace MvpApi
{
    public class MvpApiService
    {
        private readonly HttpClient client;

        public MvpApiService(string apiKey, string msaAccessToken)
        {
            client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiKey);
            client.DefaultRequestHeaders.Add("Authorization", msaAccessToken);
        }

        public async Task<ProfileRoot> GetProfileAsync()
        {
            using (var response = await client.GetAsync("https://mvpapi.azure-api.net/mvp/api/profile"))
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ProfileRoot>(json);
            }
        }

        public async Task<Stream> GetProfileImageAsync()
        {
            using (var response = await client.GetAsync("https://mvpapi.azure-api.net/mvp/api/profile/photo"))
            {
                var base64Result = await response.Content.ReadAsStringAsync();
                var bytes = Convert.FromBase64String(base64Result);

                using (var ms = new MemoryStream(bytes, 0, bytes.Length))
                {
                    ms.Position = 0;
                    return ms;
                }
            }
        }
        
        public async Task<ContributionRoot> GetContributionsAsync(int offset, int limit)
        {
            using (var response = await client.GetAsync($"https://mvpapi.azure-api.net/mvp/api/contributions/{offset}/{limit}"))
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ContributionRoot>(json);
            }
        }
    }
}
