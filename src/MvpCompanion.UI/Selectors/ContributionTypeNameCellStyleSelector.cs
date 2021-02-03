using System.Diagnostics;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using MvpApi.Common.Models;
using MvpCompanion.UI.Helpers;
using Telerik.UI.Xaml.Controls.Grid;

namespace MvpCompanion.UI.Selectors
{
    public class ContributionTypeNameCellStyleSelector : StyleSelector
    {
        public Style NormalStyle { get; set; }

        public Style ColoredBackgroundStyle { get; set; }
        
        protected override Style SelectStyleCore(object item, DependencyObject container)
        {
            if (item is DataGridCellInfo cellInfo)
            {
                if (cellInfo.Item is ContributionsModel contributionModel)
                {
                    Debug.WriteLine($"ContributionTypeName: {contributionModel.ContributionTypeName}");
                    // Get the color for that specific type name
                    var background = GetBackgroundForContributionType(contributionModel.ContributionType);
                    
                    // Add the style Setter
                    ColoredBackgroundStyle.Setters.Add(new Setter { Target = new TargetPropertyPath(Rectangle.FillProperty), Value = background });

                    return ColoredBackgroundStyle;
                }
            }

            return NormalStyle;
        }

        private SolidColorBrush GetBackgroundForContributionType(ContributionTypeModel contributionType)
        {
            // Special cases that don't have a GUId from the API
            if (contributionType.EnglishName == "Product Group Interaction (PGI)")
            {
                return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[1]);
            }

            var guidString = contributionType.Id.ToString();

            switch (guidString)
            {
                case "e36464de-179a-e411-bbc8-6c3be5a82b68": // "EnglishName": "Article"
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[15]);
                case "df6464de-179a-e411-bbc8-6c3be5a82b68": // "EnglishName": "Blog/Website Post"
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[14]);
                case "db6464de-179a-e411-bbc8-6c3be5a82b68": // "EnglishName": "Book (Author)"
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[4]);
                case "dd6464de-179a-e411-bbc8-6c3be5a82b68": // "EnglishName": "Book (Co-Author)"
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[3]);
                case "f16464de-179a-e411-bbc8-6c3be5a82b68": // "EnglishName": "Conference (Staffing)"
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[12]);
                case "0ce0dc15-0304-e911-8171-3863bb2bca60": // "EnglishName": "Docs.Microsoft.com Contribution"
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[7]);
                case "f96464de-179a-e411-bbc8-6c3be5a82b68": // "EnglishName": "Forum Moderator"
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[10]);
                case "d96464de-179a-e411-bbc8-6c3be5a82b68": // "EnglishName": "Forum Participation (3rd Party forums)"
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[18]);
                case "d76464de-179a-e411-bbc8-6c3be5a82b68": // "EnglishName": "Forum Participation (Microsoft Forums)"
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[20]);
                case "f76464de-179a-e411-bbc8-6c3be5a82b68": // "EnglishName": "Mentorship"
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[17]);
                case "d2d96407-0304-e911-8171-3863bb2bca60": // "EnglishName": "Microsoft Open Source Projects"
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[16]);
                case "414bcf30-e889-e511-8110-c4346bac0abc": // "EnglishName": "Non-Microsoft Open Source Projects"
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[19]);
                case "fd6464de-179a-e411-bbc8-6c3be5a82b68": // "EnglishName": "Organizer (User Group/Meetup/Local Events)"
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[9]);
                case "ef6464de-179a-e411-bbc8-6c3be5a82b68": // "EnglishName": "Organizer of Conference"
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[11]);
                case "ff6464de-179a-e411-bbc8-6c3be5a82b68": // "EnglishName": "Other"
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[0]);
                case "016564de-179a-e411-bbc8-6c3be5a82b68": // "EnglishName": "Product Group Feedback (General)"
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[1]);
                case "e96464de-179a-e411-bbc8-6c3be5a82b68": // "EnglishName": "Sample Code/Projects/Tools"
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[13]);
                case "fb6464de-179a-e411-bbc8-6c3be5a82b68": // "EnglishName": "Site Owner"
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[2]);
                case "d16464de-179a-e411-bbc8-6c3be5a82b68": // "EnglishName": "Speaking (Conference)"
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[5]);
                case "d56464de-179a-e411-bbc8-6c3be5a82b68": // "EnglishName": "Speaking (User Group/Meetup/Local events)"
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[6]);
                case "eb6464de-179a-e411-bbc8-6c3be5a82b68": // "EnglishName": "Technical Social Media (Twitter, Facebook, LinkedIn...)"
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[8]);
                case "056564de-179a-e411-bbc8-6c3be5a82b68": // "EnglishName": "Translation Review, Feedback and Editing"
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[0]);
                case "e56464de-179a-e411-bbc8-6c3be5a82b68": // "EnglishName": "Video/Webcast/Podcast"
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[14]);
                case "0ee0dc15-0304-e911-8171-3863bb2bca60": // "EnglishName": "Workshop/Volunteer/Proctor"
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[14]);
                default:
                    return new SolidColorBrush(Colors.Black);
            }
        }
    }
}
