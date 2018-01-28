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
        private ContributionsModel contribution;
        private bool isDirty;

        public ContributionDetailViewModel()
        {
            
        }
        
        public ObservableCollection<ContributionTypeModel> ContributionTypes { get; set; } = new ObservableCollection<ContributionTypeModel>();

        public ObservableCollection<ContributionTechnologyModel> ContributionTechnologies { get; set; } = new ObservableCollection<ContributionTechnologyModel>();

        public ObservableCollection<VisibilityViewModel> ContributionVisibilies { get; set; } = new ObservableCollection<VisibilityViewModel>();

        public ContributionsModel Contribution
        {
            get => contribution;
            set => Set(ref contribution, value);
        }

        public bool IsDirty
        {
            get => isDirty;
            set => Set(ref isDirty, value);
        }

        #region Event handlers

        public void TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            CheckIfDirty();
        }

        public void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CheckIfDirty();

            if (e.AddedItems.Count <= 0)
                return;

            var item = e.AddedItems.FirstOrDefault();

            if (item is ContributionTypeModel type)
            {
                Contribution.ContributionTypeName = type.Name;
            }
        }

        public void DatePicker_OnDateChanged(object sender, DatePickerValueChangedEventArgs e)
        {
            CheckIfDirty();
            //Debug.WriteLine($"StartDate Changed: {e.NewDate.DateTime}");
            //Contribution.StartDate = e.NewDate.DateTime;
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
                result = await App.ApiService.SubmitContributionAsync(Contribution);
            }));

            md.Commands.Add(new UICommand("cancel"));

            await md.ShowAsync();
            
            if (result == null)
            {
                await new MessageDialog("Something went wrong saving the item, please try again").ShowAsync();
                return;
            }

            var successDialog = new MessageDialog("Would you like to start a new contribution?", "Success!");

            successDialog.Commands.Add(new UICommand("yes", async (args) =>
            {
                Contribution = new ContributionsModel();
            }));

            successDialog.Commands.Add(new UICommand("no", async (args) =>
            {
                Contribution = result;
            }));

            await successDialog.ShowAsync();

        }

        private async Task LoadDataAsync()
        {
            try
            {
                IsBusy = true;

                IsBusyMessage = "getting types...";

                foreach (var type in await App.ApiService.GetContributionTypesAsync())
                {
                    ContributionTypes.Add(type);
                }


                IsBusyMessage = "getting technologies...";
                
                foreach (var tech in await App.ApiService.GetContributionTechnologiesAsync())
                {
                    ContributionTechnologies.Add(tech);
                }
                

                IsBusyMessage = "getting sharing preferences...";

                foreach (var visibility in await App.ApiService.GetVisibilitiesAsync())
                {
                    ContributionVisibilies.Add(visibility);
                }

                Debug.WriteLine($"{ContributionVisibilies.Count} ContributionVisibilies loaded!");
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

        private void CheckIfDirty()
        {

        }

        #region Navigation

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (App.ShellPage.DataContext is ShellPageViewModel shellVm && shellVm.IsLoggedIn)
            {
                await LoadDataAsync();

                if (parameter is ContributionsModel selectedContribution)
                {
                    Contribution = selectedContribution;
                }
                else
                {
                    Contribution = new ContributionsModel();
                    Contribution.AnnualQuantity = 0;
                    Contribution.SecondAnnualQuantity = 0;
                    Contribution.AnnualReach = 0;

                    Contribution.Title = "Test Upload";
                    Contribution.Description = "This is a test contribution upload from the UWP application I'm building for the MVP community to help them submit their 2018 contributions.";
                    Contribution.StartDate = DateTime.Now;
                    Contribution.ContributionTechnology = ContributionTechnologies.FirstOrDefault();
                    Contribution.ContributionType = ContributionTypes.FirstOrDefault();
                    Contribution.ContributionTypeName = Contribution?.ContributionType?.Name;
                    Contribution.ReferenceUrl = "lancemccarthy.com";

                    IsDirty = true;
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
