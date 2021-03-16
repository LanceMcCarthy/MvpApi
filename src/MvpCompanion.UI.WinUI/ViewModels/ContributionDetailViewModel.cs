using CommonHelpers.Common;
using CommonHelpers.Mvvm;
using Microsoft.Toolkit.Uwp.Connectivity;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MvpApi.Common.Models;
using MvpCompanion.UI.Common.Extensions;
using MvpCompanion.UI.Common.Helpers;
using MvpCompanion.UI.WinUI.Dialogs;
using MvpCompanion.UI.WinUI.Views;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI.Popups;
using MvpCompanion.UI.WinUI.Common;

namespace MvpCompanion.UI.WinUI.ViewModels
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
        private bool isAnnualReachRequired;
        private bool canSave;
        private string warningMessage;

        //private AdditionalTechnologyAreasPicker picker;

        #endregion

        public ContributionDetailViewModel()
        {
            if (DesignMode.DesignModeEnabled || DesignMode.DesignMode2Enabled)
            {
                Visibilities = DesignTimeHelpers.GenerateVisibilities();
                SelectedContribution = DesignTimeHelpers.GenerateContributions().FirstOrDefault();
            }

            RemoveAdditionalTechAreaCommand = new DelegateCommand<ContributionTechnologyModel>(RemoveAdditionalArea);
        }

        #region Properties

        public ContributionsModel SelectedContribution
        {
            get => selectedContribution;
            set => SetProperty(ref selectedContribution, value);
        }
        
        public ObservableCollection<ContributionAreaContributionModel> CategoryAreas { get; } = new ObservableCollection<ContributionAreaContributionModel>();

        public ObservableCollection<VisibilityViewModel> Visibilities { get; } = new ObservableCollection<VisibilityViewModel>();
        
        public bool IsSelectedContributionDirty
        {
            get => isSelectedContributionDirty;
            set => SetProperty(ref isSelectedContributionDirty, value);
        }

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

        public bool CanSave
        {
            get => canSave;
            set => SetProperty(ref canSave, value);
        }

        public string WarningMessage
        {
            get => warningMessage;
            set => SetProperty(ref warningMessage, value);
        }

        public bool IsAdditionalAreasReady { get; set; }

        public DelegateCommand<ContributionTechnologyModel> RemoveAdditionalTechAreaCommand { get; set; }

        #endregion
        
        #region Event handlers
        
        public void TitleTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Compare();
        }

        public void DescriptionTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Compare();
        }

        public void UrlTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Compare();
        }

        public void TechnologyComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Compare();
        }
        
        public void DatePicker_OnDateChanged(object sender, DatePickerValueChangedEventArgs e)
        {
            if (e.NewDate < (ShellPage.Instance.DataContext as ShellViewModel).SubmissionStartDate || e.NewDate > (ShellPage.Instance.DataContext as ShellViewModel).SubmissionDeadline)
            {
                WarningMessage = "The contribution date must be after the start of your current award period and before March 31, 2019 in order for it to count towards your evaluation";
            }
            else
            {
                WarningMessage = "";
            }

            Compare();
        }

        public void QuantityBox_OnValueChanged(object sender, EventArgs e)
        {
            Compare();
        }

        public async void AdditionalTechnologiesListView_OnItemClick(object sender, ItemClickEventArgs e)
        {
            if (SelectedContribution.AdditionalTechnologies.Count < 2)
            {
                AddAdditionalArea(e.ClickedItem as ContributionTechnologyModel);
            }
            else
            {
                await new MessageDialog("You can only have two additional areas selected, remove one and try again.").ShowAsync();
            }
        }
        
        public async void UploadContributionButton_Click(object sender, RoutedEventArgs e)
        {
            var isValid = await SelectedContribution.Validate(true);

            if (!isValid)
                return;

            SelectedContribution.UploadStatus = UploadStatus.InProgress;
            
            var success = await UploadContributionAsync(SelectedContribution);
            
            // Mark success or failure
            SelectedContribution.UploadStatus = success ? UploadStatus.Success : UploadStatus.Failed;

            // Quality assurance, only logs a successful or failed upload.
            //if (ApiInformation.IsTypePresent("Microsoft.Services.Store.Engagement.StoreServicesCustomEventLogger"))
            //{
            //    StoreServicesCustomEventLogger.GetDefault().Log($"EditContribution{SelectedContribution.UploadStatus}");
            //}

            if (SelectedContribution.UploadStatus == UploadStatus.Success)
            {
                IsSelectedContributionDirty = false;

                // Refresh the main cached contributions list because the details for this item has changed
                IsBusy = true;
                IsBusyMessage = "refreshing contributions...";

                await App.ApiService.GetAllContributionsAsync(true);

                IsBusyMessage = string.Empty;
                IsBusy = false;

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
                    // Quality assurance, only logs a successful delete.
                    //if (ApiInformation.IsTypePresent("Microsoft.Services.Store.Engagement.StoreServicesCustomEventLogger"))
                    //    StoreServicesCustomEventLogger.GetDefault().Log("DeleteContributionSuccess");

                    // Refresh the main cached contributions list because the details for this item has changed
                    IsBusy = true;
                    IsBusyMessage = "refreshing contributions...";

                    await App.ApiService.GetAllContributionsAsync(true);

                    IsBusyMessage = string.Empty;
                    IsBusy = false;

                    if (BootStrapper.Current.NavigationService.CanGoBack)
                        BootStrapper.Current.NavigationService.GoBack();
                }
                else
                {
                    // Quality assurance, only logs a failed delete.
                    //if (ApiInformation.IsTypePresent("Microsoft.Services.Store.Engagement.StoreServicesCustomEventLogger"))
                    //    StoreServicesCustomEventLogger.GetDefault().Log("DeleteContributionFailed");
                }
            }
            catch (Exception ex)
            {
                // Quality assurance, only logs a failed delete.
                //if (ApiInformation.IsTypePresent("Microsoft.Services.Store.Engagement.StoreServicesCustomEventLogger"))
                //    StoreServicesCustomEventLogger.GetDefault().Log("DeleteContributionFailed");

                await ex.LogExceptionAsync();
            }
        }

        #endregion

        #region Methods
        
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

        private void Compare()
        {
            var match = originalContribution.Compare(SelectedContribution);

            if (match)
            {
                IsSelectedContributionDirty = false;
                SelectedContribution.UploadStatus = UploadStatus.None;
            }
            else
            {
                IsSelectedContributionDirty = true;
                SelectedContribution.UploadStatus = UploadStatus.Pending;
            }

            //try
            //{
            //    if (_originalContribution == null)
            //        return;

            //    var isTitleDifferent = SelectedContribution.Title != _originalContribution.Title;
            //    var isDescriptionDifferent = SelectedContribution.Description != _originalContribution.Description;
            //    var isUrlDifferent = SelectedContribution.ReferenceUrl != _originalContribution.ReferenceUrl;
            //    var isTechnologyDifferent = SelectedContribution.ContributionTechnology.Id != _originalContribution.ContributionTechnology.Id;
            //    var isDateDifferent = SelectedContribution.StartDate.Value.Date != _originalContribution.StartDate.Value.Date;
            //    var isAnnualQuantityDifferent = SelectedContribution.AnnualQuantity != _originalContribution.AnnualQuantity;
            //    var isSecondAnnualQuantityDifferent = SelectedContribution.SecondAnnualQuantity != _originalContribution.SecondAnnualQuantity;
            //    var isAnnualReachDifferent = SelectedContribution.AnnualReach != _originalContribution.AnnualReach;

            //    if (isTitleDifferent
            //        || isDescriptionDifferent
            //        || isUrlDifferent
            //        || isTechnologyDifferent
            //        || isDateDifferent
            //        || isAnnualQuantityDifferent
            //        || isSecondAnnualQuantityDifferent
            //        || isAnnualReachDifferent)
            //    {
            //        IsSelectedContributionDirty = true;
            //        SelectedContribution.UploadStatus = UploadStatus.Pending;
            //    }
            //    else
            //    {
            //        IsSelectedContributionDirty = false;
            //        SelectedContribution.UploadStatus = UploadStatus.None;
            //    }
            //}
            //catch
            //{
                
            //}
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

        #endregion

        #region API tasks

        private async Task LoadSupportingDataAsync()
        {
            IsBusyMessage = "loading category area technologies...";

            var areaRoots = await App.ApiService.GetContributionAreasAsync();

            // Flatten out the result so that we only have a single level of grouped data, this is used for the CollectionViewSource, defined in the XAML.
            var areas = areaRoots.SelectMany(areaRoot => areaRoot.Contributions);
            
            foreach (var area in areas)
            {
                CategoryAreas.Add(area);
            }
            
            IsBusyMessage = "loading visibility options...";

            var visibilities = await App.ApiService.GetVisibilitiesAsync();
            
            foreach (var visibility in visibilities)
            {
                Visibilities.Add(visibility);
            }
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

        #endregion

        #region Navigation

        public override async void OnPageNavigatedTo(NavigationEventArgs e)
        {
            if (!NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
            {
                if (BootStrapper.Current.NavigationService.CanGoBack)
                    BootStrapper.Current.NavigationService.GoBack();
            }

            if (ShellPage.Instance.DataContext is ShellViewModel shellVm)
            {
                // Verify the user is logged in
                if (!shellVm.IsLoggedIn)
                {
                    IsBusy = true;
                    IsBusyMessage = "logging in...";

                    await ShellPage.Instance.SignInAsync();

                    IsBusyMessage = "";
                    IsBusy = false;
                }

                if (shellVm.IsLoggedIn)
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

                            SelectedContribution.UploadStatus = UploadStatus.None;

                            // There are complex rules around the names of the properties, this method determines the requirements and updates the UI accordingly
                            DetermineContributionTypeRequirements(SelectedContribution.ContributionType);

                            // cloning the object to serve as a clean original to compare against when editing and determine if the item is dirty or not.
                            originalContribution = SelectedContribution.Clone();

                            if (!(ApplicationData.Current.LocalSettings.Values["ContributionDetailPageTutorialShown"] is bool tutorialShown) || !tutorialShown)
                            {
                                var td = new TutorialDialog
                                {
                                    SettingsKey = "ContributionDetailPageTutorialShown",
                                    MessageTitle = "Contribution Details",
                                    Message = "This page shows an existing contribution's details, you cannot change the Activity Type, but other fields are editable.\r\n\n" +
                                              "- Click 'Save' button to save changes.\r\n" +
                                              "- Click 'Delete' button to permanently delete the contribution.\r\n" +
                                              "- Click the back button to leave and cancel any changes.\r\n\n" +
                                              "Note: Pay attention to how the 'required' fields change depending on the technology selection."
                                };

                                await td.ShowAsync();
                            }
                        }
                        else
                        {
                            await new MessageDialog("Something went wrong loading your selection, going back to Home page").ShowAsync();

                            if (BootStrapper.Current.NavigationService.CanGoBack)
                                BootStrapper.Current.NavigationService.GoBack();
                        }

                        // To prevent accidental back navigation
                        NavigationService.FrameFacade.BackRequested += FrameFacadeBackRequested;
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
            }

            base.OnPageNavigatedTo(e);
        }

        public override void OnPageNavigatedFrom(NavigationEventArgs e)
        {
            NavigationService.FrameFacade.BackRequested -= FrameFacadeBackRequested;
            base.OnPageNavigatedFrom(e);
        }

        public override void OnPageNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnPageNavigatingFrom(e);
        }

        private async void FrameFacadeBackRequested(object sender, HandledEventArgs e)
        {
            e.Handled = IsSelectedContributionDirty;

            if (IsSelectedContributionDirty)
            {
                var md = new MessageDialog("Navigating away now will lose your changes, continue?", "Warning: Unsaved Changes");
                md.Commands.Add(new UICommand("yes"));
                md.Commands.Add(new UICommand("no"));
                md.CancelCommandIndex = 1;
                md.DefaultCommandIndex = 1;

                var result = await md.ShowAsync();

                if (result.Label == "yes")
                {
                    if (NavigationService.CanGoBack)
                    {
                        NavigationService.GoBack();
                    }
                }
            }
        }

        #endregion
    }
}