using System.Linq;
using Windows.UI.Xaml.Controls;
using MvpApi.Common.Interfaces;

namespace MvpApi.Uwp.Views
{
    public sealed partial class HomePage : Page, IFlyoutView, IScrollableView
    {
        public HomePage()
        {
            InitializeComponent();

            // Note: The is a workaround because there's no OneWayToSource binding option for UWP
            ViewModel.SelectedContributions = ContributionsGrid.SelectedItems;
            ViewModel.GroupDescriptors = ContributionsGrid.GroupDescriptors;

            ViewModel.FlyoutView = this;
            ViewModel.ScrollableView = this;
        }

        public void CloseFlyout()
        {
            if(AwardCyclePreferenceFlyout.IsOpen)
                AwardCyclePreferenceFlyout.Hide();
            
            if (GroupingFlyout.IsOpen)
                GroupingFlyout.Hide();
        }

        public void ScrollToTop()
        {
            var firstItem = ContributionsGrid.GetDataView().Items.FirstOrDefault();

            if (firstItem != null)
            {
                ContributionsGrid.ScrollItemIntoView(firstItem);
            }
        }

        public void ScrollTo(object item)
        {
            ContributionsGrid.ScrollItemIntoView(item);
        }

        public void ScrollTo(int index)
        {
            ContributionsGrid.ScrollIndexIntoView(index);
        }

        public void ScrollToEnd()
        {
            var lastItem = ContributionsGrid.GetDataView().Items.LastOrDefault();

            if (lastItem != null)
            {
                ContributionsGrid.ScrollItemIntoView(lastItem);
            }
        }
    }
}