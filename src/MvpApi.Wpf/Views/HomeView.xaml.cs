using System.Linq;
using System.Windows.Controls;
using MvpApi.Common.Models;
using Telerik.Windows.Controls;

namespace MvpApi.Wpf.Views
{
    public partial class HomeView : UserControl
    {
        public HomeView()
        {
            InitializeComponent();
            Loaded += HomeView_Loaded;
        }

        private async void HomeView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            await ViewModel.OnLoadedAsync();
        }

        private void DataControl_OnSelectionChanged(object sender, SelectionChangeEventArgs e)
        {
            //if (ViewModel.GridSelectionMode == DataGridSelectionMode.Single && e?.AddedItems?.FirstOrDefault() is ContributionsModel contribution)
            //{

            //}
        }
    }
}
