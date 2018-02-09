using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using MvpApi.Common.Models;
using MvpApi.Uwp.Views;
using Newtonsoft.Json;

// See https://github.com/LanceMcCarthy/MvpApi/issues/8 for details on the requirements of the model value entries
namespace MvpApi.Uwp.ViewModels
{
    public class ContributionDetailViewModel : PageViewModelBase
    {
        #region Fields

        private ContributionsModel originalContribution;
        private ContributionsModel activeContribution;
        private bool isDirty;
        private ContributionAreasRootItem selectedContributionAreaAwardCategory;
        private ContributionAreaContributionModel selectedContributionAreaContributionModel;
        private ContributionAreaModel selectedContributionAreaModel;
        private bool isContributionTypeEditable = true;
        private string urlHeader = "Url";
        private string annualQuantityHeader = "Annual Quantity";
        private string secondAnnualQuantityHeader = "Second Annual Quantity";
        private string annualReachHeader = "Annual Reach";
        private bool isUrlRequired;
        private bool isAnnualQuantityRequired;
        private bool isSecondAnnualQuantityRequired;

        #endregion

        public ContributionDetailViewModel()
        {

        }

        #region Properties

        public ContributionsModel ActiveContribution
        {
            get => activeContribution;
            set => Set(ref activeContribution, value);
        }

        public ObservableCollection<ContributionTypeModel> ContributionTypes { get; set; } = new ObservableCollection<ContributionTypeModel>();

        public ObservableCollection<VisibilityViewModel> ContributionVisibilies { get; set; } = new ObservableCollection<VisibilityViewModel>();
        
        public ObservableCollection<ContributionAreasRootItem> ContributionAreaAwardCategories { get; set; } = new ObservableCollection<ContributionAreasRootItem>();

        public ContributionAreasRootItem SelectedContributionAreaAwardCategory
        {
            get => selectedContributionAreaAwardCategory;
            set => Set(ref selectedContributionAreaAwardCategory, value);
        }

        public ObservableCollection<ContributionAreaContributionModel> ContributionAreaContributions { get; set; } = new ObservableCollection<ContributionAreaContributionModel>();

        public ContributionAreaContributionModel SelectedContributionAreaContributionModel
        {
            get => selectedContributionAreaContributionModel;
            set => Set(ref selectedContributionAreaContributionModel, value);
        }

        public ContributionAreaModel SelectedContributionAreaModel
        {
            get => selectedContributionAreaModel;
            set => Set(ref selectedContributionAreaModel, value);
        }
        
        public bool IsDirty
        {
            get => isDirty;
            set => Set(ref isDirty, value);
        }

        public bool IsContributionTypeEditable
        {
            get => isContributionTypeEditable;
            set => Set(ref isContributionTypeEditable, value);
        }

        public string AnnualQuantityHeader
        {
            get => annualQuantityHeader;
            set => Set(ref annualQuantityHeader, value);
        }

        public string SecondAnnualQuantityHeader
        {
            get => secondAnnualQuantityHeader;
            set => Set(ref secondAnnualQuantityHeader, value);
        }

        public string AnnualReachHeader
        {
            get => annualReachHeader;
            set => Set(ref annualReachHeader, value);
        }

        public string UrlHeader
        {
            get => urlHeader;
            set => Set(ref urlHeader, value);
        }

        public bool IsUrlRequired
        {
            get => isUrlRequired;
            set => Set(ref isUrlRequired, value);
        }

        public bool IsAnnualQuantityRequired
        {
            get => isAnnualQuantityRequired;
            set => Set(ref isAnnualQuantityRequired, value);
        }

        public bool IsSecondAnnualQuantityRequired
        {
            get => isSecondAnnualQuantityRequired;
            set => Set(ref isSecondAnnualQuantityRequired, value);
        }
        
        #endregion

        #region Event handlers

        public void TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            // TODO check and mark IsDirty if applicable
        }

        public void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // TODO check and mark IsDirty if applicable
        }

        public void DatePicker_OnDateChanged(object sender, DatePickerValueChangedEventArgs e)
        {
            // If we're not editing an existing
            if(originalContribution == null)
                return;

            IsDirty = activeContribution.StartDate?.Date != originalContribution.StartDate?.Date;
        }

        public void RadNumericBox_OnValueChanged(object sender, EventArgs e)
        {
            // TODO check and mark IsDirty if applicable
        }

        public void ActivityType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateHeaders(ActiveContribution.ContributionType);
        }

        #endregion

        #region Methods
        
        public async void UploadContributionsButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate all required fields

            var isValidated = await ValidateRequiredFields();

            if (!isValidated)
            {
                return;
            }

            // Confirm with the user they want to save

            var md = new MessageDialog("Upload and save this contribution?");

            ContributionsModel submissionResult = null;

            md.Commands.Add(new UICommand("save", async (args) =>
            {
                // TODO see if changing int type in the model will work, they have the same properties, so it *should* work fine
                ActiveContribution.ContributionTechnology = new ContributionTechnologyModel
                {
                    Id = SelectedContributionAreaModel.Id,
                    AwardCategory = SelectedContributionAreaModel.AwardCategory,
                    AwardName = SelectedContributionAreaModel.AwardName,
                    Name = SelectedContributionAreaModel.Name
                };

                submissionResult = await App.ApiService.SubmitContributionAsync(ActiveContribution);
            }));

            md.Commands.Add(new UICommand("cancel"));

            await md.ShowAsync();

            if (submissionResult == null)
            {
                await new MessageDialog("Something went wrong saving the item, please try again").ShowAsync();
                return;
            }

            // Ask they want to start a new entry, if yes, clear the fields

            var successDialog = new MessageDialog("Would you like to start a new contribution?", "Success!");

            successDialog.Commands.Add(new UICommand("yes", args =>
            {
                ActiveContribution = new ContributionsModel
                {
                    StartDate = DateTime.Now,
                    Visibility = ContributionVisibilies.FirstOrDefault(),
                    ContributionType = ContributionTypes.FirstOrDefault(), // Note this field is READONLY when it's an existing contribution
                    ContributionTypeName = ActiveContribution?.ContributionType?.Name
                };
            }));

            successDialog.Commands.Add(new UICommand("no", args =>
            {
                ActiveContribution = submissionResult;
            }));

            await successDialog.ShowAsync();
        }
        
        private async Task<bool> ValidateRequiredFields()
        {
            if (IsUrlRequired && string.IsNullOrEmpty(ActiveContribution.ReferenceUrl))
            {
                await new MessageDialog($"The {UrlHeader} field is required when entering a {ActiveContribution.ContributionType.EnglishName} Activity Type", 
                    $"Missing {UrlHeader}!").ShowAsync();

                return false;
            }

            if (IsAnnualQuantityRequired && ActiveContribution.AnnualQuantity == null)
            {
                await new MessageDialog($"The {AnnualQuantityHeader} field is required when entering a {ActiveContribution.ContributionType.EnglishName} Activity Type", 
                    $"Missing {AnnualQuantityHeader}!").ShowAsync();

                return false;
            }

            if (IsSecondAnnualQuantityRequired && ActiveContribution.SecondAnnualQuantity == null)
            {
                await new MessageDialog($"The {SecondAnnualQuantityHeader} field is required when entering a {ActiveContribution.ContributionType.EnglishName} Activity Type", 
                    $"Missing {SecondAnnualQuantityHeader}!").ShowAsync();

                return false;
            }

            return true;
        }

        private void UpdateHeaders(ContributionTypeModel contributionType)
        {
            switch (contributionType.EnglishName)
            {
                case "Article":
                    AnnualQuantityHeader = "Number of Articles";
                    SecondAnnualQuantityHeader = "";
                    AnnualReachHeader = "Number of Views";
                    IsUrlRequired = false;
                    IsAnnualQuantityRequired = true;
                    IsSecondAnnualQuantityRequired = false;
                    break;
                case "Blog Site Posts":
                    AnnualQuantityHeader = "Number of Posts";
                    SecondAnnualQuantityHeader = "Number of Subscribers";
                    AnnualReachHeader = "Annual Unique Visitors";
                    IsUrlRequired = true;
                    IsAnnualQuantityRequired = true;
                    IsSecondAnnualQuantityRequired = false;
                    break;
                case "Book (Author)":
                    AnnualQuantityHeader = "Number of Books";
                    SecondAnnualQuantityHeader = "";
                    AnnualReachHeader = "Copies Sold";
                    IsUrlRequired = false;
                    IsAnnualQuantityRequired = true;
                    IsSecondAnnualQuantityRequired = false;
                    break;
                case "Book (Co-Author)":
                    AnnualQuantityHeader = "Number of Books";
                    SecondAnnualQuantityHeader = "";
                    AnnualReachHeader = "Copies Sold";
                    IsUrlRequired = false;
                    IsAnnualQuantityRequired = true;
                    IsSecondAnnualQuantityRequired = false;
                    break;
                case "Code Project/Tools":
                    AnnualQuantityHeader = "Number of Projects";
                    SecondAnnualQuantityHeader = "";
                    AnnualReachHeader = "Number of Downloads";
                    IsUrlRequired = true;
                    IsAnnualQuantityRequired = true;
                    IsSecondAnnualQuantityRequired = false;
                    break;
                case "Code Samples":
                    AnnualQuantityHeader = "Number of Samples";
                    SecondAnnualQuantityHeader = "";
                    AnnualReachHeader = "Number of Downloads";
                    IsUrlRequired = true;
                    IsAnnualQuantityRequired = true;
                    IsSecondAnnualQuantityRequired = false;
                    break;
                case "Conference (booth presenter)":
                    AnnualQuantityHeader = "Number of Conferences";
                    SecondAnnualQuantityHeader = "";
                    AnnualReachHeader = "Number of Visitors";
                    IsUrlRequired = false;
                    IsAnnualQuantityRequired = true;
                    IsSecondAnnualQuantityRequired = false;
                    break;
                case "Conference (organizer)":
                    AnnualQuantityHeader = "Number of Conferences";
                    SecondAnnualQuantityHeader = "";
                    AnnualReachHeader = "Number of Visitors";
                    IsUrlRequired = false;
                    IsAnnualQuantityRequired = true;
                    IsSecondAnnualQuantityRequired = false;
                    break;
                case "Forum Moderator":
                    AnnualQuantityHeader = "Number of Threads Moderated";
                    SecondAnnualQuantityHeader = "";
                    AnnualReachHeader = "Annual Reach";
                    IsUrlRequired = false;
                    IsAnnualQuantityRequired = true;
                    IsSecondAnnualQuantityRequired = false;
                    break;
                case "Forum Participation (3rd Party Forums)":
                    AnnualQuantityHeader = "Number of Answers";
                    SecondAnnualQuantityHeader = "Number of Posts";
                    AnnualReachHeader = "Views of Answers";
                    IsUrlRequired = true;
                    IsAnnualQuantityRequired = false;
                    IsSecondAnnualQuantityRequired = true;
                    break;
                case "Forum Participation (Microsoft Forums)":
                    AnnualQuantityHeader = "Number of Answers";
                    SecondAnnualQuantityHeader = "Number of Posts";
                    AnnualReachHeader = "Views of Answers";
                    IsUrlRequired = true;
                    IsAnnualQuantityRequired = true;
                    IsSecondAnnualQuantityRequired = false;
                    break;
                case "Mentorship":
                    AnnualQuantityHeader = "Number of Mentees";
                    SecondAnnualQuantityHeader = "";
                    AnnualReachHeader = "Annual Reach";
                    IsUrlRequired = false;
                    IsAnnualQuantityRequired = true;
                    IsSecondAnnualQuantityRequired = false;
                    break;
                case "Open Source Project(s)":
                    AnnualQuantityHeader = "Project(s)";
                    SecondAnnualQuantityHeader = "";
                    AnnualReachHeader = "Commits";
                    IsUrlRequired = false;
                    IsAnnualQuantityRequired = true;
                    IsSecondAnnualQuantityRequired = false;
                    break;
                case "Other":
                    AnnualQuantityHeader = "Annual Quantity";
                    SecondAnnualQuantityHeader = "";
                    AnnualReachHeader = "Annual Reach";
                    IsUrlRequired = false;
                    IsAnnualQuantityRequired = true;
                    IsSecondAnnualQuantityRequired = false;
                    break;
                case "Product Group Feedback":
                    AnnualQuantityHeader = "Number of Events provided";
                    SecondAnnualQuantityHeader = "";
                    AnnualReachHeader = "Number of Feedbacks provided";
                    IsUrlRequired = false;
                    IsAnnualQuantityRequired = true;
                    IsSecondAnnualQuantityRequired = false;
                    break;
                case "Site Owner":
                    AnnualQuantityHeader = "Posts";
                    SecondAnnualQuantityHeader = "";
                    AnnualReachHeader = "Visitors";
                    IsUrlRequired = false;
                    IsAnnualQuantityRequired = true;
                    IsSecondAnnualQuantityRequired = false;
                    break;
                case "Speaking (Conference)":
                    AnnualQuantityHeader = "Talks";
                    SecondAnnualQuantityHeader = "";
                    AnnualReachHeader = "Attendees of talks";
                    IsUrlRequired = false;
                    IsAnnualQuantityRequired = true;
                    IsSecondAnnualQuantityRequired = false;
                    break;
                case "Speaking (Local)":
                    AnnualQuantityHeader = "Talks";
                    SecondAnnualQuantityHeader = "";
                    AnnualReachHeader = "Attendees of talks";
                    IsUrlRequired = false;
                    IsAnnualQuantityRequired = true;
                    IsSecondAnnualQuantityRequired = false;
                    break;
                case "Speaking (User group)":
                    AnnualQuantityHeader = "Talks";
                    SecondAnnualQuantityHeader = "";
                    AnnualReachHeader = "Attendees of talks";
                    IsUrlRequired = false;
                    IsAnnualQuantityRequired = true;
                    IsSecondAnnualQuantityRequired = false;
                    break;
                case "Technical Social Media (Twitter, Facebook, LinkedIn...)":
                    AnnualQuantityHeader = "Number of Posts";
                    SecondAnnualQuantityHeader = "";
                    AnnualReachHeader = "Number of Followers";
                    IsUrlRequired = true;
                    IsAnnualQuantityRequired = true;
                    IsSecondAnnualQuantityRequired = false;
                    break;
                case "Translation Review, Feedback and Editing":
                    AnnualQuantityHeader = "Annual Quantity";
                    SecondAnnualQuantityHeader = "";
                    AnnualReachHeader = "Annual Reach";
                    IsUrlRequired = false;
                    IsAnnualQuantityRequired = true;
                    IsSecondAnnualQuantityRequired = false;
                    break;
                case "User Group Owner":
                    AnnualQuantityHeader = "Meetings";
                    SecondAnnualQuantityHeader = "Members";
                    AnnualReachHeader = "";
                    IsUrlRequired = false;
                    IsAnnualQuantityRequired = true;
                    IsSecondAnnualQuantityRequired = false;
                    break;
                case "Video":
                    AnnualQuantityHeader = "Number of Videos";
                    SecondAnnualQuantityHeader = "";
                    AnnualReachHeader = "Number of Views";
                    IsUrlRequired = true;
                    IsAnnualQuantityRequired = true;
                    IsSecondAnnualQuantityRequired = false;
                    break;
                case "Webcast":
                    AnnualQuantityHeader = "Number of Videos";
                    SecondAnnualQuantityHeader = "Number of Views";
                    AnnualReachHeader = "";
                    IsUrlRequired = false;
                    IsAnnualQuantityRequired = true;
                    IsSecondAnnualQuantityRequired = false;
                    break;
                case "Website Posts":
                    AnnualQuantityHeader = "Number of Posts";
                    SecondAnnualQuantityHeader = "Number of Subscribers";
                    AnnualReachHeader = "Annual Unique Visitors";
                    IsUrlRequired = true;
                    IsAnnualQuantityRequired = true;
                    IsSecondAnnualQuantityRequired = false;
                    break;
                default: // Fall back on 'other'
                    AnnualQuantityHeader = "Annual Quantity";
                    SecondAnnualQuantityHeader = "";
                    AnnualReachHeader = "Annual Reach";
                    IsUrlRequired = false;
                    IsAnnualQuantityRequired = true;
                    IsSecondAnnualQuantityRequired = false;
                    break;
            }
        }
        
        #endregion

        #region Navigation

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (App.ShellPage.DataContext is ShellPageViewModel shellVm && shellVm.IsLoggedIn)
            {
                try
                {
                    IsBusy = true;
                    // ** Get the associated lists from the API **

                    IsBusyMessage = "getting types...";

                    foreach (var type in await App.ApiService.GetContributionTypesAsync())
                    {
                        ContributionTypes.Add(type);
                    }


                    IsBusyMessage = "getting technologies...";

                    var areaResult = await App.ApiService.GetContributionAreasAsync();

                    foreach (var area in areaResult)
                    {
                        ContributionAreaAwardCategories.Add(area);

                        Debug.WriteLine($"Top Level Category: {area.AwardCategory}");

                        foreach (var area2 in area.Contributions)
                        {
                            Debug.WriteLine($"Award Name: {area2.AwardName}");

                            foreach (var area3 in area2.ContributionAreas)
                            {
                                Debug.WriteLine($"{area3.Name}");
                            }
                        }
                    }


                    IsBusyMessage = "getting visibility options...";

                    foreach (var visibility in await App.ApiService.GetVisibilitiesAsync())
                    {
                        ContributionVisibilies.Add(visibility);
                    }


                    // ** Determine if we're editing an existing contribution or creating a new one **

                    if (parameter is ContributionsModel param)
                    {
                        ActiveContribution = param;
                        IsContributionTypeEditable = false;

                        // Deep Cloning to serve as a comparison when editing
                        var json = JsonConvert.SerializeObject(param);
                        originalContribution = JsonConvert.DeserializeObject<ContributionsModel>(json);
                    }
                    else
                    {
                        ActiveContribution = new ContributionsModel
                        {
                            StartDate = DateTime.Now,
                            Visibility = ContributionVisibilies.FirstOrDefault(),
                            ContributionType = ContributionTypes.FirstOrDefault(), // Note this field is READONLY when it's an existing contribution
                            ContributionTypeName = ActiveContribution?.ContributionType?.Name
                        };

                        // -- TESTING ONLY -- 
                        //ActiveContribution.Title = "Test Upload";
                        //ActiveContribution.Description = "This is a test contribution upload from the UWP application I'm building for the MVP community to help them submit their 2018 contributions.";
                        //ActiveContribution.ReferenceUrl = "lancemccarthy.com";
                        //ActiveContribution.AnnualQuantity = 0;
                        //ActiveContribution.SecondAnnualQuantity = 0;
                        //ActiveContribution.AnnualReach = 0;

                        //IsDirty = true;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"LoadDataAsync Exception {ex}");
                }
                finally
                {
                    IsBusyMessage = "";
                    IsBusy = false;
                }
            }
            else
            {
                await NavigationService.NavigateAsync(typeof(LoginPage));
            }
        }

        public override Task OnNavigatedFromAsync(IDictionary<string, object> pageState, bool suspending)
        {
            return base.OnNavigatedFromAsync(pageState, suspending);
        }

        #endregion
    }
}


/*
 * Type, Areas, Title and Date all required
 * If the Activity type is follow by [url R], a Url is required
 *If the field is followed by an [R], it is a required field

 * If there are only 2 fields for the Activity Type, then it's AnnualQuantity and AnnualReach
 * If there are 3 fields then it's AnnualQuantity, SecondAnnualQuantity and AnnualReach

-Article
Number of Articles [R]
Number of Views

-Blog Site Posts [url R]
Number of Posts [R]
Number of Subscribers
Annual Unique Visitors

-Book (Author)
Number of Books [R]
Copies Sold

-Book (Co-Author)
Number of Books [R]
Copies Sold

-Code Project/Tools [url R]
Number of Projects [R]
Number of Downloads

-Code Samples [url R]
Number of Samples [R]
Number of Downloads

-Conference (booth presenter)
Number of Conferences [R]
Number of Visitors

-Conference (organizer)
Number of Conferences [R]
Number of Visitors

- Forum Moderator
Number of Threads Moderated [R]
Annual Reach

-Forum Participation (3rd Party Forums) [url R]
Number of Answers
Number of Posts [R]
Views of Answers

-Forum Participation (Microsoft Forums) [url R]
Number of Answers [R]
Number of Posts
Views of Answers

-Mentorship
Number of Mentees [R]
Annual Reach

-Open Source Project(s)
Project(s) [R]
Commits

-Other
Annual Quantity [R]
Annual Reach

-Product Group Feedback
Number of Events provided [R]
Number of Feedbacks provided

-Site Owner
Posts [R]
Visitors

-Speaking (Conference)
Talks [R]
Attendees of talks

-Speaking (Local)
Talks [R]
Attendees of talks

-Speaking (User group)
Talks [R]
Attendees of talks

-Technical Social Media (Twitter, Facebook, LinkedIn...) [url R]
Number of Posts [R]
Number of Followers

-Translation Review, Feedback and Editing
Annual Quantity [R]
Annual Reach

-User Group Owner
Meetings [R]
Members

-Video [url R]
Number of Videos [R]
Number of Views

-Webcast
Number of Videos [R]
Number of Views

-Website Posts [url R]
Number of Posts [R]
Number of Subscribers
Annual Unique Visitors
 * */
