using System.Linq;
using System.Windows.Controls;
using MvpApi.Common.Models;
using MvpCompanion.Wpf.ViewModels;
using Telerik.Windows.Controls;

namespace MvpCompanion.Wpf.Views
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
            if (DataContext is HomeViewModel vm)
            {
                await vm.OnLoadedAsync();
            }
        }

        private void DataControl_OnSelectionChanged(object sender, SelectionChangeEventArgs e)
        {
            if (DataContext is HomeViewModel vm)
            {
                if (vm.GridSelectionMode == DataGridSelectionMode.Single && e?.AddedItems?.FirstOrDefault() is ContributionsModel contribution)
                {

                }
            }  
        }
    }
}
