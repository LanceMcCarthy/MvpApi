using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using MvpApi.Common.Models;
using MvpApi.Uwp.Extensions;
using MvpApi.Uwp.Helpers;
using MvpApi.Uwp.Views;
using Newtonsoft.Json;
using Template10.Common;
using Template10.Mvvm;
using Template10.Utils;

namespace MvpApi.Uwp.ViewModels
{
    public class ContributionDetailViewModel : PageViewModelBase
    {
        #region Fields

        private ContributionsModel originalContribution;
        private ContributionsModel selectedContribution;
        private bool isSelectedContributionDirty;
        private string urlHeader = "Url";
        private string annualQuantityHeader = "Annual Quantity";
        private string secondAnnualQuantityHeader = "Second Annual Quantity";
        private string annualReachHeader = "Annual Reach";
        private bool isUrlRequired;
        private bool isAnnualQuantityRequired;
        private bool isSecondAnnualQuantityRequired;
        private bool canSave;
        private ContributionTechnologyModel selectedCategoryAreaTechnology;
        private VisibilityViewModel selectedVisibility;

        #endregion

        public ContributionDetailViewModel()
        {
            if (DesignMode.DesignModeEnabled || DesignMode.DesignMode2Enabled)
            {
                if (DesignMode.DesignModeEnabled || DesignMode.DesignMode2Enabled)
                {
                    //CategoryAreas = DesignTimeHelpers.GenerateTechnologyAreas(); //Causing designer layout error
                    Visibilies = DesignTimeHelpers.GenerateVisibilities();
                    SelectedContribution = DesignTimeHelpers.GenerateContributions().FirstOrDefault();
                    SelectedCategoryAreaTechnology = DesignTimeHelpers.GenerateAreaTechnologies().FirstOrDefault();
                    SelectedVisibility = DesignTimeHelpers.GenerateVisibilities().FirstOrDefault();
                }
            }
        }

        #region Properties

        public ContributionsModel SelectedContribution
        {
            get => selectedContribution;
            set => Set(ref selectedContribution, value);
        }
        
        public ObservableCollection<ContributionAreaContributionModel> CategoryAreas { get; } = new ObservableCollection<ContributionAreaContributionModel>();

        public ObservableCollection<VisibilityViewModel> Visibilies { get; } = new ObservableCollection<VisibilityViewModel>();

        public ContributionTechnologyModel SelectedCategoryAreaTechnology
        {
            get => selectedCategoryAreaTechnology;
            set => Set(ref selectedCategoryAreaTechnology, value);
        }

        public VisibilityViewModel SelectedVisibility
        {
            get => selectedVisibility;
            set => Set(ref selectedVisibility, value);
        }

        public bool IsSelectedContributionDirty
        {
            get => isSelectedContributionDirty;
            set
            {
                Set(ref isSelectedContributionDirty, value);

                // if the object has changes, update the status to pending
                SelectedContribution.UploadStatus = value 
                    ? UploadStatus.Pending 
                    : UploadStatus.None;
            }
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

        // Data form event handlers

        public void EditContributionButton_OnClick(object sender, RoutedEventArgs e)
        {

        }

        public async void TitleTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (originalContribution != null)
                IsSelectedContributionDirty = SelectedContribution.Title == originalContribution.Title;

            CanSave = await SelectedContribution.Validate();
        }

        public async void DescriptionTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (originalContribution != null)
                IsSelectedContributionDirty = SelectedContribution.Description == originalContribution.Description;

            CanSave = await SelectedContribution.Validate();
        }

        public async void UrlTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (originalContribution != null)
                IsSelectedContributionDirty = SelectedContribution.ReferenceUrl == originalContribution.ReferenceUrl;

            CanSave = await SelectedContribution.Validate();
        }

        public async void TechnologyComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (originalContribution != null)
                IsSelectedContributionDirty = SelectedContribution.ContributionTechnology.Id == originalContribution.ContributionTechnology.Id;

            CanSave = await SelectedContribution.Validate();
        }

        public async void VisibilityComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (originalContribution != null)
                IsSelectedContributionDirty = SelectedContribution.Visibility.Id == originalContribution.Visibility.Id;

            CanSave = await SelectedContribution.Validate();
        }

        public async void DatePicker_OnDateChanged(object sender, DatePickerValueChangedEventArgs e)
        {
            if (e.NewDate < new DateTime(2016, 10, 1) || e.NewDate > new DateTime(2018, 3, 31))
            {
                await new MessageDialog("The contribution date must be after the start of your current award period and before March 31, 2018 in order for it to count towards your evaluation", "Notice: Out of range").ShowAsync();

                if (originalContribution != null)
                    SelectedContribution.StartDate = originalContribution.StartDate;
            }

            if(originalContribution != null)
                IsSelectedContributionDirty = SelectedContribution.StartDate == originalContribution.StartDate;

            CanSave = await SelectedContribution.Validate();
        }

        public async void AnnualQuantityBox_OnValueChanged(object sender, EventArgs e)
        {
            if (originalContribution != null)
                IsSelectedContributionDirty = SelectedContribution.AnnualQuantity == originalContribution.AnnualQuantity;
            CanSave = await SelectedContribution.Validate();
        }

        public async void SecondAnnualQuantityBox_OnValueChanged(object sender, EventArgs e)
        {
            if (originalContribution != null)
                IsSelectedContributionDirty = SelectedContribution.SecondAnnualQuantity == originalContribution.SecondAnnualQuantity;

            CanSave = await SelectedContribution.Validate();
        }

        // CommandBar event handlers

        public async void UploadContributionButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedContribution.Visibility = SelectedVisibility;
            SelectedContribution.ContributionTechnology = SelectedCategoryAreaTechnology;

            var isValid = await SelectedContribution.Validate(true);

            if (!isValid)
                return;

            SelectedContribution.UploadStatus = UploadStatus.InProgress;

            var success = await UploadContributionAsync(SelectedContribution);

            // Mark success or failure
            SelectedContribution.UploadStatus = success ? UploadStatus.Success : UploadStatus.Failed;

            if (SelectedContribution.UploadStatus == UploadStatus.Success)
            {
                if (BootStrapper.Current.NavigationService.CanGoBack)
                    BootStrapper.Current.NavigationService.GoBack();
            }
        }

        public async void DeleteContributionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var md = new MessageDialog("Are you sure you want to delete this contribution? Deleting a contribution from the MVP database cannot be undone.", "Delete Contribution?");
                md.Commands.Add(new UICommand("DELETE"));
                md.Commands.Add(new UICommand("cancel"));

                var dialogResult = await md.ShowAsync();

                if (dialogResult.Label != "DELETE")
                    return;

                var result = await App.ApiService.DeleteContributionAsync(SelectedContribution);

                if (result == true)
                {
                    await new MessageDialog("Successfully deleted.").ShowAsync();

                    if (BootStrapper.Current.NavigationService.CanGoBack)
                        BootStrapper.Current.NavigationService.GoBack();
                }
                else
                {
                    await new MessageDialog("The contribution was not deleted, check your internet connection and try again.").ShowAsync();
                }
            }
            catch (Exception ex)
            {
                await new MessageDialog($"Something went wrong deleting this item, please try again. Error: {ex.Message}").ShowAsync();
            }
        }

        #endregion

        #region Methods
        
        private async Task LoadSupportingDataAsync()
        {
            IsBusyMessage = "loading category area technologies...";

            var areaRoots = await App.ApiService.GetContributionAreasAsync();

            // Flatten out the result so that we only have a single level of grouped data, this is used for the CollectionViewSource, defined in the XAML.
            var areas = areaRoots.SelectMany(areaRoot => areaRoot.Contributions);

            areas.ForEach(area =>
            {
                CategoryAreas.Add(area);
            });


            IsBusyMessage = "loading visibility options...";

            var visibilities = await App.ApiService.GetVisibilitiesAsync();

            visibilities.ForEach(visibility =>
            {
                Visibilies.Add(visibility);
            });
        }

        public async Task<bool> UploadContributionAsync(ContributionsModel contribution)
        {
            try
            {
                var submissionResult = await App.ApiService.SubmitContributionAsync(contribution);

                // copying back the ID which was created on the server once the item was added to the database
                contribution.ContributionId = submissionResult.ContributionId;

                return true;
            }
            catch (Exception ex)
            {
                await new MessageDialog($"Something went wrong saving the item, please try again. Error: {ex.Message}").ShowAsync();
                return false;
            }
        }
        
        public void DetermineContributionTypeRequirements(ContributionTypeModel contributionType)
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

                    // Get the associated lists from the API
                    await LoadSupportingDataAsync();

                    // Read the passed contribution parameter
                    if (parameter is ContributionsModel param)
                    {
                        SelectedContribution = param;

                        SelectedVisibility = SelectedContribution.Visibility;
                        SelectedCategoryAreaTechnology = SelectedContribution.ContributionTechnology;

                        SelectedContribution.UploadStatus = UploadStatus.None;

                        // There are complex rules around the names of the properties, this method determines the requirements and updates the UI accordingly
                        DetermineContributionTypeRequirements(SelectedContribution.ContributionType);

                        // cloning the object to serve as a clean original to compare against when editing and determine if the item is dirty or not.
                        originalContribution = SelectedContribution.Clone();
                    }
                    else
                    {
                        await new MessageDialog("Something went wrong loading your selection, going back to Home page").ShowAsync();

                        if (BootStrapper.Current.NavigationService.CanGoBack)
                            BootStrapper.Current.NavigationService.GoBack();
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
                await BootStrapper.Current.NavigationService.NavigateAsync(typeof(LoginPage));
            }
        }

        public override Task OnNavigatedFromAsync(IDictionary<string, object> pageState, bool suspending)
        {
            return base.OnNavigatedFromAsync(pageState, suspending);
        }

        #endregion

    }
}