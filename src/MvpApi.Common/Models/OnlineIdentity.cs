// Code generated by Microsoft (R) AutoRest Code Generator 0.16.0.0
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.

using System;
using Newtonsoft.Json;

namespace MvpApi.Common.Models
{
    public class OnlineIdentity
    {
        /// <summary>
        /// Initializes a new instance of the OnlineIdentity class.
        /// </summary>
        public OnlineIdentity() { }

        public OnlineIdentity(string onlineIdentityId, string mvpGuid, string name, int? privateSiteId, SharingPreference onlineIdentityVisibility, SocialNetwork socialNetwork, string url, string displayName, string userId, string microsoftAccount, bool? contributionCollected, bool? privacyConsentStatus, bool? submitted)
        {
            OnlineIdentityId = onlineIdentityId;
            MvpGuid = mvpGuid;
            Name = name;
            PrivateSiteId = privateSiteId;
            OnlineIdentityVisibility = onlineIdentityVisibility;
            SocialNetwork = socialNetwork;
            Url = url;
            DisplayName = displayName;
            UserId = userId;
            MicrosoftAccount = microsoftAccount;
            ContributionCollected = contributionCollected;
            PrivacyConsentStatus = privacyConsentStatus;
            Submitted = submitted;
        }

        [Newtonsoft.Json.JsonProperty(PropertyName = "OnlineIdentityId")]
		public string OnlineIdentityId { get; set; }

		[Newtonsoft.Json.JsonProperty(PropertyName = "MvpGuid")]
		public string MvpGuid { get; set; }

		[Newtonsoft.Json.JsonProperty(PropertyName = "Name")]
		public string Name { get; set; }

		[Newtonsoft.Json.JsonProperty(PropertyName = "PrivateSiteId")]
		public int? PrivateSiteId { get; set; }

		[Newtonsoft.Json.JsonProperty(PropertyName = "OnlineIdentityVisibility")]
		public SharingPreference OnlineIdentityVisibility { get; set; }

		[Newtonsoft.Json.JsonProperty(PropertyName = "SocialNetwork")]
		public SocialNetwork SocialNetwork { get; set; }

		[Newtonsoft.Json.JsonProperty(PropertyName = "Url")]
		public string Url { get; set; }

		[Newtonsoft.Json.JsonProperty(PropertyName = "DisplayName")]
		public string DisplayName { get; set; }

		[Newtonsoft.Json.JsonProperty(PropertyName = "UserId")]
		public string UserId { get; set; }

		[Newtonsoft.Json.JsonProperty(PropertyName = "MicrosoftAccount")]
		public string MicrosoftAccount { get; set; }

		[Newtonsoft.Json.JsonProperty(PropertyName = "ContributionCollected")]
		public bool? ContributionCollected { get; set; }

		[Newtonsoft.Json.JsonProperty(PropertyName = "PrivacyConsentStatus")]
		public bool? PrivacyConsentStatus { get; set; }

		[Newtonsoft.Json.JsonProperty(PropertyName = "Submitted")]
		public bool? Submitted { get; set; }
	}
}
