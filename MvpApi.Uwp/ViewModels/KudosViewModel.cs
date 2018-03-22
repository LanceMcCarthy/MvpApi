using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Services.Store;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Microsoft.Advertising.WinRT.UI;
using Microsoft.Services.Store.Engagement;
using MvpApi.Uwp.Helpers;
using MvpApi.Uwp.Models;
using Newtonsoft.Json.Linq;

namespace MvpApi.Uwp.ViewModels
{
    public class KudosViewModel : PageViewModelBase
    {
        private StoreContext context;
        private Visibility feedbackHubButtonVisibility;
        private InterstitialAd myInterstitialAd;

        public KudosViewModel()
        {
        }

        public ObservableCollection<Kudos> KudosCollection { get; set; } = new ObservableCollection<Kudos>();
        
        public Visibility FeedbackHubButtonVisibility
        {
            get => feedbackHubButtonVisibility;
            set => Set(ref feedbackHubButtonVisibility, value);
        }

        public async void KudosGridView_OnItemClick(object sender, ItemClickEventArgs e)
        {
            if (!(e.ClickedItem is Kudos kudo)) return;

            StoreServicesCustomEventLogger.GetDefault().Log($"{kudo.Title} Kudos Item Selected");

            if (!string.IsNullOrEmpty(kudo.ProductId))
            {
                await PurchaseKudosAsync(kudo.ProductId);
            }

            if (kudo.Title == "Store Rating")
            {
                await ShowRatingReviewDialog();
            }

            if (kudo.Title == "Video Ad")
            {
                // Wait for ad to be ready
                if (kudo.IsBusy)
                {
                    await new MessageDialog("Ad is being fetched right now, wait for busy indicator disappear and try again.").ShowAsync();
                    return;
                }

                // double check the ad is ready using the State value
                if (myInterstitialAd.State == InterstitialAdState.Ready)
                {
                    myInterstitialAd.Show();
                }
            }
        }

        public async Task ShowRatingReviewDialog()
        {
            try
            {
                IsBusy = true;
                IsBusyMessage = "opening Store rating window";

                var result = await StoreRequestHelper.SendRequestAsync(StoreContext.GetDefault(), 16, "");

                if (result.ExtendedError != null) 
                    return;

                var jsonObject = JObject.Parse(result.Response);
                
                if (jsonObject.SelectToken("status").ToString() == "success")
                {
                    await new MessageDialog("Thank you for taking the time to leave a rating!").ShowAsync();
                }
            }
            catch (Exception ex)
            {
                await ex.LogExceptionWithUserMessage();
            }
            finally
            {
                IsBusy = false;
                IsBusyMessage = "";
            }
        }

        public async Task PurchaseKudosAsync(string storeId)
        {
            try
            {
                IsBusy = true;
                IsBusyMessage = "app rating in progress (you should see a separate window)...";

                if (context == null)
                    context = StoreContext.GetDefault();
                
                var result = await context.RequestPurchaseAsync(storeId);
                
                var extendedError = "";

                if (result.ExtendedError != null)
                    extendedError = result.ExtendedError.Message;

                var resultMessage = "";

                switch (result.Status)
                {
                    case StorePurchaseStatus.AlreadyPurchased:
                        resultMessage = "You have already purchased this kudos, thank you!";
                        break;
                    case StorePurchaseStatus.Succeeded:
                        resultMessage = "Kudos provided! Thank you for your support and help in keeping this app free.";
                        break;
                    case StorePurchaseStatus.NotPurchased:
                        resultMessage = "Kudos was not provided, don't worry you were not charged for peeking at the item :D";
                        break;
                    case StorePurchaseStatus.NetworkError:
                        resultMessage = "The purchase was unsuccessful due to a network error.\r\n\nError:\r\n" + extendedError;
                        break;
                    case StorePurchaseStatus.ServerError:
                        resultMessage = "The purchase was unsuccessful due to a server error.\r\n\nError:\r\n" + extendedError;
                        break;
                    default:
                        resultMessage = "The purchase was unsuccessful due to an unknown error.\r\n\nError:\r\n" + extendedError;
                        break;
                }

                await new MessageDialog(resultMessage).ShowAsync();
            }
            catch (Exception ex)
            {
                await ex.LogExceptionWithUserMessage();
            }
            finally
            {
                IsBusy = false;
                IsBusyMessage = "";
            }
        }
        
        private void MyInterstitialAd_AdReady(object sender, object e)
        {
            var kudo = KudosCollection.FirstOrDefault(a => a.Title == "Video Ad");

            if (kudo != null)
                kudo.IsBusy = false;
        }

        private void MyInterstitialAd_ErrorOccurred(object sender, AdErrorEventArgs e)
        {
            RefreshAd();
        }

        private void MyInterstitialAd_Completed(object sender, object e)
        {
            RefreshAd();
        }

        private void MyInterstitialAd_Cancelled(object sender, object e)
        {
            RefreshAd();
        }

        private void RefreshAd()
        {
            // Note: Ad unit name is 'KudosVideoInterstitial'
            myInterstitialAd.RequestAd(AdType.Video, "9nrxnx3wlh77", "1100019939");

            var kudo = KudosCollection.FirstOrDefault(a => a.Title == "Video Ad");
            if (kudo != null) kudo.IsBusy = true;
        }

        #region Navigation

        public override Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            KudosCollection.Add(new Kudos { Title = "Video Ad", Price = "Free", ImageUrl = "/Images/VideoAd.png" });
            KudosCollection.Add(new Kudos { Title = "Store Rating", Price = "Free", ImageUrl = "/Images/4starStar.png" });
            KudosCollection.Add(new Kudos { Title = "Small Coffee", ProductId = "MvpCompanion_SmallCoffee", Price = "$1.49", ImageUrl = "/Images/CoffeeKudo.png" });
            KudosCollection.Add(new Kudos { Title = "Lunch", ProductId = "MvpCompanion_Lunch", Price = "$4.89", ImageUrl = "/Images/LunchKudo.png" });
            KudosCollection.Add(new Kudos { Title = "Dinner", ProductId = "MvpCompanion_Dinner", Price = "$9.49", ImageUrl = "/Images/DinnerKudo.png" });

            FeedbackHubButtonVisibility = StoreServicesFeedbackLauncher.IsSupported()
                ? Visibility.Visible
                : Visibility.Collapsed;

            myInterstitialAd = new InterstitialAd();
            myInterstitialAd.AdReady += MyInterstitialAd_AdReady;
            myInterstitialAd.ErrorOccurred += MyInterstitialAd_ErrorOccurred;
            myInterstitialAd.Completed += MyInterstitialAd_Completed;
            myInterstitialAd.Cancelled += MyInterstitialAd_Cancelled;

            RefreshAd();

            return base.OnNavigatedToAsync(parameter, mode, state);
        }

        public override Task OnNavigatedFromAsync(IDictionary<string, object> pageState, bool suspending)
        {
            myInterstitialAd.AdReady -= MyInterstitialAd_AdReady;
            myInterstitialAd.ErrorOccurred -= MyInterstitialAd_ErrorOccurred;
            myInterstitialAd.Completed -= MyInterstitialAd_Completed;
            myInterstitialAd.Cancelled -= MyInterstitialAd_Cancelled;

            return base.OnNavigatedFromAsync(pageState, suspending);
        }

        #endregion
    }
}