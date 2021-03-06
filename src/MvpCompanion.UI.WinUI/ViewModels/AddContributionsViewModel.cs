﻿using CommonHelpers.Common;
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
    public class AddContributionsViewModel : PageViewModelBase
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
            if (DesignMode.DesignModeEnabled || DesignMode.DesignMode2Enabled)
            {
                Types = DesignTimeHelpers.GenerateContributionTypes();
                Visibilities = DesignTimeHelpers.GenerateVisibilities();
                UploadQueue = DesignTimeHelpers.GenerateContributions();
                SelectedContribution = UploadQueue.FirstOrDefault();

                return;
            }

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
        
        public void DatePicker_OnDateChanged(object sender, DatePickerValueChangedEventArgs e)
        {
            if (e.NewDate < (ShellPage.Instance.DataContext as ShellViewModel).SubmissionStartDate || e.NewDate > (ShellPage.Instance.DataContext as ShellViewModel).SubmissionDeadline)
            {
                WarningMessage = "The date must be after the start of your current award period and before March 31st of the next award year.";

                //await new MessageDialog(WarningMessage, "Notice: Out of range").ShowAsync();

                CanUpload = false;
            }
            else
            {
                WarningMessage = "";
                //CanUpload = true;
            }
        }
        
        public void ActivityType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.FirstOrDefault() is ContributionTypeModel type)
            {
                // There are complex rules around the names of the properties, this method determines the requirements and updates the UI accordingly
                DetermineContributionTypeRequirements(type);

                // Also need set the type name
                SelectedContribution.ContributionTypeName = type.EnglishName;
            }
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

        // Button click handlers
        public async void AddCurrentItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (!await SelectedContribution.Validate(true))
                return;
            
            if (IsEditingQueuedItem)
            {
                IsEditingQueuedItem = false;
            }
            else
            {
                if(!UploadQueue.Contains(SelectedContribution))
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

                UploadQueue.Clear();

                SetupNextEntry();
            }
            catch (Exception ex)
            {
                await new MessageDialog($"Something went wrong clearing the queue, please try again. Error: {ex.Message}").ShowAsync();
            }
        }

        public async void UploadQueue_Click(object sender, RoutedEventArgs e)
        {
            bool refreshNeeded = false;

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
                // If everything was uploaded, navigate away.
                if (BootStrapper.Current.NavigationService.CanGoBack)
                    BootStrapper.Current.NavigationService.GoBack();
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
                var md = new MessageDialog("Editing this contribution will replace the unsaved information you have in the form. If you do not want to lose that information, click 'cancel' and add it to the queue before editing this one.", "Warning");
                md.Commands.Add(new UICommand("edit"));
                md.Commands.Add(new UICommand("cancel"));

                var dialogResult = await md.ShowAsync();

                if (dialogResult.Label == "cancel")
                    return;
            }

            SelectedContribution = contribution;
            IsEditingQueuedItem = true;
        }

        public async Task RemoveContribution(ContributionsModel contribution)
        {
            if (contribution == null)
                return;

            try
            {
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
            }
            catch (Exception ex)
            {
                //await new MessageDialog($"Something went wrong deleting this item, please try again. Error: {ex.Message}").ShowAsync();
            }
        }

        public async Task<bool> UploadContributionAsync(ContributionsModel contribution)
        {
            try
            {
                var submissionResult = await App.ApiService.SubmitContributionAsync(contribution);

                // copying back the ID which was created on the server once the item was added to the database
                contribution.ContributionId = submissionResult.ContributionId;

                // Quality assurance, only logs a successful upload.
                //if (ApiInformation.IsTypePresent("Microsoft.Services.Store.Engagement.StoreServicesCustomEventLogger"))
                //    StoreServicesCustomEventLogger.GetDefault().Log("ContributionUploadSuccess");

                return true;
            }
            catch (Exception ex)
            {

                // Quality assurance, only logs a failed upload.
                //if (ApiInformation.IsTypePresent("Microsoft.Services.Store.Engagement.StoreServicesCustomEventLogger"))
                //    StoreServicesCustomEventLogger.GetDefault().Log("ContributionUploadFailure");

                //await new MessageDialog($"Something went wrong saving '{contribution.Title}', it will remain in the queue for you to try again.\r\n\nError: {ex.Message}").ShowAsync();
                return false;
            }
        }

        private async Task LoadSupportingDataAsync()
        {
            IsBusyMessage = "loading types...";

            var types = await App.ApiService.GetContributionTypesAsync();
            
            foreach (var type in types)
            {
                Types.Add(type);
            }

            IsBusyMessage = "loading technologies...";

            var areaRoots = await App.ApiService.GetContributionAreasAsync();

            // Flatten out the result so that we only have a single level of grouped data, this is used for the CollectionViewSource, defined in the XAML.
            var areas = areaRoots.SelectMany(areaRoot => areaRoot.Contributions);
            
            foreach (var area in areas)
            {
                CategoryAreas.Add(area);
            }

            // TODO Try and get the CollectionViewSource to invoke now so that the LoadNextEntry will be able to preselected award category.
            
            IsBusyMessage = "loading visibility options...";

            var visibilities = await App.ApiService.GetVisibilitiesAsync();
            
            foreach (var visibility in visibilities)
            {
                Visibilities.Add(visibility);
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

                        // ** Get the necessary associated data from the API **

                        await LoadSupportingDataAsync();

                        // Note: The Category Areas will not be loaded until the CollectionViewSource is does it's initially loading.
                        SetupNextEntry();

                        if (!(ApplicationData.Current.LocalSettings.Values["AddContributionPageTutorialShown"] is bool tutorialShown) || !tutorialShown)
                        {
                            var td = new TutorialDialog
                            {
                                SettingsKey = "AddContributionPageTutorialShown",
                                MessageTitle = "Add Contribution Page",
                                Message = "This page allows you to add contributions to your MVP profile.\r\n\n" +
                                          "- Complete the form and the click the 'Add' button to add the completed contribution to the upload queue.\r\n" +
                                          "- You can edit or remove items that are already in the queue using the item's 'Edit' or 'Remove' buttons.\r\n" +
                                          "- Click 'Upload' to save the queue to your profile.\r\n" +
                                          "- You can clear the form, or the entire queue, using the 'Clear' buttons.\r\n\n" +
                                          "TIP: Watch the queue items color change as the items are uploaded and save is confirmed."
                            };

                            await td.ShowAsync();
                        }

                        // To prevent accidental back navigation
                        if (BootStrapper.Current.NavigationService.FrameFacade != null)
                            BootStrapper.Current.NavigationService.FrameFacade.BackRequested += FrameFacadeBackRequested;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"AddContributions OnNavigatedToAsync Exception {ex}");
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
            base.OnPageNavigatedFrom(e);
        }

        public override void OnPageNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (BootStrapper.Current.NavigationService.FrameFacade != null)
                BootStrapper.Current.NavigationService.FrameFacade.BackRequested -= FrameFacadeBackRequested;
            
            base.OnPageNavigatingFrom(e);
        }

        // Prevent back key press. Credit Daren May https://github.com/Windows-XAML/Template10/issues/737
        private async void FrameFacadeBackRequested(object sender, HandledEventArgs e)
        {
            try
            {
                var itemsQueued = UploadQueue.Any();

                e.Handled = itemsQueued;

                if (itemsQueued)
                {
                    var md = new MessageDialog("Navigating away now will lose your pending uploads, continue?", "Warning: Pending Uploads");
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
            catch (Exception ex)
            {
                await ExceptionLogger.LogExceptionAsync(ex);
            }
        }

        #endregion
    }
}