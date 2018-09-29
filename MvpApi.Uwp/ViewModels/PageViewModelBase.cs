namespace MvpApi.Uwp.ViewModels
{
    public class PageViewModelBase : Template10.Mvvm.ViewModelBase
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