// Code generated by Microsoft (R) AutoRest Code Generator 0.16.0.0
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.

using Newtonsoft.Json;

namespace MvpApi.Common.Models
{
    public class SocialNetworkStatusCode
    {
        /// <summary>
        /// Initializes a new instance of the SocialNetworkStatusCode class.
        /// </summary>
        public SocialNetworkStatusCode() { }

        public SocialNetworkStatusCode(int? id, string description)
        {
            Id = id;
            Description = description;
        }

        [Newtonsoft.Json.JsonProperty(PropertyName = "Id")]
        public System.Int32? Id { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "Description")]
        public string Description { get; set; }
    }
}
