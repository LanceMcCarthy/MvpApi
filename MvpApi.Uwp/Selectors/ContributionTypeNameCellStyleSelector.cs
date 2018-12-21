using System.Diagnostics;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using MvpApi.Common.Models;
using MvpApi.Uwp.Common;
using Telerik.UI.Xaml.Controls.Grid;

namespace MvpApi.Uwp.Selectors
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
                    var background = GetBackgroundForContributionType(contributionModel.ContributionTypeName);
                    
                    // Add the style Setter
                    ColoredBackgroundStyle.Setters.Add(new Setter { Target = new TargetPropertyPath(Rectangle.FillProperty), Value = background });

                    return ColoredBackgroundStyle;
                }
            }

            return NormalStyle;
        }

        private SolidColorBrush GetBackgroundForContributionType(string englishName)
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

                // Names found in testing but not in API call for contribution types
                case "Product Group Interaction (PGI)":
                    return new SolidColorBrush(DesignConstants.ContributionTypeColors[1]);
                default:
                    return new SolidColorBrush(Colors.Black);
            }
        }
    }

    //public class ContributionTypeNameCellStyleSelector : StyleSelector
    //{
    //    public Style NormalStyle { get; set; }
    //    public Style ColoredBackgroundStyle { get; set; }

    //    protected override Style SelectStyleCore(object item, DependencyObject container)
    //    {
    //        if (item is DataGridCellInfo cellInfo)
    //        {
    //            if (cellInfo.Item is ContributionsModel contributionModel)
    //            {
    //                Debug.WriteLine($"ContributionTypeName: {contributionModel.ContributionTypeName}");
    //                // Get the color for that specific type name
    //                var background = GetBackgroundForContributionType(contributionModel.ContributionTypeName);

    //                // Add the style Setter
    //                ColoredBackgroundStyle.Setters.Add(new Setter { Property = Shape.FillProperty, Value = background });

    //                return ColoredBackgroundStyle;
    //            }
    //        }

    //        return NormalStyle;
    //    }

    //    private SolidColorBrush GetBackgroundForContributionType(string englishName)
    //    {
    //        switch (englishName)
    //        {
    //            case "Other":
    //                return new SolidColorBrush(DesignConstants.ContributionTypeColors[0]);
    //            case "Product Group Feedback (General)":
    //                return new SolidColorBrush(DesignConstants.ContributionTypeColors[1]);
    //            case "Site Owner":
    //                return new SolidColorBrush(DesignConstants.ContributionTypeColors[2]);
    //            case "Book (Co-Author)":
    //                return new SolidColorBrush(DesignConstants.ContributionTypeColors[3]);
    //            case "Book (Author)":
    //                return new SolidColorBrush(DesignConstants.ContributionTypeColors[4]);
    //            case "Speaking (Conference)":
    //                return new SolidColorBrush(DesignConstants.ContributionTypeColors[5]);
    //            case "Speaking (Local)":
    //                return new SolidColorBrush(DesignConstants.ContributionTypeColors[6]);
    //            case "Speaking (User group)":
    //                return new SolidColorBrush(DesignConstants.ContributionTypeColors[7]);
    //            case "Technical Social Media (Twitter, Facebook, LinkedIn...)":
    //                return new SolidColorBrush(DesignConstants.ContributionTypeColors[8]);
    //            case "User Group Owner":
    //                return new SolidColorBrush(DesignConstants.ContributionTypeColors[9]);
    //            case "Forum Moderator":
    //                return new SolidColorBrush(DesignConstants.ContributionTypeColors[10]);
    //            case "Conference (organizer)":
    //                return new SolidColorBrush(DesignConstants.ContributionTypeColors[11]);
    //            case "Conference (booth presenter)":
    //                return new SolidColorBrush(DesignConstants.ContributionTypeColors[12]);
    //            case "Sample Project/Tools":
    //                return new SolidColorBrush(DesignConstants.ContributionTypeColors[13]);
    //            case "Blog Site Posts":
    //                return new SolidColorBrush(DesignConstants.ContributionTypeColors[14]);
    //            case "Article":
    //                return new SolidColorBrush(DesignConstants.ContributionTypeColors[15]);
    //            case "Open Source Project(s)":
    //                return new SolidColorBrush(DesignConstants.ContributionTypeColors[16]);
    //            case "Mentorship":
    //                return new SolidColorBrush(DesignConstants.ContributionTypeColors[17]);
    //            case "Sample Code":
    //                return new SolidColorBrush(DesignConstants.ContributionTypeColors[19]);
    //            case "Forum Participation (3rd Party forums)":
    //                return new SolidColorBrush(DesignConstants.ContributionTypeColors[18]);
    //            case "Forum Participation (Microsoft Forums)":
    //                return new SolidColorBrush(DesignConstants.ContributionTypeColors[20]);
    //            case "Translation Review, Feedback and Editing":
    //                return new SolidColorBrush(DesignConstants.ContributionTypeColors[0]);
    //            case "Video":
    //                return new SolidColorBrush(DesignConstants.ContributionTypeColors[14]);
    //            case "Webcast":
    //                return new SolidColorBrush(DesignConstants.ContributionTypeColors[14]);
    //            case "WebSite Posts":
    //                return new SolidColorBrush(DesignConstants.ContributionTypeColors[15]);

    //            // Names found in testing but not in API call for contribution types
    //            case "Product Group Interaction (PGI)":
    //                return new SolidColorBrush(DesignConstants.ContributionTypeColors[1]);
    //            default:
    //                return new SolidColorBrush(Colors.Black);
    //        }
    //    }
    //}
}
