using System.Collections.Generic;

namespace MvpApi.Models
{
    public class ProfileRoot
    {
        public Metadata Metadata { get; set; }
        public string MvpId { get; set; }
        public int YearsAsMvp { get; set; }
        public string FirstAwardYear { get; set; }
        public string AwardCategoryDisplay { get; set; }
        public string TechnicalExpertise { get; set; }
        public bool InTheSpotlight { get; set; }
        public string Headline { get; set; }
        public string Biography { get; set; }
        public string DisplayName { get; set; }
        public string FullName { get; set; }
        public object PrimaryEmailAddress { get; set; }
        public string ShippingCountry { get; set; }
        public string ShippingStateCity { get; set; }
        public string Languages { get; set; }
        public object OnlineIdentities { get; set; }
        public List<Certification> Certifications { get; set; }
        public object Activities { get; set; }
        public List<object> CommunityAwards { get; set; }
        public List<object> NewsHighlights { get; set; }
        public List<object> UpcomingEvent { get; set; }
    }
}
