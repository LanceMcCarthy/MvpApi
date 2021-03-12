using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MvpApi.Common.Models;

namespace MvpCompanion.UI.Wpf.Helpers
{
    public static class DesignTimeHelpers
    {
        public static ProfileViewModel GenerateSampleMvp() => new ProfileViewModel
        {
            DisplayName = "Lance",
            FullName = "Lance McCarthy",
            AwardCategoryDisplay = "Windows Development",
            YearsAsMvp = 6,
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
            new VisibilityViewModel(299600000, "Microsoft", "MicrosoftVisibilityText"),
            new VisibilityViewModel(299600003, "MVP Community", "MVPVisibilityText"),
            new VisibilityViewModel(299600002, "Public", "PublicVisibilityText")
        };

        public static ObservableCollection<OnlineIdentityViewModel> GenerateOnlineIdentities() => new ObservableCollection<OnlineIdentityViewModel>
        {
            // Linked Identity
            new OnlineIdentityViewModel
            {
                PrivateSiteId = 96470,
                SocialNetwork = new SocialNetworkViewModel
                {
                    Id = Guid.NewGuid(),
                    Name = "Exchange DL Subscription Email",
                    IconUrl = "~/Content/Images/socialGeneric.png",
                    SystemCollectionEnabled = false
                },
                Url = "http://lance.mccarthy@live.com",
                OnlineIdentityVisibility = GenerateVisibilities()[0],
                ContributionCollected = false,
                DisplayName = "",
                UserId = null,
                MicrosoftAccount = null,
                PrivacyConsentStatus = true,
                PrivacyConsentCheckStatus = false,
                PrivacyConsentCheckDate = null,
                PrivacyConsentUnCheckDate = null,
                Submitted = false
            },
            new OnlineIdentityViewModel
            {
                PrivateSiteId = 130761,
                SocialNetwork = new SocialNetworkViewModel
                {
                    Id = Guid.NewGuid(),
                    Name = "Exchange DL Subscription Email",
                    IconUrl = "~/Content/Images/socialGeneric.png",
                    SystemCollectionEnabled = false
                },
                Url = "http://windowsappdevelopment-mvp-nda@mstechdiscussions.com",
                OnlineIdentityVisibility = GenerateVisibilities()[1],
                ContributionCollected = false,
                DisplayName = "",
                UserId = null,
                MicrosoftAccount = null,
                PrivacyConsentStatus = true,
                PrivacyConsentCheckStatus = false,
                PrivacyConsentCheckDate = null,
                PrivacyConsentUnCheckDate = null,
                Submitted = false
            },
            // other identities
            new OnlineIdentityViewModel
            {
                PrivateSiteId = 96471,
                SocialNetwork = new SocialNetworkViewModel
                {
                    Id = Guid.NewGuid(),
                    Name = "Xbox Live gamertag",
                    IconUrl = "~/Content/Images/xbox.png",
                    SystemCollectionEnabled = false
                },
                Url = "OuttaSyyte",
                OnlineIdentityVisibility = GenerateVisibilities()[0],
                ContributionCollected = false,
                DisplayName = "",
                UserId = null,
                MicrosoftAccount = null,
                PrivacyConsentStatus = true,
                PrivacyConsentCheckStatus = false,
                PrivacyConsentCheckDate = null,
                PrivacyConsentUnCheckDate = null,
                Submitted = false
            },
            new OnlineIdentityViewModel
            {
                PrivateSiteId = 96471,
                SocialNetwork = new SocialNetworkViewModel
                {
                    Id = Guid.NewGuid(),
                    Name = "Klout",
                    IconUrl = "~/Content/Images/socialKlout.png",
                    SystemCollectionEnabled = false
                },
                Url = "https://klout.com/#/lancewmccarthy",
                OnlineIdentityVisibility = GenerateVisibilities()[0],
                ContributionCollected = false,
                DisplayName = "",
                UserId = null,
                MicrosoftAccount = null,
                PrivacyConsentStatus = true,
                PrivacyConsentCheckStatus = false,
                PrivacyConsentCheckDate = null,
                PrivacyConsentUnCheckDate = null,
                Submitted = false
            },
            new OnlineIdentityViewModel
            {
                PrivateSiteId = 40775,
                SocialNetwork = new SocialNetworkViewModel
                {
                    Id = Guid.NewGuid(),
                    Name = "LinkedIn",
                    IconUrl = "~/Content/Images/socialLi.png",
                    SystemCollectionEnabled = false
                },
                Url = "http://www.linkedin.com/in/lancewmccarthy/",
                OnlineIdentityVisibility = GenerateVisibilities()[1],
                ContributionCollected = false,
                DisplayName = "",
                UserId = null,
                MicrosoftAccount = null,
                PrivacyConsentStatus = true,
                PrivacyConsentCheckStatus = false,
                PrivacyConsentCheckDate = null,
                PrivacyConsentUnCheckDate = null,
                Submitted = false
            },
            new OnlineIdentityViewModel
            {
                PrivateSiteId = 40777,
                SocialNetwork = new SocialNetworkViewModel
                {
                    Id = Guid.NewGuid(),
                    Name = "MSDN/Technet",
                    IconUrl = "~/Content/Images/socialTN.png",
                    SystemCollectionEnabled = true
                },
                Url = "http://social.technet.microsoft.com/profile/lancelot%20software/",
                OnlineIdentityVisibility = GenerateVisibilities()[0],
                ContributionCollected = false,
                DisplayName = "",
                UserId = null,
                MicrosoftAccount = null,
                PrivacyConsentStatus = null,
                PrivacyConsentCheckStatus = false,
                PrivacyConsentCheckDate = null,
                PrivacyConsentUnCheckDate = null,
                Submitted = false
            },
            new OnlineIdentityViewModel
            {
                PrivateSiteId = 40772,
                SocialNetwork = new SocialNetworkViewModel
                {
                    Id = Guid.NewGuid(),
                    Name = "Microsoft Connect",
                    IconUrl = "~/Content/Images/socialGeneric.png",
                    SystemCollectionEnabled = true
                },
                Url = "lance.mccarthy@live.com",
                OnlineIdentityVisibility = GenerateVisibilities()[0],
                ContributionCollected = false,
                DisplayName = "",
                UserId = null,
                MicrosoftAccount = null,
                PrivacyConsentStatus = null,
                PrivacyConsentCheckStatus = false,
                PrivacyConsentCheckDate = null,
                PrivacyConsentUnCheckDate = null,
                Submitted = false
            },
            new OnlineIdentityViewModel
            {
                PrivateSiteId = 40773,
                SocialNetwork = new SocialNetworkViewModel
                {
                    Id = Guid.NewGuid(),
                    Name = "Twitter",
                    IconUrl = "~/Content/Images/socialTwitter.png",
                    SystemCollectionEnabled = true
                },
                Url = "http://twitter.com/lancewmccarthy",
                OnlineIdentityVisibility = GenerateVisibilities()[2],
                ContributionCollected = false,
                DisplayName = "",
                UserId = null,
                MicrosoftAccount = null,
                PrivacyConsentStatus = true,
                PrivacyConsentCheckStatus = false,
                PrivacyConsentCheckDate = null,
                PrivacyConsentUnCheckDate = null,
                Submitted = false
            },
            new OnlineIdentityViewModel
            {
                PrivateSiteId = 40782,
                SocialNetwork = new SocialNetworkViewModel
                {
                    Id = Guid.NewGuid(),
                    Name = "YouTube",
                    IconUrl = "~/Content/Images/socialYT.png",
                    SystemCollectionEnabled = false
                },
                Url = "http://www.youtube.com/user/outtasyyte/",
                OnlineIdentityVisibility = GenerateVisibilities()[2],
                ContributionCollected = false,
                DisplayName = "",
                UserId = null,
                MicrosoftAccount = null,
                PrivacyConsentStatus = true,
                PrivacyConsentCheckStatus = false,
                PrivacyConsentCheckDate = null,
                PrivacyConsentUnCheckDate = null,
                Submitted = false
            },
            new OnlineIdentityViewModel
            {
                PrivateSiteId = 130762,
                SocialNetwork = new SocialNetworkViewModel
                {
                    Id = Guid.NewGuid(),
                    Name = "YouTube",
                    IconUrl = "~/Content/Images/socialYT.png",
                    SystemCollectionEnabled = false
                },
                Url = "https://www.youtube.com/channel/UCV6JsDh22ENSF8DPvHBAs-w",
                OnlineIdentityVisibility = GenerateVisibilities()[2],
                ContributionCollected = false,
                DisplayName = "",
                UserId = null,
                MicrosoftAccount = null,
                PrivacyConsentStatus = true,
                PrivacyConsentCheckStatus = false,
                PrivacyConsentCheckDate = null,
                PrivacyConsentUnCheckDate = null,
                Submitted = false
            },
            new OnlineIdentityViewModel
            {
                PrivateSiteId = 96469,
                SocialNetwork = new SocialNetworkViewModel
                {
                    Id = Guid.NewGuid(),
                    Name = "GitHub",
                    IconUrl = "~/Content/Images/github.png",
                    SystemCollectionEnabled = false
                },
                Url = "https://github.com/LanceMcCarthy",
                OnlineIdentityVisibility = GenerateVisibilities()[2],
                ContributionCollected = false,
                DisplayName = "",
                UserId = null,
                MicrosoftAccount = null,
                PrivacyConsentStatus = true,
                PrivacyConsentCheckStatus = false,
                PrivacyConsentCheckDate = null,
                PrivacyConsentUnCheckDate = null,
                Submitted = false
            },
            new OnlineIdentityViewModel
            {
                PrivateSiteId = 96578,
                SocialNetwork = new SocialNetworkViewModel
                {
                    Id = Guid.NewGuid(),
                    Name = "StackOverflow",
                    IconUrl = "~/Content/Images/StackOverflow.png",
                    SystemCollectionEnabled = false
                },
                Url = "http://stackoverflow.com/users/1406210/lance-mccarthy",
                OnlineIdentityVisibility = GenerateVisibilities()[0],
                ContributionCollected = false,
                DisplayName = "",
                UserId = null,
                MicrosoftAccount = null,
                PrivacyConsentStatus = true,
                PrivacyConsentCheckStatus = false,
                PrivacyConsentCheckDate = null,
                PrivacyConsentUnCheckDate = null,
                Submitted = false
            },
            new OnlineIdentityViewModel
            {
                PrivateSiteId = 96578,
                SocialNetwork = new SocialNetworkViewModel
                {
                    Id = Guid.NewGuid(),
                    Name = "StackOverflow",
                    IconUrl = "~/Content/Images/StackOverflow.png",
                    SystemCollectionEnabled = false
                },
                Url = "http://windowsphone.stackexchange.com/users/141/lance-mccarthy",
                OnlineIdentityVisibility = GenerateVisibilities()[0],
                ContributionCollected = false,
                DisplayName = "",
                UserId = null,
                MicrosoftAccount = null,
                PrivacyConsentStatus = true,
                PrivacyConsentCheckStatus = false,
                PrivacyConsentCheckDate = null,
                PrivacyConsentUnCheckDate = null,
                Submitted = false
            },
            new OnlineIdentityViewModel
            {
                PrivateSiteId = 96472,
                SocialNetwork = new SocialNetworkViewModel
                {
                    Id = Guid.NewGuid(),
                    Name = "Windows/Windows Phone Dev Center ID",
                    IconUrl = "~/Content/Images/windowsPhone.png",
                    SystemCollectionEnabled = false
                },
                Url = "https://www.microsoft.com/en-us/search/result.aspx?q=Lancelot+Software&form=apps",
                OnlineIdentityVisibility = GenerateVisibilities()[0],
                ContributionCollected = false,
                DisplayName = "",
                UserId = null,
                MicrosoftAccount = null,
                PrivacyConsentStatus = true,
                PrivacyConsentCheckStatus = false,
                PrivacyConsentCheckDate = null,
                PrivacyConsentUnCheckDate = null,
                Submitted = false
            },
            new OnlineIdentityViewModel
            {
                PrivateSiteId = 40776,
                SocialNetwork = new SocialNetworkViewModel
                {
                    Id = Guid.NewGuid(),
                    Name = "Blog",
                    IconUrl = "~/Content/Images/socialGeneric.png",
                    SystemCollectionEnabled = false
                },
                Url = "https://dvlup.com",
                OnlineIdentityVisibility = GenerateVisibilities()[2],
                ContributionCollected = false,
                DisplayName = "",
                UserId = null,
                MicrosoftAccount = null,
                PrivacyConsentStatus = true,
                PrivacyConsentCheckStatus = false,
                PrivacyConsentCheckDate = null,
                PrivacyConsentUnCheckDate = null,
                Submitted = false
            },
            new OnlineIdentityViewModel
            {
                PrivateSiteId = 130760,
                SocialNetwork = new SocialNetworkViewModel
                {
                    Id = Guid.NewGuid(),
                    Name = "Meetup",
                    IconUrl = "~/Content/Images/socialGeneric.png",
                    SystemCollectionEnabled = false
                },
                Url = "https://www.meetup.com/members/14958841/",
                OnlineIdentityVisibility = GenerateVisibilities()[2],
                ContributionCollected = false,
                DisplayName = "",
                UserId = null,
                MicrosoftAccount = null,
                PrivacyConsentStatus = true,
                PrivacyConsentCheckStatus = false,
                PrivacyConsentCheckDate = null,
                PrivacyConsentUnCheckDate = null,
                Submitted = false
            },
            new OnlineIdentityViewModel
            {
                PrivateSiteId = 130759,
                SocialNetwork = new SocialNetworkViewModel
                {
                    Id = Guid.NewGuid(),
                    Name = "Instagram",
                    IconUrl = "~/Content/Images/socialGeneric.png",
                    SystemCollectionEnabled = false
                },
                Url = "https://www.instagram.com/ambassadorlance/",
                OnlineIdentityVisibility = GenerateVisibilities()[2],
                ContributionCollected = false,
                DisplayName = "",
                UserId = null,
                MicrosoftAccount = null,
                PrivacyConsentStatus = true,
                PrivacyConsentCheckStatus = false,
                PrivacyConsentCheckDate = null,
                PrivacyConsentUnCheckDate = null,
                Submitted = false
            }
        };

        public static ObservableCollection<QuestionnaireItem> GenerateQuestionnaireItems()
        {
            var items = new ObservableCollection<QuestionnaireItem>();

            items.Add(new QuestionnaireItem
            {
                QuestionItem = new AwardConsiderationQuestionModel
                {
                    AwardQuestionId = "13062caa-6a7b-47a9-b58f-2b6a85b2c123",
                    IsRequired = true,
                    QuestionContent = "What does it mean to be an MVP for you?"
                },
                AnswerItem = new AwardConsiderationAnswerModel
                {
                    AwardQuestionId = "13062caa-6a7b-47a9-b58f-2b6a85b2c123"
                }
            });

            items.Add(new QuestionnaireItem
            {
                QuestionItem = new AwardConsiderationQuestionModel
                {
                    AwardQuestionId = "13062caa-6a7b-47a9-b58f-2b6a85b2c234",
                    IsRequired = true,
                    QuestionContent = "What do you plan on doing for the next year as a the go-to source for helping the developer community?"
                },
                AnswerItem = new AwardConsiderationAnswerModel
                {
                    AwardQuestionId = "13062caa-6a7b-47a9-b58f-2b6a85b2c234"
                }
            });

            items.Add(new QuestionnaireItem
            {
                QuestionItem = new AwardConsiderationQuestionModel
                {
                    AwardQuestionId = "13062caa-6a7b-47a9-b58f-2b6a85b2c345",
                    IsRequired = true,
                    QuestionContent = "How can the MVP program help better enable you to prive the best support possible?"
                },
                AnswerItem = new AwardConsiderationAnswerModel
                {
                    AwardQuestionId = "13062caa-6a7b-47a9-b58f-2b6a85b2c345"
                }
            });

            items.Add(new QuestionnaireItem
            {
                QuestionItem = new AwardConsiderationQuestionModel
                {
                    AwardQuestionId = "13062caa-6a7b-47a9-b58f-2b6a85b2c456",
                    IsRequired = true,
                    QuestionContent = "Whatcha gonna do, whatcha gonna do, whatcha gonna do when they come for you?"
                },
                AnswerItem = new AwardConsiderationAnswerModel
                {
                    AwardQuestionId = "13062caa-6a7b-47a9-b58f-2b6a85b2c456"
                }
            });

            return items;
        }
    }
}