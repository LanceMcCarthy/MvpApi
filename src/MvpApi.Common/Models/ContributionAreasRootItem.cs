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

        // NOTE
        // This type was changed ContributionAreaModel to ContributionTechnologyModel
        // Justification: They are the same thing

        // - A ContributionAreaModel is returned from GetContributionAreasAsync();
        // - A ContributionTechnologyModel is used for SubmitContributionAsync() and GetContributionsAsync()

        // I collpased them into a single object that can be used for both so that the UI doesn't have uneccessary code

        // OLD
        //[JsonProperty(PropertyName = "ContributionArea")]
        //public IReadOnlyList<ContributionAreaModel> ContributionAreas { get; set; }

        // NEW
        [JsonProperty(PropertyName = "ContributionArea")]
        public IReadOnlyList<ContributionTechnologyModel> ContributionAreas { get; set; }
    }
}