using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using Windows.Services.Store;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using CommonHelpers.Common;
using MvpApi.Common.Models;
using MvpApi.Wpf.Helpers;
using Newtonsoft.Json.Linq;

namespace MvpApi.Wpf.ViewModels
{
    public class KudosViewModel : ViewModelBase
    {
        private StoreContext _context;
        private Visibility _feedbackHubButtonVisibility;
        //private InterstitialAd _myInterstitialAd;

        public KudosViewModel()
        {
            KudosCollection.Add(new Kudos { Title = "Video Ad", Price = "Free", ImageUrl = "/Images/VideoAd.png" });
            KudosCollection.Add(new Kudos { Title = "Store Rating", Price = "Free", ImageUrl = "/Images/4starStar.png" });
            KudosCollection.Add(new Kudos { Title = "Small Coffee", ProductId = "MvpCompanion_SmallCoffee", StoreId = "9NJ9NKHQF7C4", Price = "$1.49", ImageUrl = "/Images/CoffeeKudo.png" });
            KudosCollection.Add(new Kudos { Title = "Lunch", ProductId = "MvpCompanion_Lunch", StoreId = "9N999Z3H3GPK", Price = "$4.89", ImageUrl = "/Images/LunchKudo.png" });
            KudosCollection.Add(new Kudos { Title = "Dinner", ProductId = "MvpCompanion_Dinner", StoreId = "9NB8SR731DM6", Price = "$9.49", ImageUrl = "/Images/DinnerKudo.png" });
        }

        public ObservableCollection<Kudos> KudosCollection { get; set; } = new ObservableCollection<Kudos>();
        
        public Visibility FeedbackHubButtonVisibility
        {
            get => _feedbackHubButtonVisibility;
            set => SetProperty(ref _feedbackHubButtonVisibility, value);
        }

        public async void KudosGridView_OnItemClick(object sender, ItemClickEventArgs e)
        {
            if (!(e.ClickedItem is Kudos kudo)) return;

            //if (ApiInformation.IsTypePresent("Microsoft.Services.Store.Engagement.StoreServicesCustomEventLogger"))
            //    StoreServicesCustomEventLogger.GetDefault().Log($"{kudo.Title} Kudos Item Selected");

            if (!string.IsNullOrEmpty(kudo.StoreId))
            {
                await PurchaseKudosAsync(kudo.StoreId);
            }

            if (kudo.Title == "Store Rating")
            {
                await ShowRatingReviewDialog();
            }

            if (kudo.Title == "Video Ad")
            {
                // Wait for ad to be ready
                //if (kudo.IsBusy)
                //{
                //    await new MessageDialog("Ad is being fetched right now, wait for busy indicator disappear and try again.").ShowAsync();
                //    return;
                //}

                //// double check the ad is ready using the State value
                //if (_myInterstitialAd.State == InterstitialAdState.Ready)
                //{
                //    _myInterstitialAd.Show();
                //}
            }
        }

        public async Task ShowRatingReviewDialog()
        {
            try
            {
                IsBusy = true;
                IsBusyMessage = "rating and review in progress (you should see a separate window)...";

                var result = await StoreRequestHelper.SendRequestAsync(StoreContext.GetDefault(), 16, "");

                IsBusyMessage = "action complete, reviewing result...";

                if (result.ExtendedError != null) 
                    return;

                var jsonObject = JObject.Parse(result.Response);
                var status = jsonObject.SelectToken("status").ToString();

                IsBusyMessage = "action complete, showing result...";

                if (status == "success")
                {
                    await new MessageDialog("Thank you for taking the time to leave a rating! If you left 3 stars or lower, please let me know how I can improve the app (go to About page).", "Success").ShowAsync();
                }
                else if (status == "aborted")
                {
                    var md = new MessageDialog("If you prefer not to leave a bad rating but still want to provide feedback, click the email button below. I work hard to make sure you have a great app experience and would love to hear from you.", "Review Aborted");

                    md.Commands.Add(new UICommand("send email"));
                    md.Commands.Add(new UICommand("not now"));

                    var mdResult = await md.ShowAsync();

                    if (mdResult.Label == "send email")
                    {
                        FeedbackHelpers.Current.EmailFeedbackMessage();
                    }
                }
                else
                {
                    await new MessageDialog($"The rating or review did not complete, here's what Windows had to say: {jsonObject.SelectToken("status")}.\r\n\nIf you meant to leave a review, try again. If this keeps happening, contact us and share the error code above.", "Rating or Review was not successful").ShowAsync();
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
                IsBusyMessage = "in-app purchase in progress (you should see a separate window)...";

                if (_context == null)
                    _context = StoreContext.GetDefault();
                
                var result = await _context.RequestPurchaseAsync(storeId);

                IsBusyMessage = "action complete, reviewing result...";

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
                        resultMessage = "Kudos were not purchased. Don't worry, you were not charged for peeking ;)";
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

                IsBusyMessage = "action complete, showing result...";

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

        //private void MyInterstitialAd_AdReady(object sender, object e)
        //{
        //    var kudo = KudosCollection.FirstOrDefault(a => a.Title == "Video Ad");

        //    if (kudo != null)
        //        kudo.IsBusy = false;
        //}

        //private void MyInterstitialAd_ErrorOccurred(object sender, AdErrorEventArgs e)
        //{
        //    RefreshAd();
        //}

        //private void MyInterstitialAd_Completed(object sender, object e)
        //{
        //    RefreshAd();
        //}

        //private void MyInterstitialAd_Cancelled(object sender, object e)
        //{
        //    RefreshAd();
        //}

        //private void RefreshAd()
        //{
        //    // Note: Ad unit name is 'KudosVideoInterstitial'
        //    _myInterstitialAd.RequestAd(AdType.Video, "9nrxnx3wlh77", "1100019939");

        //    var kudo = KudosCollection.FirstOrDefault(a => a.Title == "Video Ad");
        //    if (kudo != null) kudo.IsBusy = true;
        //}

        #region Navigation

        public async Task OnLoadedAsync()
        {
            //FeedbackHubButtonVisibility = StoreServicesFeedbackLauncher.IsSupported()
            //    ? Visibility.Visible
            //    : Visibility.Collapsed;

            //_myInterstitialAd = new InterstitialAd();
            //_myInterstitialAd.AdReady += MyInterstitialAd_AdReady;
            //_myInterstitialAd.ErrorOccurred += MyInterstitialAd_ErrorOccurred;
            //_myInterstitialAd.Completed += MyInterstitialAd_Completed;
            //_myInterstitialAd.Cancelled += MyInterstitialAd_Cancelled;

            //RefreshAd();

        }

        public async Task OnUnloadedAsync()
        {
            //_myInterstitialAd.AdReady -= MyInterstitialAd_AdReady;
            //_myInterstitialAd.ErrorOccurred -= MyInterstitialAd_ErrorOccurred;
            //_myInterstitialAd.Completed -= MyInterstitialAd_Completed;
            //_myInterstitialAd.Cancelled -= MyInterstitialAd_Cancelled;

        }

        #endregion
    }
}