using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.UI.Popups;
using CommonHelpers.Common;
using CommonHelpers.Mvvm;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using MvpApi.Common.Extensions;
using MvpApi.Common.Models;
using MvpApi.Wpf.Helpers;
using MvpApi.Wpf.Properties;
using ExceptionLogger = MvpApi.Services.Utilities.ExceptionLogger;
using SelectionChangedEventArgs = System.Windows.Controls.SelectionChangedEventArgs;

namespace MvpApi.Wpf.ViewModels
{
    public class AddContributionsViewModel : ViewModelBase
    {
        #region Fields

        private ContributionsModel selectedContribution;
        private string urlHeader = "Url";
        private string annualQuantityHeader = "Annual Quantity";
        private string secondAnnualQuantityHeader = "Second Annual Quantity";
        private string annualReachHeader = "Annual Reach";
        private bool isUrlRequired;
        private bool isAnnualQuantityRequired;
        private bool isSecondAnnualQuantityRequired;
        private bool isAnnualReachRequired;
        private bool canUpload = true;
        private bool isEditingQueuedItem;
        private string warningMessage;

        #endregion

        public AddContributionsViewModel()
        {
            //if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            //{
            //    Types = DesignTimeHelpers.GenerateContributionTypes();
            //    Visibilities = DesignTimeHelpers.GenerateVisibilities();
            //    UploadQueue = DesignTimeHelpers.GenerateContributions();
            //    SelectedContribution = UploadQueue.FirstOrDefault();

            //    return;
            //}

            EditQueuedContributionCommand = new DelegateCommand<ContributionsModel>(async cont => await EditContribution(cont));
            RemoveQueuedContributionCommand = new DelegateCommand<ContributionsModel>(async cont => await RemoveContribution(cont));
            RemoveAdditionalTechAreaCommand = new DelegateCommand<ContributionTechnologyModel>(RemoveAdditionalArea);

            UploadQueue.CollectionChanged += UploadQueue_CollectionChanged;
        }

        #region Properties

        // Collections and SelectedItems

        public ObservableCollection<ContributionsModel> UploadQueue { get; } = new ObservableCollection<ContributionsModel>();

        public ObservableCollection<ContributionTypeModel> Types { get; } = new ObservableCollection<ContributionTypeModel>();

        public ObservableCollection<VisibilityViewModel> Visibilities { get; } = new ObservableCollection<VisibilityViewModel>();

        public ObservableCollection<ContributionAreaContributionModel> CategoryAreas { get; } = new ObservableCollection<ContributionAreaContributionModel>();

        public ContributionsModel SelectedContribution
        {
            get => selectedContribution;
            set => SetProperty(ref selectedContribution, value);
        }

        // Data entry control headers, using VM properties to alert validation violations

        public string AnnualQuantityHeader
        {
            get => annualQuantityHeader;
            set => SetProperty(ref annualQuantityHeader, value);
        }

        public string SecondAnnualQuantityHeader
        {
            get => secondAnnualQuantityHeader;
            set => SetProperty(ref secondAnnualQuantityHeader, value);
        }

        public string AnnualReachHeader
        {
            get => annualReachHeader;
            set => SetProperty(ref annualReachHeader, value);
        }

        public string UrlHeader
        {
            get => urlHeader;
            set => SetProperty(ref urlHeader, value);
        }

        public bool IsUrlRequired
        {
            get => isUrlRequired;
            set => SetProperty(ref isUrlRequired, value);
        }

        public bool IsAnnualQuantityRequired
        {
            get => isAnnualQuantityRequired;
            set => SetProperty(ref isAnnualQuantityRequired, value);
        }

        public bool IsSecondAnnualQuantityRequired
        {
            get => isSecondAnnualQuantityRequired;
            set => SetProperty(ref isSecondAnnualQuantityRequired, value);
        }

        public bool IsAnnualReachRequired
        {
            get => isAnnualReachRequired;
            set => SetProperty(ref isAnnualReachRequired, value);
        }

        public bool CanUpload
        {
            get => canUpload;
            set => SetProperty(ref canUpload, value);
        }

        public bool IsEditingQueuedItem
        {
            get => isEditingQueuedItem;
            set => SetProperty(ref isEditingQueuedItem, value);
        }

        public string WarningMessage
        {
            get => warningMessage;
            set => SetProperty(ref warningMessage, value);
        }

        // Commands

        public DelegateCommand<ContributionsModel> EditQueuedContributionCommand { get; set; }

        public DelegateCommand<ContributionsModel> RemoveQueuedContributionCommand { get; set; }

        public DelegateCommand<ContributionTechnologyModel> RemoveAdditionalTechAreaCommand { get; set; }

        #endregion

        #region Event handlers

        private void UploadQueue_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            CanUpload = UploadQueue.Any();
        }

        //public void DatePicker_OnDateChanged(object sender, DatePickerValueChangedEventArgs e)
        //{
        //    if (e.NewDate < (App.Current.MainWindow as ShellWindow).ViewModel.SubmissionStartDate || e.NewDate > (App.Current.MainWindow as ShellWindow).ViewModel.SubmissionDeadline)
        //    {
        //        WarningMessage = "The date must be after the start of your current award period and before March 31st of the next award year.";

        //        //await new MessageDialog(WarningMessage, "Notice: Out of range").ShowAsync();

        //        CanUpload = false;
        //    }
        //    else
        //    {
        //        WarningMessage = "";
        //        //CanUpload = true;
        //    }
        //}

        public void ActivityType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null && e.AddedItems[0] is ContributionTypeModel type)
            {
                // There are complex rules around the names of the properties, this method determines the requirements and updates the UI accordingly
                DetermineContributionTypeRequirements(type);

                // Also need set the type name
                SelectedContribution.ContributionTypeName = type.EnglishName;
            }
        }

        //public async void AdditionalTechnologiesListView_OnItemClick(object sender, ItemClickEventArgs e)
        //{
        //    if (SelectedContribution.AdditionalTechnologies.Count < 2)
        //    {
        //        AddAdditionalArea(e.ClickedItem as ContributionTechnologyModel);
        //    }
        //    else
        //    {
        //        MessageBox.Show("You can only have two additional areas selected, remove one and try again.");
        //    }
        //}

        // Button click handlers
        public async void AddCurrentItemButton_Click(object sender, RoutedEventArgs e)
        {
            var validationResult = SelectedContribution.Validate();

            if (!validationResult.Item1)
            {
                await new MessageDialog($"The {validationResult.Item2} field is a required entry for this contribution type.").ShowAsync();
                //MessageBox.Show($"The {validationResult.Item2} field is a required entry for this contribution type.");

                return;
            }

            if (IsEditingQueuedItem)
            {
                IsEditingQueuedItem = false;
            }
            else
            {
                if (!UploadQueue.Contains(SelectedContribution))
                    UploadQueue.Add(SelectedContribution);
            }

            SetupNextEntry();
        }

        public void ClearCurrentItemButton_Click(object sender, RoutedEventArgs e)
        {
            SetupNextEntry();
        }

        public async void ClearQueueButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var md = new MessageDialog("You are about to clear all of the contributions in the upload queue, are you sure?", "CLEAR");
                md.Commands.Add(new UICommand("YES"));
                md.Commands.Add(new UICommand("whoa, no!"));

                var dialogResult = await md.ShowAsync();

                if (dialogResult.Label != "YES")
                    return;

                // WPF option
                //var result = MessageBox.Show(
                //    "You are about to clear all of the contributions in the upload queue, are you sure?",
                //    "CLEAR",
                //    MessageBoxButton.OKCancel,
                //    MessageBoxImage.Warning,
                //    MessageBoxResult.Cancel);

                //if (result != MessageBoxResult.OK)
                //    return;

                UploadQueue.Clear();

                SetupNextEntry();
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);

                await new MessageDialog($"Something went wrong clearing the queue, please try again. Error: {ex.Message}").ShowAsync();
                //MessageBox.Show($"Something went wrong clearing the queue, please try again. Error: {ex.Message}");
            }
        }

        public async void UploadQueue_Click(object sender, RoutedEventArgs e)
        {
            bool refreshNeeded = false;

            var startNumber = UploadQueue.Count;

            foreach (var contribution in UploadQueue)
            {
                contribution.UploadStatus = UploadStatus.InProgress;

                var success = await UploadContributionAsync(contribution);

                if (success && !refreshNeeded)
                {
                    refreshNeeded = true;
                }

                contribution.UploadStatus = success
                    ? UploadStatus.Success
                    : UploadStatus.Failed;
            }

            // remove successfully uploaded items form the queue
            UploadQueue.Remove(c => c.UploadStatus == UploadStatus.Success);

            var endNumber = UploadQueue.Count;

            Analytics.TrackEvent("Queue Upload Attempt", new Dictionary<string, string>
            {
                {"Starting Count", $"{startNumber}"},
                {"End Count", $"{endNumber}"},
                {"Failed Uploads", $"{startNumber - endNumber}"},
            });

            // Update the Contributions list cache from the API if there were any uploads.
            if (refreshNeeded)
            {
                IsBusy = true;
                IsBusyMessage = "refreshing contributions...";

                await App.ApiService.GetAllContributionsAsync(true);

                IsBusyMessage = string.Empty;
                IsBusy = false;
            }


            if (UploadQueue.Any())
            {
                // If there was a failure, there will still be items in the Queue, select the last one in the list
                SelectedContribution = UploadQueue.LastOrDefault();
            }
            else
            {
                // If everything was uploaded, go back to home view
                (App.Current.MainWindow as ShellWindow).RootNavigationView.SelectedIndex = 0;
            }
        }

        #endregion

        #region Methods

        private void SetupNextEntry()
        {
            // Set up first contribution, ID is 0 so that we don't accidentally overwrite the data in the form if another contribution is selected for editing
            // this ContributionId will be set to null before uploading.
            SelectedContribution = new ContributionsModel
            {
                ContributionId = 0,
                StartDate = DateTime.Now,
                Visibility = Visibilities.FirstOrDefault(),
                ContributionType = Types.FirstOrDefault(),
                ContributionTypeName = SelectedContribution?.ContributionType?.Name,
                ContributionTechnology = CategoryAreas?.FirstOrDefault()?.ContributionAreas.FirstOrDefault(),
                AdditionalTechnologies = new ObservableCollection<ContributionTechnologyModel>()
            };
        }

        public void DetermineContributionTypeRequirements(ContributionTypeModel contributionType)
        {
            // Each activity type has a unique set of field names and which ones are required.
            // This extension method will parse it and return a Tuple of the unqie requirements.
            var contributionTypeRequirements = contributionType.GetContributionTypeRequirements();

            // Set the headers of the input boxes
            AnnualQuantityHeader = contributionTypeRequirements.Item1;
            SecondAnnualQuantityHeader = contributionTypeRequirements.Item2;
            AnnualReachHeader = contributionTypeRequirements.Item3;

            // Determine the required fields for upload.
            IsUrlRequired = contributionTypeRequirements.Item4;
            IsAnnualQuantityRequired = !string.IsNullOrEmpty(contributionTypeRequirements.Item1);
            IsSecondAnnualQuantityRequired = !string.IsNullOrEmpty(contributionTypeRequirements.Item2);
            IsAnnualReachRequired = !string.IsNullOrEmpty(contributionTypeRequirements.Item3);
        }

        private void AddAdditionalArea(ContributionTechnologyModel area)
        {
            if (!SelectedContribution.AdditionalTechnologies.Contains(area))
            {
                SelectedContribution.AdditionalTechnologies.Add(area);
            }
        }

        private void RemoveAdditionalArea(ContributionTechnologyModel area)
        {
            if (SelectedContribution.AdditionalTechnologies.Contains(area))
            {
                SelectedContribution.AdditionalTechnologies.Remove(area);
            }
        }

        #endregion

        #region API calls

        public async Task EditContribution(ContributionsModel contribution)
        {
            if (contribution == null)
                return;

            if (contribution.ContributionId == 0)
            {
                // UWP option
                var md = new MessageDialog("Editing this contribution will replace the unsaved information you have in the form. If you do not want to lose that information, click 'cancel' and add it to the queue before editing this one.", "Warning");
                md.Commands.Add(new UICommand("edit"));
                md.Commands.Add(new UICommand("cancel"));

                var dialogResult = await md.ShowAsync();

                if (dialogResult.Label == "cancel")
                    return;

                // WPF option
                //var result = MessageBox.Show(
                //    "Editing this contribution will replace the unsaved information you have in the form. If you do not want to lose that information, click 'cancel' and add it to the queue before editing this one.",
                //    "Warning",
                //    MessageBoxButton.OKCancel,
                //    MessageBoxImage.Warning,
                //    MessageBoxResult.Cancel);

                //if (result != MessageBoxResult.OK)
                //    return;
            }

            SelectedContribution = contribution;
            IsEditingQueuedItem = true;

            Analytics.TrackEvent("Contribution Edited");
        }

        public async Task RemoveContribution(ContributionsModel contribution)
        {
            if (contribution == null)
                return;

            try
            {
                // UWP option
                var md = new MessageDialog("Are you sure you want to remove this contribution from the queue?", "Remove Contribution?");
                md.Commands.Add(new UICommand("yes"));
                md.Commands.Add(new UICommand("cancel"));

                var dialogResult = await md.ShowAsync();

                if (dialogResult.Label == "yes")
                {
                    if (SelectedContribution == contribution)
                    {
                        SelectedContribution = null;
                    }

                    UploadQueue.Remove(contribution);

                    if (UploadQueue.Count == 0)
                    {
                        SetupNextEntry();
                    }
                }

                // WPF option
                //var result = MessageBox.Show(
                //    "Are you sure you want to remove this contribution from the queue?",
                //    "Remove Contribution?",
                //    MessageBoxButton.YesNo,
                //    MessageBoxImage.Warning,
                //    MessageBoxResult.No);

                //if (result != MessageBoxResult.Yes)
                //    return;


                //if (SelectedContribution == contribution)
                //{
                //    SelectedContribution = null;
                //}

                //UploadQueue.Remove(contribution);

                //if (UploadQueue.Count == 0)
                //{
                //    SetupNextEntry();
                //}

                Analytics.TrackEvent("Contribution Removed");
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);
                await new MessageDialog($"Something went wrong deleting this item, please try again. Error: {ex.Message}").ShowAsync();
            }
        }

        public async Task<bool> UploadContributionAsync(ContributionsModel contribution)
        {
            try
            {
                var submissionResult = await App.ApiService.SubmitContributionAsync(contribution);

                // copying back the ID which was created on the server once the item was added to the database
                contribution.ContributionId = submissionResult.ContributionId;

                Analytics.TrackEvent("New Contribution Uploaded");

                return true;
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);

                await new MessageDialog($"Something went wrong saving '{contribution.Title}', it will remain in the queue for you to try again.\r\n\nError: {ex.Message}").ShowAsync();
                return false;
            }
        }

        private async Task LoadSupportingDataAsync()
        {
            IsBusyMessage = "loading types...";

            var types = await App.ApiService.GetContributionTypesAsync();

            foreach (var contributionTypeModel in types)
            {
                Types.Add(contributionTypeModel);
            }

            IsBusyMessage = "loading technologies...";

            var areaRoots = await App.ApiService.GetContributionAreasAsync();

            // Flatten out the result so that we only have a single level of grouped data, this is used for the CollectionViewSource, defined in the XAML.
            var areas = areaRoots.SelectMany(areaRoot => areaRoot.Contributions);

            foreach (var contributionAreaContributionModel in areas)
            {
                CategoryAreas.Add(contributionAreaContributionModel);
            }

            // TODO Try and get the CollectionViewSource to invoke now so that the LoadNextEntry will be able to preselected award category.

            IsBusyMessage = "loading visibility options...";

            var visibilities = await App.ApiService.GetVisibilitiesAsync();

            foreach (var visibilityViewModel in visibilities)
            {
                Visibilities.Add(visibilityViewModel);
            }
        }

        #endregion

        #region Navigation

        public async Task OnLoadedAsync()
        {
            //if (!NetworkHelper.Current.CheckInternetConnection())
            //{

            //}

            // Verify the user is logged in
            //if (!(App.Current.MainWindow as ShellWindow).ViewModel.IsLoggedIn)
            //{
            //    IsBusy = true;
            //    IsBusyMessage = "logging in...";

            //    await (App.Current.MainWindow as ShellWindow).SignInAsync();

            //    IsBusyMessage = "";
            //    IsBusy = false;
            //}

            if (!App.ApiService.IsLoggedIn)
            {
                IsBusy = true;
                IsBusyMessage = "signing in...";

                await App.MainLoginWindow.SignInAsync();
            }
            

            //if ((App.Current.MainWindow as ShellWindow).ViewModel.IsLoggedIn)
            //{
                try
                {
                    IsBusy = true;

                    // ** Get the necessary associated data from the API **

                    await LoadSupportingDataAsync();

                    // Note: The Category Areas will not be loaded until the CollectionViewSource is does it's initially loading.
                    SetupNextEntry();

                    if (!Settings.Default.AddContributionTutorialShown)
                    {

                    }

                    //if (!(ApplicationData.Current.LocalSettings.Values["AddContributionPageTutorialShown"] is bool tutorialShown) || !tutorialShown)
                    //{
                        //var td = new TutorialDialog
                        //{
                        //    SettingsKey = "AddContributionPageTutorialShown",
                        //    MessageTitle = "Add Contribution Page",
                        //    Message = "This page allows you to add contributions to your MVP profile.\r\n\n" +
                        //              "- Complete the form and the click the 'Add' button to add the completed contribution to the upload queue.\r\n" +
                        //              "- You can edit or remove items that are already in the queue using the item's 'Edit' or 'Remove' buttons.\r\n" +
                        //              "- Click 'Upload' to save the queue to your profile.\r\n" +
                        //              "- You can clear the form, or the entire queue, using the 'Clear' buttons.\r\n\n" +
                        //              "TIP: Watch the queue items color change as the items are uploaded and save is confirmed."
                        //};

                        //await td.ShowAsync();
                    //}

                }
                catch (Exception ex)
                {
                    Crashes.TrackError(ex);
                    Debug.WriteLine($"AddContributions OnNavigatedToAsync Exception {ex}");
                }
                finally
                {
                    IsBusyMessage = "";
                    IsBusy = false;
                }
            //}
        }

        #endregion
    }
}