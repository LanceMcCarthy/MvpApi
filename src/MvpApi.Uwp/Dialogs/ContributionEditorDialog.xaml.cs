using Microsoft.Toolkit.Uwp.Connectivity;
using MvpApi.Common.Models;
using MvpApi.Uwp.ViewModels;
using MvpApi.Uwp.Views;
using MvpCompanion.UI.Common.Extensions;
using MvpCompanion.UI.Common.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Telerik.UI.Xaml.Controls.Input;
using Template10.Services.PopupService;
using Template10.Utils;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace MvpApi.Uwp.Dialogs
{
    public sealed partial class ContributionEditorDialog : ContentDialog
    {
        #region fields

        private ObservableCollection<ContributionTypeModel> _contTypes;
        private ObservableCollection<VisibilityViewModel> _contVisibilities;
        private ObservableCollection<ContributionAreaContributionModel> _contAreas;
        private CollectionViewSource _contTechnologyCvs;

        #endregion

        #region Dependency properties

        public static readonly DependencyProperty ContributionResultProperty = DependencyProperty.Register(
            "ContributionResult",
            typeof(ContributionsModel),
            typeof(ContributionEditorDialog),
            new PropertyMetadata(default(ContributionsModel)));

        public ContributionsModel ContributionResult
        {
            get => (ContributionsModel)GetValue(ContributionResultProperty);
            set => SetValue(ContributionResultProperty, value);
        }

        #endregion

        #region Constructors

        public ContributionEditorDialog(ContributionsModel originalContribution, bool cloneContribution = false)
        {
            if (originalContribution == null)
                throw new ArgumentNullException(nameof(originalContribution), "You need to use an originating contribution in order to use the ContributionEditorDialog for either editing or cloning.");

            InitializeComponent();

            ViewModel.IsCloningContribution = cloneContribution;

            // If we are cloning, make a new copy of the object to prevent making changes to the original reference
            ViewModel.Contribution = cloneContribution
                ? originalContribution.Clone(stripContributionId: true)
                : originalContribution;

            // Show the correct text for the buttons and subscribe to the clicked event
            PrimaryButtonText = cloneContribution ? "Save" : "Update";
            SecondaryButtonText = "Cancel";
            PrimaryButtonClick += SaveButton_OnClick;
            SecondaryButtonClick += CancelButton_OnClick;

            Loaded += ContributionEditorDialog_Loaded;
            Unloaded += ContributionEditorDialog_Unloaded;
        }

        #endregion

        #region Event handlers

        private async void ContributionEditorDialog_Loaded(object sender, RoutedEventArgs e)
        {
            // Phase 1 - internet and login check

            if (!NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
            {
                ViewModel.HeaderMessage = "No Internet Available";
                return;
            }

            if (ShellPage.Instance.DataContext is ShellViewModel shellVm)
            {
                // Verify the user is logged in
                if (!shellVm.IsLoggedIn)
                {
                    ViewModel.IsBusy = true;
                    ViewModel.IsBusyMessage = "logging in...";

                    await ShellPage.Instance.SignInAsync();

                    ViewModel.IsBusyMessage = "";
                    ViewModel.IsBusy = false;

                    // They still can't login, close the dialog window.
                    if (!shellVm.IsLoggedIn)
                    {
                        Hide();
                    }
                }
            }

            // Phase 2 - configure for edit or clone

            if (ViewModel.IsCloningContribution)
            {
                // Strip the the original contribution's ID to prevent accidentally overwriting an existing contribution
                ViewModel.Contribution.ContributionId = null;

                HeaderMessageGrid.Background = new SolidColorBrush(Colors.Goldenrod);
                ViewModel.HeaderMessage = "Cloning Contribution";
            }
            else
            {
                HeaderMessageGrid.Background = new SolidColorBrush(Colors.DarkSlateGray);
                ViewModel.HeaderMessage = "Editing Contribution";

                // Hide contributionType comboBox because you cannot edit the type after it has been submitted
                ContributionTypeComboBox.Visibility = Visibility.Collapsed;
            }

            // Phase 3 - Load data for combobox ItemsSource and set selected

            await LoadDataSourcesAsync();

            // Phase 4 - Set all the selected values

            await AssignSelectedValuesAsync();
        }

        private void ContributionEditorDialog_Unloaded(object sender, RoutedEventArgs e)
        {
            // Unsubscribe manually added handlers
            
            ContributionTypeComboBox.SelectionChanged -= ContributionTypeComboBox_SelectionChanged;
            ContributionTechnologyComboBox.SelectionChanged -= ContributionTechnologyComboBox_SelectionChanged;
            AvailableTechnologiesListView.ItemClick -= AvailableTechnologiesListView_ItemClick;
            StartDatePicker.SelectedDateChanged -= StartDatePicker_SelectedDateChanged;
            TitleTextBox.TextChanged -= TitleTextBox_TextChanged;
            DescriptionTextBox.TextChanged -= DescriptionTextBox_TextChanged;
            UrlTextBox.TextChanged -= UrlTextBox_TextChanged;
            AnnualQuantityNumericBox.ValueChanged -= AnnualQuantityNumericBox_ValueChanged;
            SecondAnnualQuantityNumericBox.ValueChanged -= SecondAnnualQuantityNumericBox_ValueChanged;
            AnnualReachNumericBox.ValueChanged -= AnnualReachNumericBox_ValueChanged;
            VisibilitiesComboBox.SelectionChanged -= VisibilitiesComboBox_SelectionChanged;
        }

        private async void SaveButton_OnClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var deferral = args.GetDeferral();

            try
            {
                IsPrimaryButtonEnabled = false;

                var saveSucceeded = false;

                ContributionResult = null;

                var isValid = await ViewModel.Contribution.Validate(true);

                if (isValid)
                {
                    ViewModel.Contribution.UploadStatus = UploadStatus.InProgress;

                    if (ViewModel.IsCloningContribution)
                    {
                        // Submit a new contribution
                        var newSubmissionResult = await App.ApiService.SubmitContributionAsync(ViewModel.Contribution);

                        if (newSubmissionResult != null)
                        {
                            // IMPORTANT = Uploading a new contribution returns the saved submission, which comes with an ID
                            ContributionResult = newSubmissionResult;

                            // copying back the ID which was created on the server once the item was added to the database
                            ViewModel.Contribution.ContributionId = newSubmissionResult.ContributionId;

                            saveSucceeded = true;
                        }
                    }
                    else
                    {
                        // Update an existing contribution
                        var contributionUpdated = await App.ApiService.UpdateContributionAsync(ViewModel.Contribution);

                        if (contributionUpdated == true)
                        {
                            saveSucceeded = true;
                            ContributionResult = ViewModel.Contribution;
                        }
                    }

                    // Show the result in the UI-bound object
                    ViewModel.Contribution.UploadStatus = saveSucceeded ? UploadStatus.Success : UploadStatus.Failed;

                    if (saveSucceeded)
                    {
                        Hide();
                    }
                    else
                    {
                        args.Cancel = true;
                    }
                }
                else
                {
                    // prevent the closing of the dialog
                    args.Cancel = true;

                    await new MessageDialog("Check for errors or missing data and try again.", "Invalid Contribution").ShowAsync();
                }
            }
            catch (Exception ex)
            {
                // prevent the closing of the dialog
                args.Cancel = true;

                await new MessageDialog("Something went wrong saving the contribution.", "Error").ShowAsync();

            }
            finally
            {
                deferral.Complete();
            }
        }

        private async void CancelButton_OnClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (ViewModel.Contribution.UploadStatus == UploadStatus.InProgress)
            {
                await new MessageDialog(
                    "You cannot cancel an edit while submission is in progress.\r\n\n" +
                    "If you did not intend to submit it, you can either edit or delete it after this operation is complete.",
                    "In Progress").ShowAsync();

                return;
            }

            ContributionResult = null;

            Hide();
        }

        private void ContributionTechnologyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.FirstOrDefault() is ContributionTechnologyModel tech)
            {
                ViewModel.Contribution.ContributionTechnology = tech;
            }
        }

        private void ContributionTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.FirstOrDefault() is ContributionTypeModel type)
            {
                // There are complex rules around the names of the properties, this method determines the requirements and updates the UI accordingly
                UpdateAnnualNumericalRequirements(type);

                ViewModel.Contribution.ContributionType = type;

                // Also need set the type name
                ViewModel.Contribution.ContributionTypeName = type.EnglishName;
            }
        }

        private void AvailableTechnologiesListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            // Add the selected AdditionalTechnology
            if (ViewModel.Contribution.AdditionalTechnologies.Count < 2)
            {
                AddAdditionalArea(e.ClickedItem as ContributionTechnologyModel);
            }

            // Show or hide the Add button based on the total selected
            AddButton.Visibility = ViewModel.Contribution.AdditionalTechnologies.Count < 2
                ? Visibility.Visible
                : Visibility.Collapsed;

            // Manually find the ComboBox popup to close it
            if (AvailableTechnologiesListView.Parent is FlyoutPresenter presenter && presenter.Parent is Popup popup)
            {
                popup.Hide();
            }
        }

        private void RemoveAdditionalTechAreaButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is ContributionTechnologyModel area)
            {
                if (ViewModel.Contribution.AdditionalTechnologies.Contains(area))
                {
                    ViewModel.Contribution.AdditionalTechnologies.Remove(area);

                    // If the Add button was hidden, we need to show it again
                    if (AddButton.Visibility == Visibility.Collapsed)
                    {
                        AddButton.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        private void StartDatePicker_SelectedDateChanged(DatePicker sender, DatePickerSelectedValueChangedEventArgs args)
        {
            if (args.NewDate != null && ShellPage.Instance.DataContext is ShellViewModel shellViewModel)
            {
                if (args.NewDate < shellViewModel.SubmissionStartDate || args.NewDate > shellViewModel.SubmissionDeadline)
                {
                    DateValidationBorder.Visibility = Visibility.Visible;
                    
                    IsPrimaryButtonEnabled = ViewModel.CanSave = false;
                }
                else
                {
                    DateValidationBorder.Visibility = Visibility.Collapsed;

                    ViewModel.Contribution.StartDate = args.NewDate.Value.DateTime;
                    
                    IsPrimaryButtonEnabled = ViewModel.CanSave = true;
                }
            }
        }

        private void TitleTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ViewModel.Contribution.Title = ((TextBox)sender).Text;
        }

        private void DescriptionTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ViewModel.Contribution.Description = ((TextBox)sender).Text;
        }

        private void UrlTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ViewModel.Contribution.ReferenceUrl = ((TextBox)sender).Text;
        }

        private void AnnualQuantityNumericBox_ValueChanged(object sender, EventArgs e)
        {
            ViewModel.Contribution.AnnualQuantity = Convert.ToInt32(((RadNumericBox)sender).Value);
        }

        private void SecondAnnualQuantityNumericBox_ValueChanged(object sender, EventArgs e)
        {
            ViewModel.Contribution.SecondAnnualQuantity = Convert.ToInt32(((RadNumericBox)sender).Value);
        }

        private void AnnualReachNumericBox_ValueChanged(object sender, EventArgs e)
        {
            ViewModel.Contribution.AnnualReach = Convert.ToInt32(((RadNumericBox)sender).Value);
        }

        private void VisibilitiesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null && e.AddedItems.Count > 0)
            {
                var item = e.AddedItems.FirstOrDefault();

                if (item is VisibilityViewModel selectedVisibility)
                {
                    ViewModel.Contribution.Visibility = selectedVisibility;
                }
            }
        }

        #endregion

        #region Tasks

        private async Task LoadDataSourcesAsync()
        {
            try
            {
                ViewModel.IsBusy = true;

                // ********* Setup ContributionType ComboBox ********* //

                ViewModel.IsBusyMessage = "loading contribution types...";

                _contTypes = new ObservableCollection<ContributionTypeModel>();

                var types = await App.ApiService.GetContributionTypesAsync();
                types.ForEach(type => { _contTypes.Add(type); });

                ContributionTypeComboBox.ItemsSource = types;


                // ********* Setup ContributionTechnology ComboBox ********* //

                ViewModel.IsBusyMessage = "loading technologies...";

                var areaRoots = await App.ApiService.GetContributionAreasAsync();

                // Flatten out the result so that we only have a single level of grouped data, this is used for the CollectionViewSource, defined in the XAML.
                _contAreas = new ObservableCollection<ContributionAreaContributionModel>();

                var areas = areaRoots.SelectMany(areaRoot => areaRoot.Contributions);
                areas.ForEach(area => { _contAreas.Add(area); });

                _contTechnologyCvs = new CollectionViewSource
                {
                    Source = _contAreas,
                    IsSourceGrouped = true,
                    ItemsPath = new PropertyPath("ContributionAreas")
                };
                
                ContributionTechnologyComboBox.ItemsSource = _contTechnologyCvs;
                AvailableTechnologiesListView.ItemsSource = _contTechnologyCvs; // this is inside a popup


                // ********* Setup Visibilities ComboBox ********* //

                ViewModel.IsBusyMessage = "loading visibilities...";

                _contVisibilities = new ObservableCollection<VisibilityViewModel>();

                var visibilities = await App.ApiService.GetVisibilitiesAsync();
                visibilities.ForEach(visibility => { _contVisibilities.Add(visibility); });

                VisibilitiesComboBox.ItemsSource = _contVisibilities;
                
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ContributionEditorDialog LoadDataSourcesAsync Exception {ex}");
                await ex.LogExceptionAsync();
            }
            finally
            {
                ViewModel.IsBusyMessage = "";
                ViewModel.IsBusy = false;
            }
        }

        private async Task AssignSelectedValuesAsync()
        {
            try
            {
                // This setup follows the same order that the elements are defined in XAML

                ViewModel.IsBusy = true;
                ViewModel.IsBusyMessage = "setting selected values...";

                // ContributionType ComboBox

                ContributionTypeComboBox.SelectedItem = ViewModel.Contribution.ContributionType;
                ContributionTypeComboBox.SelectionChangedTrigger = ComboBoxSelectionChangedTrigger.Committed;
                ContributionTypeComboBox.SelectionChanged += ContributionTypeComboBox_SelectionChanged;

                // ContributionTechnology ComboBox

                ContributionTechnologyComboBox.SelectedItem = ViewModel.Contribution.ContributionTechnology;
                ContributionTechnologyComboBox.SelectionChangedTrigger = ComboBoxSelectionChangedTrigger.Committed;
                ContributionTechnologyComboBox.SelectionChanged += ContributionTechnologyComboBox_SelectionChanged;

                AvailableTechnologiesListView.ItemClick += AvailableTechnologiesListView_ItemClick;

                // SelectedAdditionalTechnologies ListView

                SelectedAdditionalTechnologiesListView.ItemsSource = ViewModel.Contribution.AdditionalTechnologies;

                // Hide the Add button based on number of SelectedAdditionalTechnologies

                AddButton.Visibility = ViewModel.Contribution.AdditionalTechnologies.Count < 2
                    ? Visibility.Visible
                    : Visibility.Collapsed;

                
                // StartDate DatePicker

                StartDatePicker.Date = ViewModel.Contribution.StartDate != null
                    ? new DateTimeOffset((DateTime)ViewModel.Contribution.StartDate)
                    : DateTimeOffset.Now;

                StartDatePicker.SelectedDateChanged += StartDatePicker_SelectedDateChanged;

                // Title TextBox

                TitleTextBox.Text = ViewModel.Contribution.Title;
                TitleTextBox.TextChanged += TitleTextBox_TextChanged;

                // Description TextBox

                DescriptionTextBox.Text = ViewModel.Contribution.Description;
                DescriptionTextBox.TextChanged += DescriptionTextBox_TextChanged;

                // URL TextBox

                UrlTextBox.Text = ViewModel.Contribution.Description;
                UrlTextBox.TextChanged += UrlTextBox_TextChanged;

                // Quantities panel items

                AnnualQuantityNumericBox.Minimum = 0;
                AnnualQuantityNumericBox.Maximum = int.MaxValue;
                AnnualQuantityNumericBox.Value = Convert.ToDouble(ViewModel.Contribution.AnnualQuantity);
                AnnualQuantityNumericBox.ValueChanged += AnnualQuantityNumericBox_ValueChanged;

                SecondAnnualQuantityNumericBox.Minimum = 0;
                SecondAnnualQuantityNumericBox.Maximum = int.MaxValue;
                SecondAnnualQuantityNumericBox.Value = Convert.ToDouble(ViewModel.Contribution.SecondAnnualQuantity);
                SecondAnnualQuantityNumericBox.ValueChanged += SecondAnnualQuantityNumericBox_ValueChanged;

                AnnualReachNumericBox.Minimum = 0;
                AnnualReachNumericBox.Maximum = int.MaxValue;
                AnnualReachNumericBox.Value = Convert.ToDouble(ViewModel.Contribution.AnnualReach);
                AnnualReachNumericBox.ValueChanged += AnnualReachNumericBox_ValueChanged;

                // Visibilities ComboBox
                VisibilitiesComboBox.SelectedItem = ViewModel.Contribution.Visibility;
                VisibilitiesComboBox.SelectionChangedTrigger = ComboBoxSelectionChangedTrigger.Committed;
                VisibilitiesComboBox.SelectionChanged += VisibilitiesComboBox_SelectionChanged;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ContributionEditorDialog AssignSelectedValues Exception {ex}");
                await ex.LogExceptionAsync();
            }
            finally
            {
                ViewModel.IsBusyMessage = "";
                ViewModel.IsBusy = false;
            }
        }

        public void UpdateAnnualNumericalRequirements(ContributionTypeModel contributionType)
        {
            // Each activity type has a unique set of field names and which ones are required.
            // This extension method will parse it and return a Tuple of the unique requirements.
            var contributionTypeRequirements = contributionType.GetContributionTypeRequirements();

            // Set the headers of the input boxes
            ViewModel.AnnualQuantityHeader = contributionTypeRequirements.Item1;
            ViewModel.SecondAnnualQuantityHeader = contributionTypeRequirements.Item2;
            ViewModel.AnnualReachHeader = contributionTypeRequirements.Item3;

            // Determine the required fields for upload.
            ViewModel.IsAnnualQuantityRequired = !string.IsNullOrEmpty(contributionTypeRequirements.Item1);
            ViewModel.IsSecondAnnualQuantityRequired = !string.IsNullOrEmpty(contributionTypeRequirements.Item2);
            ViewModel.IsAnnualReachRequired = !string.IsNullOrEmpty(contributionTypeRequirements.Item3);
            ViewModel.IsUrlRequired = contributionTypeRequirements.Item4;
        }

        private void AddAdditionalArea(ContributionTechnologyModel area)
        {
            if (!ViewModel.Contribution.AdditionalTechnologies.Contains(area))
            {
                ViewModel.Contribution.AdditionalTechnologies.Add(area);
            }
        }

        #endregion
    }
}