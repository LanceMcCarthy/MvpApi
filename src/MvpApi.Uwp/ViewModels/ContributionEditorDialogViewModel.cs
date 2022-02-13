using Windows.ApplicationModel;
using MvpApi.Common.Models;
using Template10.Mvvm;

namespace MvpApi.Uwp.ViewModels
{
    public class ContributionEditorDialogViewModel : ViewModelBase
    {
        private ContributionsModel _contribution;
        private string _urlHeader = "Url";
        private string _annualQuantityHeader = "Annual Quantity";
        private string _secondAnnualQuantityHeader = "Second Annual Quantity";
        private string _annualReachHeader = "Annual Reach";
        private bool _isUrlRequired;
        private bool _isAnnualQuantityRequired;
        private bool _isSecondAnnualQuantityRequired;
        private bool _isAnnualReachRequired;
        private bool _canSave = true;
        private string _headerMessage;
        private bool _isBusy;
        private string _isBusyMessage;
        private bool _isCloningContribution;
        
        public ContributionEditorDialogViewModel()
        {
            if (DesignMode.DesignModeEnabled || DesignMode.DesignMode2Enabled)
            {
                //Types = DesignTimeHelpers.GenerateContributionTypes();
                //Visibilities = DesignTimeHelpers.GenerateVisibilities();
                //UploadQueue = DesignTimeHelpers.GenerateContributions();
                //Contribution = UploadQueue.FirstOrDefault();
            }
        }
        
        public ContributionsModel Contribution
        {
            get => _contribution;
            set => Set(ref _contribution, value);
        }
        
        public string AnnualQuantityHeader
        {
            get => _annualQuantityHeader;
            set => Set(ref _annualQuantityHeader, value);
        }

        public string SecondAnnualQuantityHeader
        {
            get => _secondAnnualQuantityHeader;
            set => Set(ref _secondAnnualQuantityHeader, value);
        }

        public string AnnualReachHeader
        {
            get => _annualReachHeader;
            set => Set(ref _annualReachHeader, value);
        }

        public string UrlHeader
        {
            get => _urlHeader;
            set => Set(ref _urlHeader, value);
        }

        public bool IsUrlRequired
        {
            get => _isUrlRequired;
            set => Set(ref _isUrlRequired, value);
        }

        public bool IsAnnualQuantityRequired
        {
            get => _isAnnualQuantityRequired;
            set => Set(ref _isAnnualQuantityRequired, value);
        }

        public bool IsSecondAnnualQuantityRequired
        {
            get => _isSecondAnnualQuantityRequired;
            set => Set(ref _isSecondAnnualQuantityRequired, value);
        }

        public bool IsAnnualReachRequired
        {
            get => _isAnnualReachRequired;
            set => Set(ref _isAnnualReachRequired, value);
        }

        public bool CanSave
        {
            get => _canSave;
            set => Set(ref _canSave, value);
        }

        public string HeaderMessage
        {
            get => _headerMessage;
            set => Set(ref _headerMessage, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => Set(ref _isBusy, value);
        }

        public string IsBusyMessage
        {
            get => _isBusyMessage;
            set => Set(ref _isBusyMessage, value);
        }

        public bool IsCloningContribution
        {
            get => _isCloningContribution;
            set => Set(ref _isCloningContribution, value);
        }
    }
}
