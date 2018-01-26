using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using MvpApi.Common.Models;
using MvpApi.Uwp.Views;
using Telerik.Core.Data;
using Template10.Mvvm;

namespace MvpApi.Uwp.ViewModels
{
    public class HomePageViewModel : ViewModelBase
    {
        #region Fields
        
        private bool isBusy;
        private string isBusyMessage;
        private int? currentOffset = 0;
        private string displayTotal;

        #endregion

        public HomePageViewModel()
        {
            Activities = new IncrementalLoadingCollection<ContributionsModel>(LoadMoreItems) { BatchSize = 100 };
        }

        private async Task<IEnumerable<ContributionsModel>> LoadMoreItems(uint count)
        {
            var result = await App.ApiService.GetContributionsAsync(currentOffset, (int)count);
            
            currentOffset = result.PagingIndex;

            DisplayTotal = $"{currentOffset} of {result.TotalContributions}";

            // If we've recieved all the contributions, return null to stop automatic loading
            if (result.PagingIndex == result.TotalContributions)
                return null;

            return result.Contributions;
        }

        #region Properties
        
        public IncrementalLoadingCollection<ContributionsModel> Activities { get; set; }
        
        public bool IsBusy
        {
            get => isBusy;
            set => Set(ref isBusy, value);
        }

        public string IsBusyMessage
        {
            get => isBusyMessage;
            set => Set(ref isBusyMessage, value);
        }
        
        public string DisplayTotal
        {
            get => displayTotal;
            set => Set(ref displayTotal, value);
        }
        
        #endregion
        
        #region Methods
        

        #endregion


        #region Event Handlers

        public async void AddActivityButton_Click(object sender, RoutedEventArgs e)
        {
            await NavigationService.NavigateAsync(typeof(AddSubmissionPage));
        }
        
        #endregion

        #region Navigation

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (App.ShellPage.DataContext is ShellPageViewModel shellVm && shellVm.IsLoggedIn)
            {

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
