using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using MvpApi.Common.Annotations;
using MvpApi.Common.Models;
using MvpApi.Uwp.Extensions;
using MvpApi.Uwp.Helpers;
using Template10.Utils;

namespace MvpApi.Uwp.Dialogs
{
    public sealed partial class EditActivityDialog : ContentDialog, INotifyPropertyChanged
    {
        #region Fields

        private ContributionsModel selectedContribution;
        private bool canSave = true;

        #endregion

        public EditActivityDialog()
        {
            InitializeComponent();

            if (DesignMode.DesignModeEnabled || DesignMode.DesignMode2Enabled)
            {
                Types = DesignTimeHelpers.GenerateContributionTypes();
                Visibilities = DesignTimeHelpers.GenerateVisibilities();

                return;
            }

            Loaded += EditActivityDialog_Loaded;

        }

        #region Properties

        // Collections and SelectedItems

        public ObservableCollection<ContributionTypeModel> Types { get; } = new ObservableCollection<ContributionTypeModel>();

        public ObservableCollection<VisibilityViewModel> Visibilities { get; } = new ObservableCollection<VisibilityViewModel>();

        public ObservableCollection<ContributionAreaContributionModel> CategoryAreas { get; } = new ObservableCollection<ContributionAreaContributionModel>();

        public ContributionsModel SelectedContribution
        {
            get => selectedContribution;
            set => Set(ref selectedContribution, value);
        }
        
        public bool CanSave
        {
            get => canSave;
            set => Set(ref canSave, value);
        }

        #endregion

        private async void EditActivityDialog_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadSourceData();

            // Set up the model's initial values
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

            // Selections based on controls

            SelectedAdditionalTechnologiesListView.ItemsSource = SelectedContribution.AdditionalTechnologies;

            ContributionTypeComboBox.SelectedItem = SelectedContribution.ContributionType;
            AreasComboBox.SelectedItem = SelectedContribution.ContributionTechnology;
            StartDatePicker.SelectedDate = SelectedContribution.StartDate;
            TitleTextBox.Text = SelectedContribution.Title;
            UrlTextBox.Text = SelectedContribution.ReferenceUrl;
            DescriptionTextBox.Text = SelectedContribution.Description;

            AnnualQuantityNumericBox.Value = SelectedContribution.AnnualQuantity;
            SecondAnnualQuantityNumericBox.Value = SelectedContribution.SecondAnnualQuantity;
            AnnualReachNumericBox.Value = SelectedContribution.AnnualReach;

            ToggleIsBusy("");
        }

        private async void SaveButton_OnClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var deferral = args.GetDeferral();

            try
            {
                SelectedContribution.ContributionType = ContributionTypeComboBox.SelectionBoxItem as ContributionTypeModel;
                SelectedContribution.ContributionTypeName = (ContributionTypeComboBox.SelectionBoxItem as ContributionTypeModel)?.Name;
                SelectedContribution.ContributionTechnology = AreasComboBox.SelectedItem as ContributionTechnologyModel;

                // Already set upon loading, items are added or removed during selection
                // SelectedContribution.AdditionalTechnologies = SelectedAdditionalTechnologiesListView.ItemsSource

                SelectedContribution.Title = TitleTextBox.Text;
                SelectedContribution.StartDate = Convert.ToDateTime(StartDatePicker.SelectedDate);
                SelectedContribution.ReferenceUrl = UrlTextBox.Text;
                SelectedContribution.Description = DescriptionTextBox.Text;

                SelectedContribution.AnnualQuantity = (int?)AnnualQuantityNumericBox.Value;
                SelectedContribution.SecondAnnualQuantity = (int?)SecondAnnualQuantityNumericBox.Value;
                SelectedContribution.AnnualReach = (int?)AnnualReachNumericBox.Value;

                var isValid = await SelectedContribution.Validate(true);

                if (isValid)
                {
                    this.Hide();
                }
                else
                {
                    args.Cancel = true;
                }

            }
            catch
            {
                args.Cancel = true;
            }
            finally
            {
                deferral.Complete();
            }
        }

        private void CancelButton_OnClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            SelectedContribution = null;
            this.Hide();
        }

        #region Event handlers
        
        public async void DatePicker_OnDateChanged(object sender, DatePickerValueChangedEventArgs e)
        {
            if (e.NewDate < new DateTime(2016, 10, 1) || e.NewDate > new DateTime(2019, 4, 1))
            {
                await new MessageDialog("The contribution date must be after the start of your current award period and before April 1st, 2019 in order for it to count towards your evaluation", "Notice: Out of range").ShowAsync();

                WarningGrid.Visibility = Visibility.Visible;
                WarningTextBlock.Text = "The contribution date must be after the start of your current award period and before March 31, 2019 in order for it to count towards your evaluation";
                
                CanSave = false;
            }
            else
            {
                WarningGrid.Visibility = Visibility.Collapsed;
                WarningTextBlock.Text = "";

                CanSave = true;
            }
        }

        // There are complex rules around the field names and what's required, this updates the UI accordingly.
        public void ActivityType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.FirstOrDefault() is ContributionTypeModel type)
            {
                DetermineContributionTypeRequirements(type);
            }
        }

        public async void AdditionalTechnologiesListView_OnItemClick(object sender, ItemClickEventArgs e)
        {
            if (SelectedContribution.AdditionalTechnologies.Count < 2)
            {
                if (e.ClickedItem is ContributionTechnologyModel tech)
                {
                    if (!SelectedContribution.AdditionalTechnologies.Contains(tech))
                    {
                        SelectedContribution.AdditionalTechnologies.Add(tech);
                    }
                }
            }
            else
            {
                await new MessageDialog("You can only have two additional areas selected, remove one and try again.").ShowAsync();
            }

            AddTechnologyButton.Visibility = SelectedContribution.AdditionalTechnologies.Count >= 2 ? Visibility.Collapsed : Visibility.Visible;
        }

        private void RemoveAdditionalTechnologyButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is ContributionTechnologyModel area)
            {
                if (SelectedContribution.AdditionalTechnologies.Contains(area))
                {
                    SelectedContribution.AdditionalTechnologies.Remove(area);
                }
            }

            AddTechnologyButton.Visibility = SelectedContribution.AdditionalTechnologies.Count >= 2 ? Visibility.Collapsed : Visibility.Visible;
        }
        

        #endregion

        #region Methods

        private async Task LoadSourceData()
        {
            ToggleIsBusy("loading types...");

            var types = await App.ApiService.GetContributionTypesAsync();
            types.ForEach(type =>
            {
                Types.Add(type);
            });

            ContributionTypeComboBox.ItemsSource = Types;

            ToggleIsBusy("loading technologies...");

            var areaRoots = await App.ApiService.GetContributionAreasAsync();

            // Flatten out the result so that we only have a single level of grouped data, this is used for the CollectionViewSource, defined in the XAML.
            var areas = areaRoots.SelectMany(areaRoot => areaRoot.Contributions);
            areas.ForEach(area =>
            {
                CategoryAreas.Add(area);
            });

            // This sets the ItemsSource for AreasComboBox and AdditionalTechnologiesListView
            awardCategoriesCvs.Source = CategoryAreas;
            awardCategoriesCvs.ItemsPath = new PropertyPath("ContributionAreas");
            awardCategoriesCvs.IsSourceGrouped = true;

            ToggleIsBusy("loading visibility options...");

            var visibilities = await App.ApiService.GetVisibilitiesAsync();
            visibilities.ForEach(visibility =>
            {
                Visibilities.Add(visibility);
            });

            VisibilitiesComboBox.ItemsSource = Visibilities;
        }

        private void ToggleIsBusy(string message)
        {
            BusyIndicator.Content = message;

            if (string.IsNullOrEmpty(message))
            {
                BusyIndicator.IsActive = false;
                BusyIndicator.Visibility = Visibility.Collapsed;
            }
            else
            {
                BusyIndicator.IsActive = true;
                BusyIndicator.Visibility = Visibility.Visible;
            }
        }

        public void DetermineContributionTypeRequirements(ContributionTypeModel contributionType)
        {
            // Each activity type has a unique set of field names and which ones are required.
            // This extension method will parse it and return a Tuple of the unqie requirements.
            var contributionTypeRequirements = contributionType.GetContributionTypeRequirements();

            // Set the headers of the input boxes
            AnnualQuantityHeaderTextBlock.Text = contributionTypeRequirements.Item1;
            SecondAnnualQuantityHeaderTextBlock.Text = contributionTypeRequirements.Item2;
            AnnualReachHeaderTextBlock.Text = contributionTypeRequirements.Item3;
            
            // Determine the required fields for upload.
            UrlRequiredTextBlock.Visibility = contributionTypeRequirements.Item4 
                ? Visibility.Visible 
                : Visibility.Collapsed;

            AnnualQuantityNumericBox.Visibility = string.IsNullOrEmpty(contributionTypeRequirements.Item1) 
                ? Visibility.Collapsed 
                : Visibility.Visible;

            SecondAnnualQuantityNumericBox.Visibility = !string.IsNullOrEmpty(contributionTypeRequirements.Item2) 
                ? Visibility.Collapsed 
                : Visibility.Visible;
            
            AnnualReachNumericBox.Visibility = !string.IsNullOrEmpty(contributionTypeRequirements.Item3) 
                ? Visibility.Collapsed 
                : Visibility.Visible;
        }
        
        #endregion

        #region INPC

        private bool Set<T>(
            ref T backingStore, T value,
            [CallerMemberName]string propertyName = "",
            Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;

            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
