using CommonHelpers.Common;

namespace MvpApi.Common.Models
{
    public class Kudos : BindableBase
    {
        private string _title;
        private string _productId;
        private string _storeId;
        private string _imageUrl;
        private string _price;
        private bool _isBusy;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string ProductId
        {
            get => _productId;
            set => SetProperty(ref _productId, value);
        }

        public string StoreId
        {
            get => _storeId;
            set => SetProperty(ref _storeId, value);
        }

        public string ImageUrl
        {
            get => _imageUrl;
            set => SetProperty(ref _imageUrl, value);
        }

        public string Price
        {
            get => _price;
            set => SetProperty(ref _price, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }
    }
}
