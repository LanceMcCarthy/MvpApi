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

namespace MvpApi.Uwp.ViewModels
{
    public class ContributionDetailViewModel : PageViewModelBase
    {
        private ContributionsModel activeContribution;
        private bool isDirty;
        private ContributionAreasRootItem selectedContributionAreaAwardCategory;
        private ContributionAreaContributionModel selectedContributionAreaContributionModel;
        private ContributionAreaModel selectedContributionAreaModel;
        private bool isContributionTypeEditable = true;

        public ContributionDetailViewModel()
        {
            
        }
        public ContributionsModel ActiveContribution
        {
            get => activeContribution;
            set => Set(ref activeContribution, value);
        }
        
        public ObservableCollection<ContributionTypeModel> ContributionTypes { get; set; } = new ObservableCollection<ContributionTypeModel>();

        public ObservableCollection<VisibilityViewModel> ContributionVisibilies { get; set; } = new ObservableCollection<VisibilityViewModel>();

        // Cascading
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

        #region Event handlers

        public void TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            CheckIfDirty();
        }
        
        public void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CheckIfDirty();
        }

        public void DatePicker_OnDateChanged(object sender, DatePickerValueChangedEventArgs e)
        {
            CheckIfDirty();
        }

        public void RadNumericBox_OnValueChanged(object sender, EventArgs e)
        {
            CheckIfDirty();
        }

        #endregion

        public async void UploadContributionsButton_Click(object sender, RoutedEventArgs e)
        {
            var md = new MessageDialog("Upload and save this contribution?");

            ContributionsModel result = null;

            md.Commands.Add(new UICommand("save", async (args) =>
            {
                // TODO see if changing int type in the model will work, they have the same properties, so it *should* work fine
                ActiveContribution.ContributionTechnology = new ContributionTechnologyModel()
                {
                    Id = SelectedContributionAreaModel.Id,
                    AwardCategory = SelectedContributionAreaModel.AwardCategory,
                    AwardName = SelectedContributionAreaModel.AwardName,
                    Name = SelectedContributionAreaModel.Name
                };
                
                result = await App.ApiService.SubmitContributionAsync(ActiveContribution);
            }));

            md.Commands.Add(new UICommand("cancel"));

            await md.ShowAsync();
            
            if (result == null)
            {
                await new MessageDialog("Something went wrong saving the item, please try again").ShowAsync();
                return;
            }

            var successDialog = new MessageDialog("Would you like to start a new contribution?", "Success!");

            successDialog.Commands.Add(new UICommand("yes", args =>
            {
                ActiveContribution = new ContributionsModel();
            }));

            successDialog.Commands.Add(new UICommand("no", args =>
            {
                ActiveContribution = result;
            }));

            await successDialog.ShowAsync();
        }
        
        private void CheckIfDirty()
        {

        }

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

                    
                    IsBusyMessage = "getting sharing preferences...";

                    foreach (var visibility in await App.ApiService.GetVisibilitiesAsync())
                    {
                        ContributionVisibilies.Add(visibility);
                    }


                    // ** Determine if we're editing an existing contribution or creating a new one **

                    if (parameter is ContributionsModel param)
                    {
                        ActiveContribution = param;
                        IsContributionTypeEditable = false;
                    }
                    else
                    {
                        ActiveContribution = new ContributionsModel();

                        // -- TESTING ONLY -- 
                        ActiveContribution.Title = "Test Upload";
                        ActiveContribution.Description = "This is a test contribution upload from the UWP application I'm building for the MVP community to help them submit their 2018 contributions.";
                        ActiveContribution.StartDate = DateTime.Now;
                        ActiveContribution.ReferenceUrl = "lancemccarthy.com";
                        
                        ActiveContribution.Visibility = ContributionVisibilies.FirstOrDefault();

                        // Should only be available for setting when 
                        ActiveContribution.ContributionType = ContributionTypes.FirstOrDefault();

                        // This is for when in edit mode (you cant change ContributionType after its been submiteed
                        ActiveContribution.ContributionTypeName = ActiveContribution?.ContributionType?.Name;

                        ActiveContribution.AnnualQuantity = 0;
                        ActiveContribution.SecondAnnualQuantity = 0;
                        ActiveContribution.AnnualReach = 0;
                        
                        IsDirty = true;
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
