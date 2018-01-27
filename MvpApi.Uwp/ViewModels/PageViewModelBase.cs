using Template10.Mvvm;

namespace MvpApi.Uwp.ViewModels
{
    public class PageViewModelBase : ViewModelBase
    {
        private bool isBusy;
        private string isBusyMessage;

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
    }
}