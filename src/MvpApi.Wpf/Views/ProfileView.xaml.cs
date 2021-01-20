using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MvpApi.Common.Models;

namespace MvpApi.Wpf.Views
{
    public partial class ProfileView : UserControl
    {
        public ProfileView()
        {
            InitializeComponent();
            Loaded += ProfileView_Loaded;
        }

        private async void ProfileView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.ProfileViewModel vm)
            {
                await vm.OnLoadedAsync();
            }
        }

        private void OnlineIdentitiesListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is ViewModels.ProfileViewModel vm)
            {
                // Add any selected items to the SelectedItems collection
                if (e.AddedItems != null)
                {
                    foreach (OnlineIdentityViewModel identity in e.AddedItems)
                    {
                        vm.SelectedOnlineIdentities.Add(identity);
                    }
                }

                // Remove any selected items from the SelectedItems collection
                if (e.RemovedItems != null)
                {
                    foreach (OnlineIdentityViewModel identity in e.RemovedItems)
                    {
                        vm.SelectedOnlineIdentities.Remove(identity);
                    }
                }

                // Enable or Disable the ClearSelection and Delete buttons according to the selected items collection's count
                vm.AreAppBarButtonsEnabled = vm.SelectedOnlineIdentities.Any();
            }
            
        }
    }
}
