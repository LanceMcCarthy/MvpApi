using MvpApi.Common.Models;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MvpApi.Uwp.Dialogs
{
    public sealed partial class ImportContributionsDialog : ContentDialog
    {
        private ContributionsModel lastExpanded;

        public ImportContributionsDialog(IList<ContributionsModel> items)
        {
            this.InitializeComponent();
            DataGrid1.ItemsSource = items;
        }

        private void ExpandButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is ContributionsModel contribution)
            {
                if (lastExpanded != null && lastExpanded == contribution)
                {
                    DataGrid1.HideRowDetailsForItem(contribution);
                }
                else
                {
                    DataGrid1.ShowRowDetailsForItem(contribution);
                }

                lastExpanded = contribution;
            }
        }

        private void ImportContributionsDialog_OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.Hide();
        }
    }
}
