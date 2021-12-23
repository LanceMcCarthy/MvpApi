using MvpApi.Common.Models;
//using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Foundation.Metadata;
using Windows.Services.Store;
using Windows.UI.Popups;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
//using VungleSDK;
using MvpCompanion.UI.WinUI.Common;
using CommonHelpers.Common;
using CommunityToolkit.WinUI.Connectivity;
//using Microsoft.AppCenter.Analytics;
using MvpCompanion.UI.WinUI.Helpers;

namespace MvpCompanion.UI.WinUI.ViewModels;

public class KudosViewModel : ViewModelBase
{
    private StoreContext storeContext;
    private Visibility feedbackHubButtonVisibility;
    //VungleAd sdkInstance;
    //private string vungleAdPlacementId = "KUDOSPAGEINTERSTITIAL-9395221";

    public KudosViewModel()
    {
        KudosCollection.Add(new Kudos { Title = "Video Ad", Price = "Free", ImageUrl = "/Images/VideoAd.png" });
        KudosCollection.Add(new Kudos { Title = "Store Rating", Price = "Free", ImageUrl = "/Images/4starStar.png" });
        KudosCollection.Add(new Kudos { Title = "Small Coffee", ProductId = "MvpCompanion_SmallCoffee", StoreId = "9NJ9NKHQF7C4", Price = "$1.49", ImageUrl = "/Images/CoffeeKudo.png" });
        KudosCollection.Add(new Kudos { Title = "Lunch", ProductId = "MvpCompanion_Lunch", StoreId = "9N999Z3H3GPK", Price = "$4.89", ImageUrl = "/Images/LunchKudo.png" });
        KudosCollection.Add(new Kudos { Title = "Dinner", ProductId = "MvpCompanion_Dinner", StoreId = "9NB8SR731DM6", Price = "$9.49", ImageUrl = "/Images/DinnerKudo.png" });
    }

    public ObservableCollection<Kudos> KudosCollection { get; set; } = new();

    public Visibility FeedbackHubButtonVisibility
    {
        get => feedbackHubButtonVisibility;
        set => SetProperty(ref feedbackHubButtonVisibility, value);
    }

    public async void KudosGridView_OnItemClick(object sender, ItemClickEventArgs e)
    {
        if (!(e.ClickedItem is Kudos kudo)) return;
        
        //Analytics.TrackEvent("Kudo Selection", new Dictionary<string, string>
        //{
        //    {"Item", kudo.Title}
        //});

        if (!string.IsNullOrEmpty(kudo.StoreId))
        {
            await PurchaseKudosAsync(kudo.StoreId, kudo.Title);
        }

        if (kudo.Title == "Store Rating")
        {
            await ShowRatingReviewDialog();
        }

        if (kudo.Title == "Video Ad")
        {
            if (kudo.IsBusy)
            {
                await new MessageDialog("Ad is being fetched right now, wait for busy indicator disappear and try again.").ShowAsync();
            }
            else
            {
                //AdConfig adConfig = new AdConfig();
                //adConfig.SoundEnabled = false;

                //sdkInstance.PlayAdAsync(adConfig, vungleAdPlacementId);
            }
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

            var jsonObject = JsonObject.Parse(result.Response);
            var status = jsonObject.GetNamedString("status");

            IsBusyMessage = "action complete, showing result...";

            switch (status)
            {
                case "success":
                    await new MessageDialog("Thank you for taking the time to leave a rating! If you left 3 stars or lower, please let me know how I can improve the app (go to About page).", "Success").ShowAsync();
                    break;
                case "aborted":
                {
                    var md = new MessageDialog("If you prefer not to leave a bad rating but still want to provide feedback, click the email button below. I work hard to make sure you have a great app experience and would love to hear from you.", "Review Aborted");

                    md.Commands.Add(new UICommand("send email"));
                    md.Commands.Add(new UICommand("not now"));

                    var mdResult = await md.ShowAsync();

                    if (mdResult.Label == "send email")
                    {
                        await FeedbackHelpers.Current.EmailFeedbackMessageAsync();
                    }

                    break;
                }
                default:
                    await new MessageDialog($"The rating or review did not complete, here's what Windows had to say: {jsonObject.GetNamedString("status")}.\r\n\nIf you meant to leave a review, try again. If this keeps happening, contact us and share the error code above.", "Rating or Review was not successful").ShowAsync();
                    break;
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

    public async Task PurchaseKudosAsync(string storeId, string name)
    {
        try
        {
            IsBusy = true;
            IsBusyMessage = "in-app purchase in progress (you should see a separate window)...";

            if (storeContext == null)
                storeContext = StoreContext.GetDefault();

            var result = await storeContext.RequestPurchaseAsync(storeId);

            IsBusyMessage = "action complete, reviewing result...";

            var extendedError = "";

            if (result.ExtendedError != null)
                extendedError = result.ExtendedError.Message;

            var resultMessage = "";

            //Analytics.TrackEvent("Kudo Purchase Attempt", new Dictionary<string, string>
            //{
            //    {"StoreId", storeId},
            //    {"Kudo Name", name},
            //    {"Result", $"{result.Status}"}
            //});

            resultMessage = result.Status switch
            {
                StorePurchaseStatus.AlreadyPurchased => "You have already purchased this kudos, thank you!",
                StorePurchaseStatus.Succeeded => "Kudos provided! Thank you for your support and help in keeping this app free.",
                StorePurchaseStatus.NotPurchased => "Kudos were not purchased. Don't worry, you were not charged for peeking ;)",
                StorePurchaseStatus.NetworkError => "The purchase was unsuccessful due to a network error.\r\n\nError:\r\n" + extendedError,
                StorePurchaseStatus.ServerError => "The purchase was unsuccessful due to a server error.\r\n\nError:\r\n" + extendedError,
                _ => "The purchase was unsuccessful due to an unknown error.\r\n\nError:\r\n" + extendedError
            };

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

    //private async void SdkInstance_OnAdPlayableChanged(object sender, AdPlayableEventArgs e)
    //{
    //    Debug.WriteLine($"AdPlayable changed: {e.Placement}, Playable: {e.AdPlayable}");

    //    var kudo = KudosCollection.FirstOrDefault(a => a.Title == "Video Ad");

    //    await CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
    //    {
    //        if (kudo != null)
    //        {
    //            kudo.IsBusy = !e.AdPlayable;
    //        }
    //    });
    //}

    #region Navigation

    public async void OnLoaded()
    {
        if (!NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
        {
            await new MessageDialog("This application requires an internet connection. Please check your connection and try again.", "No Internet").ShowAsync();
            return;
        }

        //FeedbackHubButtonVisibility = StoreServicesFeedbackLauncher.IsSupported()
        //    ? Visibility.Visible
        //    : Visibility.Collapsed;

        //sdkInstance = AdFactory.GetInstance(ExternalConstants.VungleAppId);
        //sdkInstance.OnAdPlayableChanged += SdkInstance_OnAdPlayableChanged;
        //sdkInstance.LoadAd(vungleAdPlacementId);
    }

    public async void OnUnloaded()
    {
        //sdkInstance.OnAdPlayableChanged -= SdkInstance_OnAdPlayableChanged;
    }
    
    #endregion
}