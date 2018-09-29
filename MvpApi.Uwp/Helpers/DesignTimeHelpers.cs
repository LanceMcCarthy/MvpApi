using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MvpApi.Common.Models;

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

        public static ObservableCollection<ContributionsModel> GenerateContributions() => new ObservableCollection<ContributionsModel>
        {
            new ContributionsModel
            {
                StartDate = DateTime.Now,
                Title = "Awesome Thing One",
                Description = "This is a very cool thing that I'm adding in for a contribution because I need some design time data :D",
                ReferenceUrl = "https:coolthing.com",
                UploadStatus = UploadStatus.Pending,
                AnnualQuantity = 1,
                SecondAnnualQuantity = 2,
                AnnualReach = 100,
                ContributionType = GenerateContributionTypes()[0],
                ContributionTypeName = GenerateContributionTypes()[0].Name,
                ContributionTechnology = GenerateAreaTechnologies()[0],
                Visibility = GenerateVisibilities()[0],
                AdditionalTechnologies = new ObservableCollection<ContributionTechnologyModel>()
                {
                    GenerateAreaTechnologies()[1],
                    GenerateAreaTechnologies()[2]
                }
            },
            new ContributionsModel
            {
                StartDate = DateTime.Now,
                Title = "Awesome Thing Two",
                Description = "This is a very cool thing that I'm adding in for a contribution because I need some design time data :D",
                ReferenceUrl = "https:coolthing.com",
                UploadStatus = UploadStatus.InProgress,
                AnnualQuantity = 1,
                SecondAnnualQuantity = 2,
                AnnualReach = 100,
                ContributionType = GenerateContributionTypes()[0],
                ContributionTypeName = GenerateContributionTypes()[0].Name,
                ContributionTechnology = GenerateAreaTechnologies()[0],
                Visibility = GenerateVisibilities()[0],
                AdditionalTechnologies = new ObservableCollection<ContributionTechnologyModel>()
                {
                    GenerateAreaTechnologies()[1],
                    GenerateAreaTechnologies()[2]
                }
            },
            new ContributionsModel
            {
                StartDate = DateTime.Now,
                Title = "Awesome Thing Three",
                Description = "This is a very cool thing that I'm adding in for a contribution because I need some design time data :D",
                ReferenceUrl = "https:coolthing.com",
                UploadStatus = UploadStatus.Failed,
                AnnualQuantity = 1,
                SecondAnnualQuantity = 2,
                AnnualReach = 100,
                ContributionType = GenerateContributionTypes()[0],
                ContributionTypeName = GenerateContributionTypes()[0].Name,
                ContributionTechnology = GenerateAreaTechnologies()[0],
                Visibility = GenerateVisibilities()[0],
                AdditionalTechnologies = new ObservableCollection<ContributionTechnologyModel>()
                {
                    GenerateAreaTechnologies()[1],
                    GenerateAreaTechnologies()[2]
                }
            },
            new ContributionsModel
            {
                StartDate = DateTime.Now,
                Title = "Awesome Thing Four",
                Description = "This is a very cool thing that I'm adding in for a contribution because I need some design time data :D",
                ReferenceUrl = "https:coolthing.com",
                UploadStatus = UploadStatus.Success,
                AnnualQuantity = 1,
                SecondAnnualQuantity = 2,
                AnnualReach = 100,
                ContributionType = GenerateContributionTypes()[0],
                ContributionTypeName = GenerateContributionTypes()[0].Name,
                ContributionTechnology = GenerateAreaTechnologies()[0],
                Visibility = GenerateVisibilities()[0],
                AdditionalTechnologies = new ObservableCollection<ContributionTechnologyModel>()
                {
                    GenerateAreaTechnologies()[1],
                    GenerateAreaTechnologies()[2]
                }
            }
        };

        public static ObservableCollection<ContributionTechnologyModel> GenerateAreaTechnologies() => new ObservableCollection<ContributionTechnologyModel>()
        {
            new ContributionTechnologyModel(new Guid("153fc913-6025-4ac1-99bc-79ffd5f34281"),"Tech Name 1","Tech Award Name 1","Tech Award Category 1"),
            new ContributionTechnologyModel(new Guid("253fc913-6025-4ac1-99bc-79ffd5f34281"),"Tech Name 2","Tech Award Name 2","Tech Award Category 2"),
            new ContributionTechnologyModel(new Guid("353fc913-6025-4ac1-99bc-79ffd5f34281"),"Tech Name 3","Tech Award Name 3","Tech Award Category 3")
        };

        public static ObservableCollection<ContributionTypeModel> GenerateContributionTypes() => new ObservableCollection<ContributionTypeModel>()
        {
            new ContributionTypeModel(new Guid("13062caa-6a7b-47a9-b58f-2b6a85b2cbb9"), "Type Name 1", "English Type Name 1"),
            new ContributionTypeModel(new Guid("23062caa-6a7b-47a9-b58f-2b6a85b2cbb9"), "Type Name 2", "English Type Name 2"),
            new ContributionTypeModel(new Guid("33062caa-6a7b-47a9-b58f-2b6a85b2cbb9"), "Type Name 3", "English Type Name 3"),
        };

        public static ObservableCollection<ContributionAreaContributionModel> GenerateTechnologyAreas() => new ObservableCollection<ContributionAreaContributionModel>()
        {
            new ContributionAreaContributionModel
            {
                AwardName = GenerateAreaTechnologies()[0].AwardName,
                ContributionAreas = GenerateAreaTechnologies()
            },
            new ContributionAreaContributionModel
            {
                AwardName = GenerateAreaTechnologies()[1].AwardName,
                ContributionAreas = GenerateAreaTechnologies()
            }
        };

        public static ObservableCollection<VisibilityViewModel> GenerateVisibilities() => new ObservableCollection<VisibilityViewModel>
        {
            new VisibilityViewModel(1, "Microsoft"),
            new VisibilityViewModel(2, "MVP Community"),
            new VisibilityViewModel(3, "Public")
        };
    }

}
