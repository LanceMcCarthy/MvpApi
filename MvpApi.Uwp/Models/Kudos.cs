using CommonHelpers.Common;

namespace MvpApi.Uwp.Models
{
    public class Kudos : BindableBase
    {
        public string Title { get; set; }
        public string ProductId { get; set; }
        public string StoreId { get; set; }
        public string ImageUrl { get; set; }
        public string Price { get; set; }

        private bool isBusy;
        public bool IsBusy
        {
            get => isBusy;
            set => SetProperty(ref isBusy, value);
        }
    }
}
