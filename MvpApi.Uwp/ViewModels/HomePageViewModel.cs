using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using MvpApi.Common.Models;
using Template10.Mvvm;

namespace MvpApi.Uwp.ViewModels
{
    public class HomePageViewModel : ViewModelBase
    {
        #region Fields

        private ContributionViewModel currentContributionView;
        private bool isBusy;
        private string isBusyMessage;
        private int currentPage;
        private int itemsPerPage;

        #endregion

        public HomePageViewModel()
        {
            //Activities = new IncrementalLoadingCollection<ContributionsSource, ContributionsModel>(20, OnLoading, OnEndLoading);
        }

        #region Properties

        //public IncrementalLoadingCollection<ContributionsSource, ContributionsModel> Activities { get; set; }
        
        public ContributionViewModel CurrentContributionView
        {
            get => currentContributionView;
            set => Set(ref currentContributionView, value);
        }

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

        public int CurrentPage
        {
            get => currentPage;
            set => Set(ref currentPage, value);
        }

        public int ItemsPerPage
        {
            get => itemsPerPage;
            set => Set(ref itemsPerPage, value);
        }
        
        #endregion
        
        #region Methods

        private async Task GetNextContributionAsync(int pageToGet)
        {
            try
            {
                IsBusy = true;
                IsBusyMessage = $"loaging page {CurrentPage}...";
                CurrentContributionView = await App.ApiService.GetContributionsAsync(pageToGet, ItemsPerPage);

                if(CurrentContributionView?.PagingIndex != null)
                    CurrentPage = (int)CurrentContributionView.PagingIndex;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
            finally
            {
                IsBusy = false;
                IsBusyMessage = "";
            }
        }
        
        //private void OnLoading()
        //{

        //}

        //private void OnEndLoading()
        //{

        //}

        #endregion


        #region Event Handlers

        public void AddActivityButton_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public async void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            var previousPage = CurrentPage - 1;
            if (previousPage < 0)
                previousPage = 0;

            await GetNextContributionAsync(previousPage);
        }

        public async void NextButton_Click(object sender, RoutedEventArgs e)
        {
            var nextPage = CurrentPage + 1;
            await GetNextContributionAsync(nextPage);
        }


        #endregion

        #region Navigation

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            var shellVm = App.ShellPage.DataContext as ShellPageViewModel;
            if (shellVm != null && shellVm.IsLoggedIn)
                await GetNextContributionAsync(0);
        }

        public override Task OnNavigatedFromAsync(IDictionary<string, object> pageState, bool suspending)
        {
            return base.OnNavigatedFromAsync(pageState, suspending);
        }

        #endregion

    }

    // TODO continue testing IIncrementalLoading solution. Problem is result is wrapped in a result object, dont want to lose that data
    //public class ContributionsSource : IIncrementalSource<ContributionsModel>
    //{
    //    public ContributionsSource()
    //    {
    //    }

    //    public async Task<IEnumerable<ContributionsModel>> GetPagedItemsAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = new CancellationToken())
    //    {
    //        var contrib = await App.ApiService.GetContributionsAsync(pageIndex, pageSize);
    //        return contrib.Contributions;
    //    }
    //}
}
