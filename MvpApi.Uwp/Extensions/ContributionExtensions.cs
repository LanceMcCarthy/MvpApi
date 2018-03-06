using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Popups;
using MvpApi.Common.Models;
using Newtonsoft.Json;

namespace MvpApi.Uwp.Extensions
{
    public static class ContributionExtensions
    {
        /// <summary>
        /// Validates the values of a ContributionsModel instance. 
        /// </summary>
        /// <param name="contribution">The contribution to be validated</param>
        /// <param name="showErrorMessage">show the user an error message with the field name of the missing value. 
        /// The default is False (quick validation mode). Set to True when you're about to save the contribution.
        /// </param>
        /// <returns>Boolean that denotes a successful validation</returns>
        public static async Task<bool> Validate(this ContributionsModel contribution, bool showErrorMessage = false)
        {
            if(contribution == null)
                throw new NullReferenceException("The contribution was null");

            bool isValid = true;
            string failedFieldName = "";
            
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
            
            // *** ContributionType Sepcific Condition ***

            // Gets a Tuple that contains the specific conditions of a chosen ContributionType
            var typeRequirements = contribution.ContributionType.GetContributionTypeRequirements();

            // Check Url
            if (typeRequirements.Item1 && string.IsNullOrEmpty(contribution.ReferenceUrl))
            {
                failedFieldName = "url";
                isValid = false;
            }

            // Check AnnualQuantity
            if (typeRequirements.Item2 && contribution.AnnualQuantity == null)
            {
                failedFieldName = "first quantity";
                isValid = false;
            }

            // Check SecondAnnualQuantity
            if (typeRequirements.Item3 && contribution.SecondAnnualQuantity == null)
            {
                failedFieldName = "second quantity";
                isValid = false;
            }

            // *** end of ContributionType check ***

            // Check Visibility
            if (contribution.Visibility == null)
            {
                failedFieldName = "Visibility";
                isValid = false;
            }

            // If we're using this extension method for final validation and not fast validation, show error message to user
            if(!isValid && showErrorMessage)
                await new MessageDialog($"The {failedFieldName} field is a required entry for this contribution type. is a required field").ShowAsync();

            return isValid;
        }

        /// <summary>
        /// Determines which fields are required for the ContributionType
        /// </summary>
        /// <param name="contributionType"></param>
        /// <returns></returns>
        public static Tuple<bool,bool,bool> GetContributionTypeRequirements(this ContributionTypeModel contributionType)
        {
            bool isUrlRequired;
            bool isAnnualQuantityRequired;
            bool isSecondAnnualQuantityRequired;

            switch (contributionType.EnglishName)
            {
                case "Article":
                    isUrlRequired = false;
                    isAnnualQuantityRequired = true;
                    isSecondAnnualQuantityRequired = false;
                    break;
                case "Blog Site Posts":
                    isUrlRequired = true;
                    isAnnualQuantityRequired = true;
                    isSecondAnnualQuantityRequired = false;
                    break;
                case "Book (Author)":
                    isUrlRequired = false;
                    isAnnualQuantityRequired = true;
                    isSecondAnnualQuantityRequired = false;
                    break;
                case "Book (Co-Author)":
                    isUrlRequired = false;
                    isAnnualQuantityRequired = true;
                    isSecondAnnualQuantityRequired = false;
                    break;
                case "Code Project/Tools":
                    isUrlRequired = true;
                    isAnnualQuantityRequired = true;
                    isSecondAnnualQuantityRequired = false;
                    break;
                case "Code Samples":
                    isUrlRequired = true;
                    isAnnualQuantityRequired = true;
                    isSecondAnnualQuantityRequired = false;
                    break;
                case "Conference (booth presenter)":
                    isUrlRequired = false;
                    isAnnualQuantityRequired = true;
                    isSecondAnnualQuantityRequired = false;
                    break;
                case "Conference (organizer)":
                    isUrlRequired = false;
                    isAnnualQuantityRequired = true;
                    isSecondAnnualQuantityRequired = false;
                    break;
                case "Forum Moderator":
                    isUrlRequired = false;
                    isAnnualQuantityRequired = true;
                    isSecondAnnualQuantityRequired = false;
                    break;
                case "Forum Participation (3rd Party Forums)":
                    isUrlRequired = true;
                    isAnnualQuantityRequired = false;
                    isSecondAnnualQuantityRequired = true;
                    break;
                case "Forum Participation (Microsoft Forums)":
                    isUrlRequired = true;
                    isAnnualQuantityRequired = true;
                    isSecondAnnualQuantityRequired = false;
                    break;
                case "Mentorship":
                    isUrlRequired = false;
                    isAnnualQuantityRequired = true;
                    isSecondAnnualQuantityRequired = false;
                    break;
                case "Open Source Project(s)":
                    isUrlRequired = false;
                    isAnnualQuantityRequired = true;
                    isSecondAnnualQuantityRequired = false;
                    break;
                case "Other":
                    isUrlRequired = false;
                    isAnnualQuantityRequired = true;
                    isSecondAnnualQuantityRequired = false;
                    break;
                case "Product Group Feedback":
                    isUrlRequired = false;
                    isAnnualQuantityRequired = true;
                    isSecondAnnualQuantityRequired = false;
                    break;
                case "Site Owner":
                    isUrlRequired = false;
                    isAnnualQuantityRequired = true;
                    isSecondAnnualQuantityRequired = false;
                    break;
                case "Speaking (Conference)":
                    isUrlRequired = false;
                    isAnnualQuantityRequired = true;
                    isSecondAnnualQuantityRequired = false;
                    break;
                case "Speaking (Local)":
                    isUrlRequired = false;
                    isAnnualQuantityRequired = true;
                    isSecondAnnualQuantityRequired = false;
                    break;
                case "Speaking (User group)":
                    isUrlRequired = false;
                    isAnnualQuantityRequired = true;
                    isSecondAnnualQuantityRequired = false;
                    break;
                case "Technical Social Media (Twitter, Facebook, LinkedIn...)":
                    isUrlRequired = true;
                    isAnnualQuantityRequired = true;
                    isSecondAnnualQuantityRequired = false;
                    break;
                case "Translation Review, Feedback and Editing":
                    isUrlRequired = false;
                    isAnnualQuantityRequired = true;
                    isSecondAnnualQuantityRequired = false;
                    break;
                case "User Group Owner":
                    isUrlRequired = false;
                    isAnnualQuantityRequired = true;
                    isSecondAnnualQuantityRequired = false;
                    break;
                case "Video":
                    isUrlRequired = true;
                    isAnnualQuantityRequired = true;
                    isSecondAnnualQuantityRequired = false;
                    break;
                case "Webcast":
                    isUrlRequired = false;
                    isAnnualQuantityRequired = true;
                    isSecondAnnualQuantityRequired = false;
                    break;
                case "Website Posts":
                    isUrlRequired = true;
                    isAnnualQuantityRequired = true;
                    isSecondAnnualQuantityRequired = false;
                    break;
                default: 
                    isUrlRequired = false;
                    isAnnualQuantityRequired = true;
                    isSecondAnnualQuantityRequired = false;
                    break;
            }

            return new Tuple<bool, bool, bool>(isUrlRequired, isAnnualQuantityRequired, isSecondAnnualQuantityRequired);
        }

        /// <summary>
        /// Deep clones the ContributionsModel (no reference to oringal
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static ContributionsModel Clone(this ContributionsModel source)
        {
            // Deep clone using json.net
            var json = JsonConvert.SerializeObject(source);
            var clone = JsonConvert.DeserializeObject<ContributionsModel>(json);
            
            Debug.WriteLine($"Clone complete. Contribution: {clone.Title}");

            return clone;
        }
    }
}