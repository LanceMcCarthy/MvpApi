using Windows.UI.Xaml.Controls;
using MvpApi.Common.Interfaces;

namespace MvpApi.Uwp.Views
{
    public sealed partial class HomePage : Page, IFlyoutView
    {
        public HomePage()
        {
            InitializeComponent();

            // Note: The is a workaround because there's no OneWayToSource binding option for UWP
            ViewModel.SelectedContributions = ContributionsGrid.SelectedItems;
            ViewModel.GroupDescriptors = ContributionsGrid.GroupDescriptors;

            ViewModel.FlyoutView = this;
        }

        public void CloseFlyout()
        {
            if(AwardCyclePreferenceFlyout.IsOpen)
                AwardCyclePreferenceFlyout.Hide();
            
            if (GroupingFlyout.IsOpen)
                GroupingFlyout.Hide();
        }
    }
}