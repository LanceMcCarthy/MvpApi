using MvpApi.Common.Interfaces;
using MvpApi.Common.Models;
using System.Collections;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;

namespace MvpApi.Uwp.Views
{
    public sealed partial class HomePage : Page, IFlyoutView, IScrollableView, IExpandableItemView
    {
        private ContributionsModel lastExpanded = null;

        public HomePage()
        {
            InitializeComponent();

            // Note: The is a workaround because there's no OneWayToSource binding option for UWP
            ViewModel.SelectedContributions = ContributionsGrid.SelectedItems;
            ViewModel.GroupDescriptors = ContributionsGrid.GroupDescriptors;

            ViewModel.FlyoutView = this;
            ViewModel.ScrollableView = this;
            ViewModel.ExpandableItemView = this;
        }

        #region Interface methods

        public void CloseFlyout()
        {
            if (GroupingFlyout.IsOpen)
                GroupingFlyout.Hide();
        }

        public int GetCurrentVisibleIndex()
        {
            var point = new Point
            {
                X = ContributionsGrid.CenterPoint.X,
                Y = ContributionsGrid.CenterPoint.Y
            };

            var centerItem = ContributionsGrid.HitTestService.RowItemFromPoint(point) as ContributionsModel;

            return (ContributionsGrid.ItemsSource as IList).IndexOf(centerItem);
        }

        public object GetCurrentVisibleItem()
        {
            var point = new Point
            {
                X = ContributionsGrid.CenterPoint.X,
                Y = ContributionsGrid.CenterPoint.Y
            };

            return ContributionsGrid.HitTestService.RowItemFromPoint(point) as ContributionsModel;
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

        public void CollapseRow(object item)
        {
            if (item is ContributionsModel contribution)
            {
                ContributionsGrid.ShowRowDetailsForItem(contribution);
                lastExpanded = null;
            }
        }

        public void ExpandRow(object item)
        {
            if(item is ContributionsModel contribution)
            {
                ContributionsGrid.ShowRowDetailsForItem(contribution);
                lastExpanded = contribution;
            }
        }

        public void ToggleRowDetail(object item)
        {
            if (item is ContributionsModel contribution)
            {
                if (lastExpanded != null && lastExpanded == contribution)
                {
                    ContributionsGrid.HideRowDetailsForItem(contribution);
                }
                else
                {
                    ContributionsGrid.ShowRowDetailsForItem(contribution);
                }

                lastExpanded = contribution;
            }
        }

        public void CollapseAll()
        {
            throw new System.NotImplementedException();
        }

        public void ExpandAll()
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}