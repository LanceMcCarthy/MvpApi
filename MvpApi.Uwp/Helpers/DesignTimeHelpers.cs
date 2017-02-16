using System;
using System.Collections.Generic;
using MvpApi.Models;

namespace MvpApi.Uwp.Helpers
{
    public static class DesignTimeHelpers
    {
        public static ProfileViewModel GenerateSampleMvp() => new ProfileViewModel
        {
            DisplayName = "Lance",
            FullName = "Lance McCarthy",
            AwardCategoryDisplay = "Windows Development",
            YearsAsMvp = 4,
            Headline = "Mvp stuffs here",
            Certifications = GenerateCertifications(),
            CommunityAwards = GenerateAwards(),
            Activities = GenerateActivities()
        };
        

        public static List<CertificationViewModel> GenerateCertifications() => new List<CertificationViewModel>
        {
            new CertificationViewModel
            {
                Title = "CERT1",
                CertificationVisibility = new VisibilityViewModel { Description = "The visibility", Id = 1 },
                Id = Guid.NewGuid()
            },
            new CertificationViewModel
            {
                Title = "CERT2",
                CertificationVisibility = new VisibilityViewModel { Description = "The visibility", Id = 2 },
                Id = Guid.NewGuid()
            }
        };

        public static List<AwardRecognitionViewModel> GenerateAwards() => new List<AwardRecognitionViewModel>
        {
            new AwardRecognitionViewModel()
            {
                 Title = "Award 1",
                 DateEarned = DateTime.Now,
                 AwardRecognitionVisibility = new VisibilityViewModel { Description = "The visibility", Id = 1 },
                 Description = "This is an award",
                 ReferenceUrl = "http://bing.com"
            },
            new AwardRecognitionViewModel()
            {
                 Title = "Award 2",
                 DateEarned = DateTime.Now,
                 AwardRecognitionVisibility = new VisibilityViewModel { Description = "The visibility", Id = 2 },
                 Description = "This is an award",
                 ReferenceUrl = "http://bing.com"
            }
        };

        public static List<ActivityViewModel> GenerateActivities() => new List<ActivityViewModel>
        {
            new ActivityViewModel()
            {
                 ActivityVisibility = new VisibilityViewModel { Description = "The visibility", Id = 1 },
                 Description = "This is an activity",
                 ReferenceUrl = "http://bing.com",
                 ActivityType = new ActivityTypeViewModel() { Name = "Name Here", EnglishName = "English name here", Id = Guid.NewGuid()},
                 AnnualQuantity = 100,
                 SecondAnnualQuantity = 100,
                 AllAnswersUrl = "http://stackoverflow.com",
                 AnnualReach = 100,
                 AllPostsUrl = "http://stackoverflow.com",
                 ApplicableTechnology = new ActivityTechnologyViewModel() {AwardName = "Award name", AwardCategory = "WinDev", Name = "Windows Dev", Active = true, Id = Guid.NewGuid(), Statuscode = 1}
            },
            new ActivityViewModel()
            {
                 ActivityVisibility = new VisibilityViewModel { Description = "The visibility", Id = 1 },
                 Description = "This is an activity",
                 ReferenceUrl = "http://bing.com",
                 ActivityType = new ActivityTypeViewModel() { Name = "Name Here", EnglishName = "English name here", Id = Guid.NewGuid()},
                 AnnualQuantity = 100,
                 SecondAnnualQuantity = 100,
                 AllAnswersUrl = "http://stackoverflow.com",
                 AnnualReach = 100,
                 AllPostsUrl = "http://stackoverflow.com",
                 ApplicableTechnology = new ActivityTechnologyViewModel() {AwardName = "Award name", AwardCategory = "WinDev", Name = "Windows Dev", Active = true, Id = Guid.NewGuid(), Statuscode = 1}
            },
        };
    }

}
