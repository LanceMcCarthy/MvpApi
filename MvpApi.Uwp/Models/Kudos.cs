using MvpApi.Common.Models;

namespace MvpApi.Uwp.Models
{
    public class Kudos : ObservableObject
    {
        public string Title { get; set; }
        public string ProductId { get; set; }
        public string ImageUrl { get; set; }
        public string Price { get; set; }

        private bool isBusy;
        public bool IsBusy
        {
            get => isBusy;
            set {isBusy = value; OnPropertyChanged();}
        }
    }
}
