using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Services.Store;
using Windows.Storage;
using Windows.UI.Popups;
using CommonHelpers.Mvvm;
using MvpApi.Services.Utilities;
using MvpCompanion.UI.Common.Helpers;
using Newtonsoft.Json.Linq;

namespace MvpApi.Uwp.ViewModels
{
    public class ShellViewModel : PageViewModelBase
    {
        private MvpApi.Common.Models.ProfileViewModel _mvp;
        private string _profileImagePath;
        private bool _isLoggedIn;
        private DateTime _submissionStartDate = ServiceConstants.SubmissionStartDate;
        private DateTime _submissionDeadline = ServiceConstants.SubmissionDeadline;

        public ShellViewModel()
        {
            if (DesignMode.DesignModeEnabled || DesignMode.DesignMode2Enabled)
            {
                Mvp = DesignTimeHelpers.GenerateSampleMvp();
                IsLoggedIn = true;
                ProfileImagePath = "/Images/MvpIcon.png";
            }
        }

        public string ProfileImagePath
        {
            get => _profileImagePath;
            set
            {
                _profileImagePath = value;

                // Manually invoke PropertyChanged to ensure image is reloaded, even if the file path is the same.
                RaisePropertyChanged();
            }
        }

        public MvpApi.Common.Models.ProfileViewModel Mvp
        {
            get => _mvp;
            set => Set(ref _mvp, value);
        }

        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set => Set(ref _isLoggedIn, value);
        }
        
        public DateTime SubmissionStartDate
        {
            get
            {
                if (ApplicationData.Current.LocalSettings.Values.TryGetValue(nameof(SubmissionStartDate), out object rawValue))
                {
                    _submissionStartDate = DateTime.Parse((string)rawValue);
                }
                else
                {
                    ApplicationData.Current.LocalSettings.Values[nameof(SubmissionStartDate)] = _submissionStartDate.ToLongDateString();
                }

                return _submissionStartDate;
            }
            set
            {
                if (Set(ref _submissionStartDate, value))
                {
                    ApplicationData.Current.LocalSettings.Values[nameof(SubmissionStartDate)] = _submissionStartDate.ToLongDateString();
                }
            }
        }

        public DateTime SubmissionDeadline
        {
            get
            {
                if (ApplicationData.Current.LocalSettings.Values.TryGetValue(nameof(SubmissionDeadline), out object rawValue))
                {
                    _submissionDeadline = DateTime.Parse((string)rawValue);
                }
                else
                {
                    ApplicationData.Current.LocalSettings.Values[nameof(SubmissionDeadline)] = _submissionDeadline.ToLongDateString();
                }
                
                return _submissionDeadline;
            }
            set
            {
                if (Set(ref _submissionDeadline, value))
                {
                    ApplicationData.Current.LocalSettings.Values[nameof(SubmissionDeadline)] = _submissionDeadline.ToLongDateString();
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
                        await FeedbackHelpers.Current.EmailFeedbackMessageAsync();
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
    }
}
