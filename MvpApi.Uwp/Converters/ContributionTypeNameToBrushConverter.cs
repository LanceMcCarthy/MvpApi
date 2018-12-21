using System;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using MvpApi.Uwp.Common;

namespace MvpApi.Uwp.Converters
{
    internal class ContributionTypeNameToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string englishName)
            {
                switch (englishName)
                {
                    case "Other":
                        return new SolidColorBrush(DesignConstants.ContributionTypeColors[0]);
                    case "Product Group Feedback (General)":
                        return new SolidColorBrush(DesignConstants.ContributionTypeColors[1]);
                    case "Site Owner":
                        return new SolidColorBrush(DesignConstants.ContributionTypeColors[2]);
                    case "Book (Co-Author)":
                        return new SolidColorBrush(DesignConstants.ContributionTypeColors[3]);
                    case "Book (Author)":
                        return new SolidColorBrush(DesignConstants.ContributionTypeColors[4]);
                    case "Speaking (Conference)":
                        return new SolidColorBrush(DesignConstants.ContributionTypeColors[5]);
                    case "Speaking (Local)":
                        return new SolidColorBrush(DesignConstants.ContributionTypeColors[6]);
                    case "Speaking (User group)":
                        return new SolidColorBrush(DesignConstants.ContributionTypeColors[7]);
                    case "Technical Social Media (Twitter, Facebook, LinkedIn...)":
                        return new SolidColorBrush(DesignConstants.ContributionTypeColors[8]);
                    case "User Group Owner":
                        return new SolidColorBrush(DesignConstants.ContributionTypeColors[9]);
                    case "Forum Moderator":
                        return new SolidColorBrush(DesignConstants.ContributionTypeColors[10]);
                    case "Conference (organizer)":
                        return new SolidColorBrush(DesignConstants.ContributionTypeColors[11]);
                    case "Conference (booth presenter)":
                        return new SolidColorBrush(DesignConstants.ContributionTypeColors[12]);
                    case "Sample Project/Tools":
                        return new SolidColorBrush(DesignConstants.ContributionTypeColors[13]);
                    case "Blog Site Posts":
                        return new SolidColorBrush(DesignConstants.ContributionTypeColors[14]);
                    case "Article":
                        return new SolidColorBrush(DesignConstants.ContributionTypeColors[15]);
                    case "Open Source Project(s)":
                        return new SolidColorBrush(DesignConstants.ContributionTypeColors[16]);
                    case "Mentorship":
                        return new SolidColorBrush(DesignConstants.ContributionTypeColors[17]);
                    case "Sample Code":
                        return new SolidColorBrush(DesignConstants.ContributionTypeColors[19]);
                    case "Forum Participation (3rd Party forums)":
                        return new SolidColorBrush(DesignConstants.ContributionTypeColors[18]);
                    case "Forum Participation (Microsoft Forums)":
                        return new SolidColorBrush(DesignConstants.ContributionTypeColors[20]);
                    case "Translation Review, Feedback and Editing":
                        return new SolidColorBrush(DesignConstants.ContributionTypeColors[0]);
                    case "Video":
                        return new SolidColorBrush(DesignConstants.ContributionTypeColors[14]);
                    case "Webcast":
                        return new SolidColorBrush(DesignConstants.ContributionTypeColors[14]);
                    case "WebSite Posts":
                        return new SolidColorBrush(DesignConstants.ContributionTypeColors[15]);
                    case "Product Group Interaction (PGI)":
                        return new SolidColorBrush(DesignConstants.ContributionTypeColors[1]);
                    default:
                        return new SolidColorBrush(Colors.Black);
                }
            }

            return new SolidColorBrush(Colors.Black);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
