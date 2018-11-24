using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using Newtonsoft.Json;

namespace MvpApi.Services.Apis
{
    public class ApiKeyManager
    {
        private static Lazy<ApiKeyManager> _instance = new Lazy<ApiKeyManager>(LazyThreadSafetyMode.ExecutionAndPublication);
        public static ApiKeyManager Instance => _instance.Value;

        /// <summary>
        /// Gets the private subscription and clientId keys needed for Api usage. sign up here for your own https://mvpapi.portal.azure-api.net/
        /// </summary>
        public ApiKeyManager()
        {
            Initialize();
        }
        
        public string SubscriptionKey { get; private set; }

        public string ClientId { get; private set; }
        
        private void Initialize()
        {
            using (var client = new HttpClient())
            {
                var fetchTask = client.GetStringAsync("https://dvlup.blob.core.windows.net/general-app-files/JsonConfigs/MvpCompanionKeys.json");
                var json = fetchTask.GetAwaiter().GetResult();
                var keys = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

                this.SubscriptionKey = keys["SubscriptionKey"];
                this.ClientId = keys["ClientId"];
            }
        }
    }
}