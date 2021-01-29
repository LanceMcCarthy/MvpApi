namespace MvpCompanion.UI.ViewModels
{
    public class PageViewModelBase : Template10.Mvvm.ViewModelBase
    {
        private bool _isBusy;
        private string _isBusyMessage;

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
    }
}