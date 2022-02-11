using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation.Metadata;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Microsoft.Services.Store.Engagement;
using Microsoft.Toolkit.Uwp.Connectivity;
using MvpApi.Common.Models;
using MvpCompanion.UI.Common.Extensions;
using MvpCompanion.UI.Common.Helpers;
using MvpApi.Uwp.Views;
using Template10.Mvvm;
using Template10.Services.PopupService;
using Template10.Utils;

namespace MvpApi.Uwp.ViewModels
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
            set => Set(ref _selectedContribution, value);
        }

        // Data entry control headers, using VM properties to alert validation violations

        public string AnnualQuantityHeader
        {
            get => _annualQuantityHeader;
            set => Set(ref _annualQuantityHeader, value);
        }

        public string SecondAnnualQuantityHeader
        {
            get => _secondAnnualQuantityHeader;
            set => Set(ref _secondAnnualQuantityHeader, value);
        }

        public string AnnualReachHeader
        {
            get => _annualReachHeader;
            set => Set(ref _annualReachHeader, value);
        }

        public string UrlHeader
        {
            get => _urlHeader;
            set => Set(ref _urlHeader, value);
        }

        public bool IsUrlRequired
        {
            get => _isUrlRequired;
            set => Set(ref _isUrlRequired, value);
        }

        public bool IsAnnualQuantityRequired
        {
            get => _isAnnualQuantityRequired;
            set => Set(ref _isAnnualQuantityRequired, value);
        }

        public bool IsSecondAnnualQuantityRequired
        {
            get => _isSecondAnnualQuantityRequired;
            set => Set(ref _isSecondAnnualQuantityRequired, value);
        }

        public bool IsAnnualReachRequired
        {
            get => _isAnnualReachRequired;
            set => Set(ref _isAnnualReachRequired, value);
        }

        public bool CanSave
        {
            get => _canSave;
            set => Set(ref _canSave, value);
        }

        public string WarningMessage
        {
            get => _warningMessage;
            set => Set(ref _warningMessage, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => Set(ref _isBusy, value);
        }

        public string IsBusyMessage
        {
            get => _isBusyMessage;
            set => Set(ref _isBusyMessage, value);
        }
        
        public bool EditingExistingContribution
        {
            get => _editingExistingContribution;
            set => Set(ref _editingExistingContribution, value);
        }

        #endregion

        #region Commands

        // Commands

        public DelegateCommand<ContributionTechnologyModel> RemoveAdditionalTechAreaCommand { get; set; }

        #endregion

        #region Event Handlers

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
            var lv = sender as ListView;
            var foPresenter = lv?.Parent as FlyoutPresenter;
            var popup = foPresenter?.Parent as Popup;
            popup?.Hide();
        }

        #endregion

        #region Tasks

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

                        types.ForEach(type =>
                        {
                            Types.Add(type);
                        });

                        IsBusyMessage = "loading technologies...";

                        var areaRoots = await App.ApiService.GetContributionAreasAsync();

                        // Flatten out the result so that we only have a single level of grouped data, this is used for the CollectionViewSource, defined in the XAML.
                        var areas = areaRoots.SelectMany(areaRoot => areaRoot.Contributions);

                        areas.ForEach(area =>
                        {
                            CategoryAreas.Add(area);
                        });

                        // TODO Try and get the CollectionViewSource to invoke now so that the LoadNextEntry will be able to preselected award category.

                        IsBusyMessage = "loading visibility options...";

                        var visibilities = await App.ApiService.GetVisibilitiesAsync();

                        visibilities.ForEach(visibility =>
                        {
                            Visibilities.Add(visibility);
                        });

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

        }
        
        #endregion
    }
}
