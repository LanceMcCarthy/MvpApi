using Microsoft.UI.Xaml.Controls;

namespace MvpCompanion.UI.WinUI.Views
{
    public sealed partial class HomePage : Page
    {
        public HomePage()
        {
            InitializeComponent();

            // Note: The is a workaround because there's no OneWayToSource binding option for UWP
            ViewModel.SelectedContributions = ContributionsGrid.SelectedItems;
            ViewModel.GroupDescriptors = ContributionsGrid.GroupDescriptors;
        }
    }
}