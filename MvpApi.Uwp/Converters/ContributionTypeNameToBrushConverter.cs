using System;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using MvpApi.Uwp.Helpers;

namespace MvpApi.Uwp.Converters
{
    internal class ContributionTypeNameToBrushConverter : IValueConverter
    {
        public bool IsForegroundColor { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string englishName)
            {
                return IsForegroundColor ? GetForegroundColor(englishName) : GetBackgroundColor(englishName);
            }

            return new SolidColorBrush(Colors.Black);
        }

        private static SolidColorBrush GetForegroundColor(string contributionTypeName)
        {
            switch (contributionTypeName)
            {
                case "Other":
                    return new SolidColorBrush(PaletteHelper.ContributionTypeForegroundColors[0]);
                case "Product Group Feedback (General)":
                    return new SolidColorBrush(PaletteHelper.ContributionTypeForegroundColors[1]);
                case "Site Owner":
                    return new SolidColorBrush(PaletteHelper.ContributionTypeForegroundColors[2]);
                case "Book (Co-Author)":
                    return new SolidColorBrush(PaletteHelper.ContributionTypeForegroundColors[3]);
                case "Book (Author)":
                    return new SolidColorBrush(PaletteHelper.ContributionTypeForegroundColors[4]);
                case "Speaking (Conference)":
                    return new SolidColorBrush(PaletteHelper.ContributionTypeForegroundColors[5]);
                case "Speaking (Local)":
                    return new SolidColorBrush(PaletteHelper.ContributionTypeForegroundColors[6]);
                case "Speaking (User group)":
                    return new SolidColorBrush(PaletteHelper.ContributionTypeForegroundColors[7]);
                case "Technical Social Media (Twitter, Facebook, LinkedIn...)":
                    return new SolidColorBrush(PaletteHelper.ContributionTypeForegroundColors[8]);
                case "User Group Owner":
                    return new SolidColorBrush(PaletteHelper.ContributionTypeForegroundColors[9]);
                case "Forum Moderator":
                    return new SolidColorBrush(PaletteHelper.ContributionTypeForegroundColors[10]);
                case "Conference (organizer)":
                    return new SolidColorBrush(PaletteHelper.ContributionTypeForegroundColors[11]);
                case "Conference (booth presenter)":
                    return new SolidColorBrush(PaletteHelper.ContributionTypeForegroundColors[12]);
                case "Sample Project/Tools":
                    return new SolidColorBrush(PaletteHelper.ContributionTypeForegroundColors[13]);
                case "Blog Site Posts":
                    return new SolidColorBrush(PaletteHelper.ContributionTypeForegroundColors[14]);
                case "Article":
                    return new SolidColorBrush(PaletteHelper.ContributionTypeForegroundColors[15]);
                case "Open Source Project(s)":
                    return new SolidColorBrush(PaletteHelper.ContributionTypeForegroundColors[16]);
                case "Mentorship":
                    return new SolidColorBrush(PaletteHelper.ContributionTypeForegroundColors[17]);
                case "Sample Code":
                    return new SolidColorBrush(PaletteHelper.ContributionTypeForegroundColors[19]);
                case "Forum Participation (3rd Party forums)":
                    return new SolidColorBrush(PaletteHelper.ContributionTypeForegroundColors[18]);
                case "Forum Participation (Microsoft Forums)":
                    return new SolidColorBrush(PaletteHelper.ContributionTypeForegroundColors[20]);
                case "Translation Review, Feedback and Editing":
                    return new SolidColorBrush(PaletteHelper.ContributionTypeForegroundColors[0]);
                case "Video":
                    return new SolidColorBrush(PaletteHelper.ContributionTypeForegroundColors[14]);
                case "Webcast":
                    return new SolidColorBrush(PaletteHelper.ContributionTypeForegroundColors[14]);
                case "WebSite Posts":
                    return new SolidColorBrush(PaletteHelper.ContributionTypeForegroundColors[15]);
                case "Product Group Interaction (PGI)":
                    return new SolidColorBrush(PaletteHelper.ContributionTypeForegroundColors[1]);
                default:
                    return new SolidColorBrush(Colors.Black);
            }
        }

        private static SolidColorBrush GetBackgroundColor(string contributionTypeName)
        {
            switch (contributionTypeName)
            {
                case "Other":
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[0]);
                case "Product Group Feedback (General)":
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[1]);
                case "Site Owner":
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[2]);
                case "Book (Co-Author)":
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[3]);
                case "Book (Author)":
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[4]);
                case "Speaking (Conference)":
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[5]);
                case "Speaking (Local)":
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[6]);
                case "Speaking (User group)":
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[7]);
                case "Technical Social Media (Twitter, Facebook, LinkedIn...)":
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[8]);
                case "User Group Owner":
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[9]);
                case "Forum Moderator":
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[10]);
                case "Conference (organizer)":
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[11]);
                case "Conference (booth presenter)":
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[12]);
                case "Sample Project/Tools":
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[13]);
                case "Blog Site Posts":
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[14]);
                case "Article":
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[15]);
                case "Open Source Project(s)":
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[16]);
                case "Mentorship":
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[17]);
                case "Sample Code":
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[19]);
                case "Forum Participation (3rd Party forums)":
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[18]);
                case "Forum Participation (Microsoft Forums)":
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[20]);
                case "Translation Review, Feedback and Editing":
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[0]);
                case "Video":
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[14]);
                case "Webcast":
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[14]);
                case "WebSite Posts":
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[15]);
                case "Product Group Interaction (PGI)":
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[1]);
                default:
                    return new SolidColorBrush(Colors.Black);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
