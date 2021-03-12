using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel;
using Microsoft.Toolkit.Uwp.Connectivity;
using MvpApi.Common.Models;
//using MvpCompanion.UI.Extensions;
using CommonHelpers.Common;
using CommonHelpers.Mvvm;
using System.ComponentModel;
using MvpApi.Common.Extensions;
using MvpCompanion.UI.Wpf.Helpers;
using MvpCompanion.UI.Wpf.Views;

namespace MvpCompanion.UI.Wpf.ViewModels
{
    public class ContributionEditorDialogViewModel : ViewModelBase
    {
        #region Fields

        private ContributionsModel _originalContribution;
        private ContributionsModel _selectedContribution;
        private string _urlHeader = "Url";
        private string _annualQuantityHeader = "Annual Quantity";
        private string _secondAnnualQuantityHeader = "Second Annual Quantity";
        private string _annualReachHeader = "Annual Reach";
        private bool _isUrlRequired;
        private bool _isAnnualQuantityRequired;
        private bool _isSecondAnnualQuantityRequired;
        private bool _isAnnualReachRequired;
        private bool _canSave = true;
        private string _warningMessage;
        private bool _isBusy;
        private string _isBusyMessage;
        private bool _editingExistingContribution;

        #endregion

        public ContributionEditorDialogViewModel()
        {
            if (DesignMode.DesignModeEnabled || DesignMode.DesignMode2Enabled)
            {
                Types = DesignTimeHelpers.GenerateContributionTypes();
                Visibilities = DesignTimeHelpers.GenerateVisibilities();
                UploadQueue = DesignTimeHelpers.GenerateContributions();
                SelectedContribution = UploadQueue.FirstOrDefault();
            }
            
            RemoveAdditionalTechAreaCommand = new DelegateCommand<ContributionTechnologyModel>(RemoveAdditionalArea);
        }

        #region Properties

        // Collections and SelectedItems

        public ObservableCollection<ContributionsModel> UploadQueue { get; } = new ObservableCollection<ContributionsModel>();

        public ObservableCollection<ContributionTypeModel> Types { get; } = new ObservableCollection<ContributionTypeModel>();

        public ObservableCollection<VisibilityViewModel> Visibilities { get; } = new ObservableCollection<VisibilityViewModel>();

        public ObservableCollection<ContributionAreaContributionModel> CategoryAreas { get; } = new ObservableCollection<ContributionAreaContributionModel>();

        public ContributionsModel SelectedContribution
        {
            get => _selectedContribution;
            set => SetProperty(ref _selectedContribution, value);
        }

        // Data entry control headers, using VM properties to alert validation violations

        public string AnnualQuantityHeader
        {
            get => _annualQuantityHeader;
            set => SetProperty(ref _annualQuantityHeader, value);
        }

        public string SecondAnnualQuantityHeader
        {
            get => _secondAnnualQuantityHeader;
            set => SetProperty(ref _secondAnnualQuantityHeader, value);
        }

        public string AnnualReachHeader
        {
            get => _annualReachHeader;
            set => SetProperty(ref _annualReachHeader, value);
        }

        public string UrlHeader
        {
            get => _urlHeader;
            set => SetProperty(ref _urlHeader, value);
        }

        public bool IsUrlRequired
        {
            get => _isUrlRequired;
            set => SetProperty(ref _isUrlRequired, value);
        }

        public bool IsAnnualQuantityRequired
        {
            get => _isAnnualQuantityRequired;
            set => SetProperty(ref _isAnnualQuantityRequired, value);
        }

        public bool IsSecondAnnualQuantityRequired
        {
            get => _isSecondAnnualQuantityRequired;
            set => SetProperty(ref _isSecondAnnualQuantityRequired, value);
        }

        public bool IsAnnualReachRequired
        {
            get => _isAnnualReachRequired;
            set => SetProperty(ref _isAnnualReachRequired, value);
        }

        public bool CanSave
        {
            get => _canSave;
            set => SetProperty(ref _canSave, value);
        }

        public string WarningMessage
        {
            get => _warningMessage;
            set => SetProperty(ref _warningMessage, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public string IsBusyMessage
        {
            get => _isBusyMessage;
            set => SetProperty(ref _isBusyMessage, value);
        }
        
        public bool EditingExistingContribution
        {
            get => _editingExistingContribution;
            set => SetProperty(ref _editingExistingContribution, value);
        }

        // Commands

        public DelegateCommand<ContributionTechnologyModel> RemoveAdditionalTechAreaCommand { get; set; }
        
        // Methods

        public void DatePicker_OnDateChanged(object sender, DatePickerValueChangedEventArgs e)
        {
            if (e.NewDate < (ShellPage.Instance.DataContext as ShellViewModel).SubmissionStartDate || e.NewDate > (ShellPage.Instance.DataContext as ShellViewModel).SubmissionDeadline)
            {
                WarningMessage = "The contribution date must be after the start of your current award period and before March 31st in order for it to count towards your evaluation";
            }
            else
            {
                WarningMessage = "";
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
            
            // Manually find the flyout's popup to close it
            // TODO fix
            //var lv = sender as ListView;
            //var foPresenter = lv?.Parent as FlyoutPresenter;
            //var popup = foPresenter?.Parent as Popup;
            //popup?.Hide();
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

        // TODO fix
        public async Task OnDialogLoadedAsync()
        {
            if (!NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
            {
                WarningMessage = "No Internet Available";
                return;
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

                        // If the contribution object wasn't passed during Dialog creation, setup a blank one.
                        if (SelectedContribution == null)
                        {
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

                        // TODO prevent accidental back navigation
                        //if (BootStrapper.Current.NavigationService.FrameFacade != null)
                        //{
                        //    BootStrapper.Current.NavigationService.FrameFacade.BackRequested += FrameFacade_BackRequested;
                        //}
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"AddContributions OnNavigatedToAsync Exception {ex}");
                        await ex.LogExceptionAsync();
                    }
                    finally
                    {
                        IsBusyMessage = "";
                        IsBusy = false;
                    }
                }
            }
        }

        public void OnDialogClosingAsync()
        {
            //if (BootStrapper.Current.NavigationService.FrameFacade != null)
            //{
            //    BootStrapper.Current.NavigationService.FrameFacade.BackRequested -= FrameFacade_BackRequested;
            //}
        }

        private async void FrameFacade_BackRequested(object sender, HandledEventArgs e)
        {
            try
            {
                var md = new MessageDialog("Navigating away will lose any pending edits, continue?", "Close Editor?");
                md.Commands.Add(new UICommand("yes"));
                md.Commands.Add(new UICommand("no"));
                md.CancelCommandIndex = 1;
                md.DefaultCommandIndex = 1;

                var result = await md.ShowAsync();

                // If the user clicked no, then make the back request as handled to prevent closure of dialog
                e.Handled = result.Label != "yes";
            }
            catch (Exception ex)
            {
                await ex.LogExceptionAsync();
            }
        }

        #endregion
    }
}
