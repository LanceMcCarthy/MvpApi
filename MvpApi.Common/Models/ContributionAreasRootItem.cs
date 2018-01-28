using System.Collections.Generic;
using Newtonsoft.Json;

namespace MvpApi.Common.Models
{
    public class ContributionAreasRootItem
    {
        [JsonProperty(PropertyName = "AwardCategory")]
        public string AwardCategory { get; set; }

        [JsonProperty(PropertyName = "Contributions")]
        public IReadOnlyList<ContributionAreaContributionModel> Contributions { get; set; }
    }
    
    public class ContributionAreaContributionModel
    {
        [JsonProperty(PropertyName = "AwardName")]
        public string AwardName { get; set; }

        [JsonProperty(PropertyName = "ContributionArea")]
        public IReadOnlyList<ContributionAreaModel> ContributionAreas { get; set; }
    }
}