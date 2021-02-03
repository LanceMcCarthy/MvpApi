using System;
using Windows.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using MvpCompanion.UI.Helpers;

namespace MvpCompanion.UI.Converters
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

            return IsForegroundColor ? new SolidColorBrush(Colors.Black) : new SolidColorBrush(Colors.Transparent);
        }

        private static SolidColorBrush GetForegroundColor(string contributionTypeName)
        {
            switch (contributionTypeName)
            {
                case "Article": // e36464de-179a-e411-bbc8-6c3be5a82b68
                    return new SolidColorBrush(PaletteHelper.ContributionTypeForegroundColors[15]);
                case "Blog/Website Post": // df6464de-179a-e411-bbc8-6c3be5a82b68
                    return new SolidColorBrush(PaletteHelper.ContributionTypeForegroundColors[14]);
                case "Book (Author)": // db6464de-179a-e411-bbc8-6c3be5a82b68
                    return new SolidColorBrush(PaletteHelper.ContributionTypeForegroundColors[4]);
                case "Book (Co-Author)": // dd6464de-179a-e411-bbc8-6c3be5a82b68
                    return new SolidColorBrush(PaletteHelper.ContributionTypeForegroundColors[3]);
                case "Conference (Staffing)": // f16464de-179a-e411-bbc8-6c3be5a82b68
                    return new SolidColorBrush(PaletteHelper.ContributionTypeForegroundColors[12]);
                case "Docs.Microsoft.com Contribution": // "0ce0dc15-0304-e911-8171-3863bb2bca60
                    return new SolidColorBrush(PaletteHelper.ContributionTypeForegroundColors[7]);
                case "Forum Moderator": // f96464de-179a-e411-bbc8-6c3be5a82b68
                    return new SolidColorBrush(PaletteHelper.ContributionTypeForegroundColors[10]);
                case "Forum Participation (3rd Party forums)": // d96464de-179a-e411-bbc8-6c3be5a82b68
                    return new SolidColorBrush(PaletteHelper.ContributionTypeForegroundColors[18]);
                case "Forum Participation (Microsoft Forums)": // d76464de-179a-e411-bbc8-6c3be5a82b68
                    return new SolidColorBrush(PaletteHelper.ContributionTypeForegroundColors[20]);
                case "Mentorship": // f76464de-179a-e411-bbc8-6c3be5a82b68
                    return new SolidColorBrush(PaletteHelper.ContributionTypeForegroundColors[17]);
                case "Microsoft Open Source Projects": // d2d96407-0304-e911-8171-3863bb2bca60
                    return new SolidColorBrush(PaletteHelper.ContributionTypeForegroundColors[16]);
                case "Non-Microsoft Open Source Projects": // 414bcf30-e889-e511-8110-c4346bac0abc
                    return new SolidColorBrush(PaletteHelper.ContributionTypeForegroundColors[19]);
                case "Organizer (User Group/Meetup/Local Events)": // fd6464de-179a-e411-bbc8-6c3be5a82b68
                    return new SolidColorBrush(PaletteHelper.ContributionTypeForegroundColors[9]);
                case "Organizer of Conference": // ef6464de-179a-e411-bbc8-6c3be5a82b68
                    return new SolidColorBrush(PaletteHelper.ContributionTypeForegroundColors[11]);
                case "Other": // ff6464de-179a-e411-bbc8-6c3be5a82b68
                    return new SolidColorBrush(PaletteHelper.ContributionTypeForegroundColors[0]);
                case "Product Group Interaction (PGI)":
                    return new SolidColorBrush(PaletteHelper.ContributionTypeForegroundColors[1]);
                case "Product Group Feedback (General)": //016564de-179a-e411-bbc8-6c3be5a82b68
                    return new SolidColorBrush(PaletteHelper.ContributionTypeForegroundColors[1]);
                case "Sample Code/Projects/Tools": // e96464de-179a-e411-bbc8-6c3be5a82b68
                    return new SolidColorBrush(PaletteHelper.ContributionTypeForegroundColors[13]);
                case "Site Owner": // fb6464de-179a-e411-bbc8-6c3be5a82b68
                    return new SolidColorBrush(PaletteHelper.ContributionTypeForegroundColors[2]);
                case "Speaking (Conference)": // d16464de-179a-e411-bbc8-6c3be5a82b68
                    return new SolidColorBrush(PaletteHelper.ContributionTypeForegroundColors[5]);
                case "Speaking (User Group/Meetup/Local events)": // d56464de-179a-e411-bbc8-6c3be5a82b68
                    return new SolidColorBrush(PaletteHelper.ContributionTypeForegroundColors[6]);
                case "Technical Social Media (Twitter, Facebook, LinkedIn...)": // eb6464de-179a-e411-bbc8-6c3be5a82b68
                    return new SolidColorBrush(PaletteHelper.ContributionTypeForegroundColors[8]);
                case "Translation Review, Feedback and Editing": // 056564de-179a-e411-bbc8-6c3be5a82b68
                    return new SolidColorBrush(PaletteHelper.ContributionTypeForegroundColors[0]);
                case "Video/Webcast/Podcast": // e56464de-179a-e411-bbc8-6c3be5a82b68
                    return new SolidColorBrush(PaletteHelper.ContributionTypeForegroundColors[14]);
                case "Workshop/Volunteer/Proctor": // 0ee0dc15-0304-e911-8171-3863bb2bca60
                    return new SolidColorBrush(PaletteHelper.ContributionTypeForegroundColors[14]);
                default:
                    return new SolidColorBrush(Colors.Black);
            }
        }

        private static SolidColorBrush GetBackgroundColor(string contributionTypeName)
        {
            switch (contributionTypeName)
            {
                case "Article": // e36464de-179a-e411-bbc8-6c3be5a82b68
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[15]);
                case "Blog/Website Post": // df6464de-179a-e411-bbc8-6c3be5a82b68
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[14]);
                case "Book (Author)": // db6464de-179a-e411-bbc8-6c3be5a82b68
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[4]);
                case "Book (Co-Author)": // dd6464de-179a-e411-bbc8-6c3be5a82b68
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[3]);
                case "Conference (Staffing)": // f16464de-179a-e411-bbc8-6c3be5a82b68
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[12]);
                case "Docs.Microsoft.com Contribution": // "0ce0dc15-0304-e911-8171-3863bb2bca60
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[7]);
                case "Forum Moderator": // f96464de-179a-e411-bbc8-6c3be5a82b68
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[10]);
                case "Forum Participation (3rd Party forums)": // d96464de-179a-e411-bbc8-6c3be5a82b68
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[18]);
                case "Forum Participation (Microsoft Forums)": // d76464de-179a-e411-bbc8-6c3be5a82b68
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[20]);
                case "Mentorship": // f76464de-179a-e411-bbc8-6c3be5a82b68
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[17]);
                case "Microsoft Open Source Projects": // d2d96407-0304-e911-8171-3863bb2bca60
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[16]);
                case "Non-Microsoft Open Source Projects": // 414bcf30-e889-e511-8110-c4346bac0abc
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[19]);
                case "Organizer (User Group/Meetup/Local Events)": // fd6464de-179a-e411-bbc8-6c3be5a82b68
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[9]);
                case "Organizer of Conference": // ef6464de-179a-e411-bbc8-6c3be5a82b68
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[11]);
                case "Other": // ff6464de-179a-e411-bbc8-6c3be5a82b68
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[0]);
                case "Product Group Interaction (PGI)":
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[1]);
                case "Product Group Feedback (General)": //016564de-179a-e411-bbc8-6c3be5a82b68
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[1]);
                case "Sample Code/Projects/Tools": // e96464de-179a-e411-bbc8-6c3be5a82b68
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[13]);
                case "Site Owner": // fb6464de-179a-e411-bbc8-6c3be5a82b68
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[2]);
                case "Speaking (Conference)": // d16464de-179a-e411-bbc8-6c3be5a82b68
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[5]);
                case "Speaking (User Group/Meetup/Local events)": // d56464de-179a-e411-bbc8-6c3be5a82b68
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[6]);
                case "Technical Social Media (Twitter, Facebook, LinkedIn...)": // eb6464de-179a-e411-bbc8-6c3be5a82b68
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[8]);
                case "Translation Review, Feedback and Editing": // 056564de-179a-e411-bbc8-6c3be5a82b68
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[0]);
                case "Video/Webcast/Podcast": // e56464de-179a-e411-bbc8-6c3be5a82b68
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[14]);
                case "Workshop/Volunteer/Proctor": // 0ee0dc15-0304-e911-8171-3863bb2bca60
                    return new SolidColorBrush(PaletteHelper.ContributionTypeBackgroundColors[14]);
                default:
                    return new SolidColorBrush(Colors.LightGray);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
