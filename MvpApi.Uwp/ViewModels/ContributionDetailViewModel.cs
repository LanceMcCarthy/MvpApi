using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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
        private bool canSave;
        private bool evaluationInProgress;

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

        public bool CanSave
        {
            get => canSave;
            set => Set(ref canSave, value);
        }

        #endregion

        #region Event handlers

        public void TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            EvaluateCanSave();
        }

        public void UrlBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            EvaluateCanSave();
        }

        public void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EvaluateCanSave();
        }

        public void DatePicker_OnDateChanged(object sender, DatePickerValueChangedEventArgs e)
        {
            EvaluateCanSave();
        }

        public void AnnualQuantityBox_OnValueChanged(object sender, EventArgs e)
        {
            EvaluateCanSave();
        }

        public void SecondAnnualQuantityBox_OnValueChanged(object sender, EventArgs e)
        {
            EvaluateCanSave();
        }

        public void ActivityType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateHeaders(ActiveContribution.ContributionType);

            ActiveContribution.ContributionTypeName = ActiveContribution.ContributionType.EnglishName;

            EvaluateCanSave();
        }

        private void EvaluateCanSave()
        {
            if (evaluationInProgress)
                return;

            evaluationInProgress = true;

            // These are all required, no matter the activity type
            if (string.IsNullOrEmpty(ActiveContribution?.Title) ||
                string.IsNullOrEmpty(ActiveContribution?.ContributionTypeName) ||
                ActiveContribution?.ContributionType == null ||
                ActiveContribution?.Visibility == null ||
                ActiveContribution?.ContributionTechnology == null)
            {
                CanSave = false;
            }
            else if (ActiveContribution?.StartDate?.Date > DateTime.Now)
            {
                CanSave = false;
            }
            else if (IsAnnualQuantityRequired && ActiveContribution?.AnnualQuantity == null)
            {
                CanSave = false;
            }
            else if (IsSecondAnnualQuantityRequired && ActiveContribution?.SecondAnnualQuantity == null)
            {
                CanSave = false;
            }
            else if (IsUrlRequired && string.IsNullOrEmpty(ActiveContribution?.ReferenceUrl))
            {
                CanSave = false;
            }
            else
            {
                CanSave = true;
            }

            evaluationInProgress = false;
        }

        public async void UploadContributionButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate all required fields
            var isValidated = await ValidateRequiredFields();

            if (!isValidated)
            {
                await new MessageDialog("You need to fill in every field marked as 'Required'").ShowAsync();
                return;
            }

            var md = new MessageDialog("Upload and save this contribution?");
            md.Commands.Add(new UICommand("save"));
            md.Commands.Add(new UICommand("cancel"));

            var dialogResult = await md.ShowAsync();

            if (dialogResult.Label == "cancel")
                return;

            ActiveContribution.ContributionTechnology = new ContributionTechnologyModel
            {
                Id = SelectedContributionAreaModel.Id,
                AwardCategory = SelectedContributionAreaModel.AwardCategory,
                AwardName = SelectedContributionAreaModel.AwardName,
                Name = SelectedContributionAreaModel.Name
            };

            var success = await SaveContributionAsync();

            if (success) await SetupNextEntryAsync();
        }

        public async void DeleteContributionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var md = new MessageDialog("Are you sure you want to delete this contribution?", "Delete Contribution?");
                md.Commands.Add(new UICommand("DELETE"));
                md.Commands.Add(new UICommand("cancel"));

                var dialogResult = await md.ShowAsync();

                if (dialogResult.Label != "DELETE")
                    return;

                var result = await App.ApiService.DeleteContributionAsync(ActiveContribution);

                if (result == true)
                {
                    if(NavigationService.CanGoBack)
                        NavigationService.GoBack();
                }
            }
            catch (Exception ex)
            {
                await new MessageDialog($"Something went wrong saving the item, please try again. Error: {ex.Message}").ShowAsync();
            }
        }

        #endregion

        #region Methods
        
        public async Task<bool> SaveContributionAsync()
        {
            try
            {
                var submissionResult = await App.ApiService.SubmitContributionAsync(ActiveContribution);

                ActiveContribution.ContributionId = submissionResult.ContributionId;

                return true;
            }
            catch (Exception ex)
            {
                await new MessageDialog($"Something went wrong saving the item, please try again. Error: {ex.Message}").ShowAsync();
                return false;
            }
        }

        private async Task SetupNextEntryAsync()
        {
            var continueDialog = new MessageDialog("You can prefill the same award category selections as the last one, or start over fresh", "Uploaded! Enter another contribution?");
            continueDialog.Commands.Add(new UICommand("Yes (head start)"));
            continueDialog.Commands.Add(new UICommand("Yes (start over)"));
            continueDialog.Commands.Add(new UICommand("I'm done!"));
            var continueDialogResult = await continueDialog.ShowAsync();

            // if user wants to enter a new contribution, give them a headstart by re-using the dropdown selections from the last submission

            if (continueDialogResult.Label == "Yes (head start)")
            {
                var nextContribution = new ContributionsModel
                {
                    StartDate = DateTime.Now,
                    Visibility = ActiveContribution?.Visibility,
                    ContributionType = ActiveContribution?.ContributionType,
                    ContributionTypeName = ActiveContribution?.ContributionType?.Name,
                    ContributionTechnology = ActiveContribution?.ContributionTechnology
                };

                ActiveContribution = nextContribution;
            }
            else if (continueDialogResult.Label == "Yes (start over)")
            {
                // default
                ActiveContribution = new ContributionsModel
                {
                    StartDate = DateTime.Now,
                    Visibility = ContributionVisibilies.FirstOrDefault(),
                    ContributionType = ContributionTypes.FirstOrDefault(),
                };
            }
            else
            {
                
            }
        }
        
        private async Task<bool> ValidateRequiredFields()
        {
            if (ActiveContribution.ContributionType == null)
            {
                await new MessageDialog($"The Contribution Type is a required field").ShowAsync();
                return false;
            }

            if (ActiveContribution.Visibility == null)
            {
                await new MessageDialog($"The Visibility is a required field").ShowAsync();
                return false;
            }

            if (ActiveContribution.ContributionTechnology == null)
            {
                await new MessageDialog($"The Category Technology is a required field").ShowAsync();
                return false;
            }

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

                        ActivityType_SelectionChanged(null, null);

                        IsContributionTypeEditable = false;

                        // Deep cloning the object to serve as a clean original to compare against when editing and determine if the item is dirty or not.
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
