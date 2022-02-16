using Microsoft.Services.Store.Engagement;
using MvpApi.Common.Models;
using MvpCompanion.UI.Common.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.Services.Store;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace MvpApi.Uwp.ViewModels
{
    public class KudosViewModel : PageViewModelBase
    {
        private StoreContext _context;
        private Visibility _feedbackHubButtonVisibility;

        public KudosViewModel()
        {
            //KudosCollection.Add(new Kudos { Title = "Store Rating", Price = "Free", ImageUrl = "/Images/4starStar.png" });
            KudosCollection.Add(new Kudos { Title = "Small Coffee", ProductId = "MvpCompanion_SmallCoffee", StoreId = "9NJ9NKHQF7C4", Price = "$1.49", ImageUrl = "/Images/CoffeeKudo.png" });
            KudosCollection.Add(new Kudos { Title = "Lunch", ProductId = "MvpCompanion_Lunch", StoreId = "9N999Z3H3GPK", Price = "$4.89", ImageUrl = "/Images/LunchKudo.png" });
            KudosCollection.Add(new Kudos { Title = "Dinner", ProductId = "MvpCompanion_Dinner", StoreId = "9NB8SR731DM6", Price = "$9.49", ImageUrl = "/Images/DinnerKudo.png" });
        }

        public ObservableCollection<Kudos> KudosCollection { get; set; } = new ObservableCollection<Kudos>();

        public Visibility FeedbackHubButtonVisibility
        {
            get => _feedbackHubButtonVisibility;
            set => Set(ref _feedbackHubButtonVisibility, value);
        }

        public async void KudosGridView_OnItemClick(object sender, ItemClickEventArgs e)
        {
            if (!(e.ClickedItem is Kudos kudo)) return;

            if (ApiInformation.IsTypePresent("Microsoft.Services.Store.Engagement.StoreServicesCustomEventLogger"))
                StoreServicesCustomEventLogger.GetDefault().Log($"{kudo.Title} Kudos Item Selected");

            if (!string.IsNullOrEmpty(kudo.StoreId))
            {
                await PurchaseKudosAsync(kudo.StoreId);
            }

            //if (kudo.Title == "Store Rating")
            //{
            //    await ShowRatingReviewDialog();
            //}
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

        #region Navigation

        public override Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            FeedbackHubButtonVisibility = StoreServicesFeedbackLauncher.IsSupported()
                ? Visibility.Visible
                : Visibility.Collapsed;
            
            return base.OnNavigatedToAsync(parameter, mode, state);
        }

        public override Task OnNavigatedFromAsync(IDictionary<string, object> pageState, bool suspending)
        {
            return base.OnNavigatedFromAsync(pageState, suspending);
        }

        #endregion
    }
}