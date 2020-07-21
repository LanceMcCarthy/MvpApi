using System;
using System.Collections.ObjectModel;
using System.Linq;
using MvpApi.Common.Models;
using Newtonsoft.Json;

namespace MvpApi.Common.Extensions
{
    public static class ContributionExtensions
    {
        /// <summary>
        /// Validates the values of a ContributionsModel instance. 
        /// </summary>
        /// <param name="contribution">The contribution to be validated</param>
        /// <returns>The bool parameter denotes a successful validation. If unsuccessful, the string parameter will have the name of the field that is incomplete</returns>
        public static Tuple<bool, string> Validate(this ContributionsModel contribution)
        {
            if(contribution == null)
                throw new NullReferenceException("The contribution was null");

            var isValid = true;
            var failedFieldName = "";
            
            // Check title
            if (string.IsNullOrEmpty(contribution?.Title))
            {
                failedFieldName = "Title";
                isValid = false;
            }

            if (contribution.ContributionType == null)
            {
                failedFieldName = "Contribution Type";
                isValid = false;
            }

            // Check the contribution area
            if (contribution.ContributionTechnology == null)
            {
                failedFieldName = "Contribution Technology";
                isValid = false;
            }
            
            // *** ContributionType Specific Condition ***

            // Gets a Tuple that contains the specific conditions of a chosen ContributionType
            var typeRequirements = contribution.ContributionType.GetContributionTypeRequirements(); //contribution.ContributionType.GetContributionTypeRequirementsById();

            // Check Url
            if (string.IsNullOrEmpty(contribution.ReferenceUrl) && typeRequirements.Item4)
            {
                failedFieldName = "url";
                isValid = false;
            }

            // Check AnnualQuantity
            if (contribution.AnnualQuantity == null && !string.IsNullOrEmpty(typeRequirements.Item1))
            {
                failedFieldName = "First Quantity";
                isValid = false;
            }

            // Check SecondAnnualQuantity
            if (contribution.SecondAnnualQuantity == null && !string.IsNullOrEmpty(typeRequirements.Item2))
            {
                failedFieldName = "Second Quantity";
                isValid = false;
            }

            // Check AnnualReach
            if (contribution.AnnualReach == null && !string.IsNullOrEmpty(typeRequirements.Item3))
            {
                failedFieldName = "Annual Reach";
                isValid = false;
            }

            // *** end of ContributionType check ***

            // Check Visibility
            if (contribution.Visibility == null)
            {
                failedFieldName = "Visibility";
                isValid = false;
            }

            return new Tuple<bool, string>(isValid, failedFieldName);
        }
        
        //public static Tuple<bool, bool, bool, bool> GetContributionTypeRequirementsById(this ContributionTypeModel contributionType)
        //{
        //    bool isUrlRequired = false;
        //    bool isAnnualQuantityRequired = false;
        //    bool isSecondAnnualQuantityRequired = false;
        //    bool isAnnualReachRequired = false;

        //    var guidString = contributionType.Id.ToString();

        //    switch (guidString)
        //    {
        //        case "e36464de-179a-e411-bbc8-6c3be5a82b68": //"EnglishName": "Article"
        //            isAnnualQuantityRequired = true;
        //            isAnnualReachRequired = true;
        //            break;
        //        case "db6464de-179a-e411-bbc8-6c3be5a82b68": //"EnglishName": "Book (Author)"
        //        case "dd6464de-179a-e411-bbc8-6c3be5a82b68": // "EnglishName": "Book (Co-Author)"
        //        case "f16464de-179a-e411-bbc8-6c3be5a82b68": // "EnglishName": "Conference (Staffing)"
        //        case "0ce0dc15-0304-e911-8171-3863bb2bca60": // "EnglishName": "Docs.Microsoft.com Contribution"
        //        case "f96464de-179a-e411-bbc8-6c3be5a82b68": // "EnglishName": "Forum Moderator"
        //        case "f76464de-179a-e411-bbc8-6c3be5a82b68": //  "EnglishName": "Mentorship"
        //        case "d2d96407-0304-e911-8171-3863bb2bca60": // "EnglishName": "Microsoft Open Source Projects"
        //        case "414bcf30-e889-e511-8110-c4346bac0abc": // "EnglishName": "Non-Microsoft Open Source Projects"
        //        case "fd6464de-179a-e411-bbc8-6c3be5a82b68": // "EnglishName": "Organizer (User Group/Meetup/Local Events)"
        //        case "ef6464de-179a-e411-bbc8-6c3be5a82b68": // "EnglishName": "Organizer of Conference"
        //        case "ff6464de-179a-e411-bbc8-6c3be5a82b68": // "EnglishName": "Other"
        //        case "016564de-179a-e411-bbc8-6c3be5a82b68": // "EnglishName": "Product Group Feedback (General)"
        //        case "fb6464de-179a-e411-bbc8-6c3be5a82b68": // "EnglishName": "Site Owner"
        //        case "d16464de-179a-e411-bbc8-6c3be5a82b68": // "EnglishName": "Speaking (Conference)"
        //        case "d56464de-179a-e411-bbc8-6c3be5a82b68": // "EnglishName": "Speaking (User Group/Meetup/Local events)"
        //        case "056564de-179a-e411-bbc8-6c3be5a82b68": // "EnglishName": "Translation Review, Feedback and Editing"
        //        case "0ee0dc15-0304-e911-8171-3863bb2bca60": // "EnglishName": "Workshop/Volunteer/Proctor"
        //            isAnnualQuantityRequired = true;
        //            break;
        //        case "df6464de-179a-e411-bbc8-6c3be5a82b68": // "EnglishName": "Blog/Website Post"
        //        case "d76464de-179a-e411-bbc8-6c3be5a82b68": // "EnglishName": "Forum Participation (Microsoft Forums)"
        //        case "e96464de-179a-e411-bbc8-6c3be5a82b68": // "EnglishName": "Sample Code/Projects/Tools"
        //        case "eb6464de-179a-e411-bbc8-6c3be5a82b68": // "EnglishName": "Technical Social Media (Twitter, Facebook, LinkedIn...)"
        //        case "e56464de-179a-e411-bbc8-6c3be5a82b68": // "EnglishName": "Video/Webcast/Podcast"
        //            isUrlRequired = true;
        //            isAnnualQuantityRequired = true;
        //            break;
        //        case "d96464de-179a-e411-bbc8-6c3be5a82b68": // "EnglishName": "Forum Participation (3rd Party forums)"
        //            isUrlRequired = true;
        //            isAnnualQuantityRequired = true;
        //            isSecondAnnualQuantityRequired = true;
        //            break;
        //    }

        //    return new Tuple<bool, bool, bool, bool>(isUrlRequired, isAnnualQuantityRequired, isSecondAnnualQuantityRequired, isAnnualReachRequired);
        //}

        /// <summary>
        /// Titles of the quantities. These title are used to describe what the value is being entered. For example AnnualQuantity could be for Number of Article
        /// </summary>
        /// <param name="contributionType"></param>
        /// <returns>AnnualQuantityHeader, SecondAnnualQuantityHeader, AnnualReachHeader, IsUrlRequired</returns>
        public static Tuple<string, string, string, bool> GetContributionTypeRequirements(this ContributionTypeModel contributionType)
        {
            var annualQuantityHeader = "";
            var secondAnnualQuantityHeader = "";
            var annualReachHeader = "";
            bool isUrlRequired = false;

            var guidString = contributionType.Id.ToString();

            switch (guidString)
            {
                case "e36464de-179a-e411-bbc8-6c3be5a82b68": // "EnglishName": "Article"
                    annualQuantityHeader = "Number of Articles";
                    secondAnnualQuantityHeader = "";
                    annualReachHeader = "Number of Views";
                    break;
                case "df6464de-179a-e411-bbc8-6c3be5a82b68": // "EnglishName": "Blog/Website Post"
                    annualQuantityHeader = "Number of Posts";
                    secondAnnualQuantityHeader = "Number of Subscribers";
                    annualReachHeader = "Annual Unique Visitors";
                    isUrlRequired = true;
                    break;
                case "db6464de-179a-e411-bbc8-6c3be5a82b68": // "EnglishName": "Book (Author)"
                    annualQuantityHeader = "Number of Books";
                    secondAnnualQuantityHeader = "";
                    annualReachHeader = "Copies Sold";
                    break;
                case "dd6464de-179a-e411-bbc8-6c3be5a82b68": // "EnglishName": "Book (Co-Author)"
                    annualQuantityHeader = "Number of Books";
                    secondAnnualQuantityHeader = "";
                    annualReachHeader = "Copies Sold";
                    break;
                case "f16464de-179a-e411-bbc8-6c3be5a82b68": // "EnglishName": "Conference (Staffing)"
                    annualQuantityHeader = "Number of Conferences";
                    secondAnnualQuantityHeader = "";
                    annualReachHeader = "Number of Visitors";
                    break;
                case "0ce0dc15-0304-e911-8171-3863bb2bca60": // "EnglishName": "Docs.Microsoft.com Contribution"
                    annualQuantityHeader = "Pull Requests/Issues/Submissions";
                    secondAnnualQuantityHeader = "";
                    annualReachHeader = "";
                    break;
                case "f96464de-179a-e411-bbc8-6c3be5a82b68": // "EnglishName": "Forum Moderator"
                    annualQuantityHeader = "Number of Threads moderated";
                    secondAnnualQuantityHeader = "";
                    annualReachHeader = "";
                    break;
                case "d96464de-179a-e411-bbc8-6c3be5a82b68": // "EnglishName": "Forum Participation (3rd Party forums)"
                    annualQuantityHeader = "Number of Answers";
                    secondAnnualQuantityHeader = "Number of Posts";
                    annualReachHeader = "Views of Answers";
                    isUrlRequired = true;
                    break;
                case "d76464de-179a-e411-bbc8-6c3be5a82b68": // "EnglishName": "Forum Participation (Microsoft Forums)"
                    annualQuantityHeader = "Number of Answers";
                    secondAnnualQuantityHeader = "Number of Posts";
                    annualReachHeader = "Views of Answers";
                    isUrlRequired = true;
                    break;
                case "f76464de-179a-e411-bbc8-6c3be5a82b68": // "EnglishName": "Mentorship"
                    annualQuantityHeader = "Number of Mentorship Activity";
                    secondAnnualQuantityHeader = "";
                    annualReachHeader = "Number of Mentees";
                    break;
                case "d2d96407-0304-e911-8171-3863bb2bca60": // "EnglishName": "Microsoft Open Source Projects"
                    annualQuantityHeader = "Number of Projects";
                    secondAnnualQuantityHeader = "";
                    annualReachHeader = "";
                    break;
                case "414bcf30-e889-e511-8110-c4346bac0abc": // "EnglishName": "Non-Microsoft Open Source Projects"
                    annualQuantityHeader = "Project(s)";
                    secondAnnualQuantityHeader = "";
                    annualReachHeader = "Contributions";
                    break;
                case "fd6464de-179a-e411-bbc8-6c3be5a82b68": // "EnglishName": "Organizer (User Group/Meetup/Local Events)"
                    annualQuantityHeader = "Meetings";
                    secondAnnualQuantityHeader = "";
                    annualReachHeader = "Members";
                    break;
                case "ef6464de-179a-e411-bbc8-6c3be5a82b68": // "EnglishName": "Organizer of Conference"
                    annualQuantityHeader = "Number of Conferences";
                    secondAnnualQuantityHeader = "";
                    annualReachHeader = "Number of Attendees";
                    break;
                case "ff6464de-179a-e411-bbc8-6c3be5a82b68": // "EnglishName": "Other"
                    annualQuantityHeader = "Annual Quantity";
                    secondAnnualQuantityHeader = "";
                    annualReachHeader = "Annual Reach";
                    break;
                case "016564de-179a-e411-bbc8-6c3be5a82b68": // "EnglishName": "Product Group Feedback (General)"
                    annualQuantityHeader = "Number of Events Participated";
                    secondAnnualQuantityHeader = "";
                    annualReachHeader = "Number of Feedbacks Provided";
                    break;
                case "e96464de-179a-e411-bbc8-6c3be5a82b68": // "EnglishName": "Sample Code/Projects/Tools"
                    annualQuantityHeader = "Number of Samples";
                    secondAnnualQuantityHeader = "";
                    annualReachHeader = "Number of Downloads";
                    isUrlRequired = true;
                    break;
                case "fb6464de-179a-e411-bbc8-6c3be5a82b68": // "EnglishName": "Site Owner"
                    annualQuantityHeader = "Number of Sites";
                    secondAnnualQuantityHeader = "";
                    annualReachHeader = "Number of Visitors";
                    break;
                case "d16464de-179a-e411-bbc8-6c3be5a82b68": // "EnglishName": "Speaking (Conference)"
                    annualQuantityHeader = "Number of Talks";
                    secondAnnualQuantityHeader = "";
                    annualReachHeader = "Attendees of Talks";
                    break;
                case "d56464de-179a-e411-bbc8-6c3be5a82b68": // "EnglishName": "Speaking (User Group/Meetup/Local events)"
                    annualQuantityHeader = "Number of Talks";
                    secondAnnualQuantityHeader = "";
                    annualReachHeader = "Attendees of Talks";
                    break;
                case "eb6464de-179a-e411-bbc8-6c3be5a82b68": // "EnglishName": "Technical Social Media (Twitter, Facebook, LinkedIn...)"
                    annualQuantityHeader = "Number of Talks";
                    secondAnnualQuantityHeader = "";
                    annualReachHeader = "Number of Followers";
                    isUrlRequired = true;
                    break;
                case "056564de-179a-e411-bbc8-6c3be5a82b68": // "EnglishName": "Translation Review, Feedback and Editing"
                    annualQuantityHeader = "Annual Quantity";
                    secondAnnualQuantityHeader = "";
                    annualReachHeader = "";
                    break;
                case "e56464de-179a-e411-bbc8-6c3be5a82b68": // "EnglishName": "Video/Webcast/Podcast"
                    annualQuantityHeader = "Number of Videos";
                    secondAnnualQuantityHeader = "";
                    annualReachHeader = "Number of Views";
                    isUrlRequired = true;
                    break;
                case "0ee0dc15-0304-e911-8171-3863bb2bca60": // "EnglishName": "Workshop/Volunteer/Proctor"
                    annualQuantityHeader = "Number of Events";
                    secondAnnualQuantityHeader = "";
                    annualReachHeader = "";
                    break;
            }

            return new Tuple<string, string, string, bool>(annualQuantityHeader, secondAnnualQuantityHeader, annualReachHeader, isUrlRequired);
        }

        /// <summary>
        /// Deep clones the ContributionsModel (no reference to original)
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static ContributionsModel Clone(this ContributionsModel source)
        {
            // Deep clone using json.net
            var json = JsonConvert.SerializeObject(source);
            return JsonConvert.DeserializeObject<ContributionsModel>(json);
        }

        /// <summary>
        /// Helper to remove items from a collection without collection modification issues
        /// </summary>
        /// <typeparam name="TContributionsModel"></typeparam>
        /// <param name="coll">Collection of Contributions</param>
        /// <param name="condition">Predicate</param>
        /// <returns></returns>
        public static int Remove<TContributionsModel>(this ObservableCollection<TContributionsModel> coll, Func<TContributionsModel, bool> condition)
        {
            var itemsToRemove = coll.Where(condition).ToList();

            foreach (var itemToRemove in itemsToRemove)
            {
                coll.Remove(itemToRemove);
            }

            return itemsToRemove.Count;
        }

        public static bool Compare(this ContributionsModel original, ContributionsModel selected)
        {
            try
            {
                if (original == null || selected == null)
                    return false;

                if (selected.Title != original.Title)
                    return false;

                if (selected.Description != original.Description)
                    return false;

                if (selected.ReferenceUrl != original.ReferenceUrl)
                    return false;

                if (selected.ContributionTechnology.Id != original.ContributionTechnology.Id)
                    return false;

                if (selected.StartDate.Value.Date != original.StartDate.Value.Date)
                    return false;

                if (selected.AnnualQuantity != original.AnnualQuantity)
                    return false;

                if (selected.SecondAnnualQuantity != original.SecondAnnualQuantity)
                    return false;

                if (selected.AnnualReach != original.AnnualReach)
                    return false;
            }
            catch
            {
                return false;
            }

            // If all comparisons pass, true to signify a perfect match
            return true;
        }
    }
}