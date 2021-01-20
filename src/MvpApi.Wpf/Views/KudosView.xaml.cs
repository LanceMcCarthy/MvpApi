using System.Windows;
using System.Windows.Controls;
using MvpApi.Common.Models;
using MvpApi.Wpf.ViewModels;

namespace MvpApi.Wpf.Views
{
    public partial class KudosView : UserControl
    {
        public KudosView()
        {
            InitializeComponent();
        }

        private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var k = (sender as Button).DataContext as Kudos;

            if (!string.IsNullOrEmpty(k.StoreId))
            {
                await (DataContext as KudosViewModel).PurchaseKudosAsync(k.StoreId);
            }

            if (k.Title.Equals("Store Rating"))
            {
                await (DataContext as KudosViewModel).ShowRatingReviewDialog();
            }
        }
    }
}
