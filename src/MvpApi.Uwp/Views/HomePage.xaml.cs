using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using MvpApi.Common.Interfaces;
using MvpApi.Common.Models;
using MvpApi.Uwp.Dialogs;
using Telerik.UI.Xaml.Controls.Grid;
using Button = Windows.UI.Xaml.Controls.Button;

namespace MvpApi.Uwp.Views
{
    public sealed partial class HomePage : Page, IFlyoutView, IScrollableView
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
        }

        private void ExpandButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is ContributionsModel contribution)
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

        private async void CloneButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is ContributionsModel originalContribution)
            {
                var editDialog = new ContributionEditorDialog(originalContribution, true);

                await editDialog.ShowAsync();
                
                if (editDialog.ContributionResult != null)
                {
                    Debug.WriteLine($"Cloned item uploaded {editDialog.ContributionResult.ContributionTypeName}");

                    var refreshData = await AskForDataRefreshAsync();

                    if (refreshData)
                    {
                        ViewModel.IsBusyMessage = "refreshing contributions...";

                        var itemIndex = ViewModel.Contributions.IndexOf(originalContribution);

                        await ViewModel.RefreshAndReturnToPositionAsync(Convert.ToUInt32(itemIndex));
                    }
                    else
                    {
                        ViewModel.Contributions.Insert(0, editDialog.ContributionResult);
                    }
                }
            }
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is ContributionsModel contribution)
            {
                var editDialog = new ContributionEditorDialog(contribution, false);

                await editDialog.ShowAsync();
                
                if (editDialog.ContributionResult != null)
                {
                    var itemIndex = ViewModel.Contributions.IndexOf(contribution);

                    Debug.WriteLine($"Edited {editDialog.ContributionResult.ContributionTypeName}");

                    var refreshData = await AskForDataRefreshAsync();

                    if (refreshData)
                    {
                        ViewModel.IsBusyMessage = "refreshing contributions...";
                        
                        await ViewModel.RefreshAndReturnToPositionAsync(Convert.ToUInt32(itemIndex));
                    }
                    else
                    {
                        ViewModel.Contributions.RemoveAt(itemIndex);
                        ViewModel.Contributions.Insert(itemIndex, editDialog.ContributionResult);
                    }
                }
                else
                {
                    await new MessageDialog("The Edit operation did not complete.", "Operation Failed").ShowAsync();
                }
            }
        }
        
        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is ContributionsModel contribution)
            {
                if (contribution.IsEditable != true)
                    return;
                
                var md = new MessageDialog(
                    "Are you sure you want to delete this? there is no way to recover it.",
                    "Delete?");

                md.Commands.Add(new UICommand("DELETE"));
                md.Commands.Add(new UICommand("cancel"));
                md.DefaultCommandIndex = 1;

                var mdResult = await md.ShowAsync();

                if (mdResult.Label != "DELETE") 
                    return;

                try
                {
                    ViewModel.IsBusy = false;
                    ViewModel.IsBusyMessage = $"deleting {contribution.Title}...";

                    var indexToReturnTo = ViewModel.Contributions.IndexOf(contribution);
                            
                    var success = await App.ApiService.DeleteContributionAsync(contribution);

                    if (success == true)
                    {
                        // Ask for local data refresh 
                        var refreshData = await AskForDataRefreshAsync();

                        if (refreshData)
                        {
                            ViewModel.IsBusyMessage = "refreshing contributions...";

                            // TODO - IMPORTANT: decide if we need a full refresh or if this custom refresh with scrolling position works
                            await ViewModel.RefreshAndReturnToPositionAsync(Convert.ToUInt32(indexToReturnTo));
                        }
                        else
                        {
                            ViewModel.Contributions.Remove(contribution);
                        }
                    }
                    else
                    {
                        await new MessageDialog("The contribution was not deleted, the API refused to complete the request to remove this item.", "Not Deleted").ShowAsync();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Delete Contribution Exception: {ex}");
                }
                finally
                {
                    ViewModel.IsBusy = false;
                    ViewModel.IsBusyMessage = "";
                }
            }
        }

        private async Task<bool> AskForDataRefreshAsync()
        {
            var refreshDataConfirmation = new MessageDialog(
                "Operation was successful, do you want to refresh your local data?\r\n\n" +
                "- A refresh will pull the latest data from the API, but you might lose your scrolling position.\r\n" +
                "- Skipping this will keep your position in the list to keep working, but your fetch numbers might be inaccurate until you do a refresh.",
                "Success! Refresh Data Now?");

            refreshDataConfirmation.Commands.Add(new UICommand("Yes, refresh data now"));
            refreshDataConfirmation.Commands.Add(new UICommand("No, I'll refresh later"));
            refreshDataConfirmation.DefaultCommandIndex = 0;

            var md2Result = await refreshDataConfirmation.ShowAsync();

            return md2Result.Label != "No, I'll refresh later";
        }

        public void CloseFlyout()
        {
            if (AwardCyclePreferenceFlyout.IsOpen)
                AwardCyclePreferenceFlyout.Hide();

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
    }
}