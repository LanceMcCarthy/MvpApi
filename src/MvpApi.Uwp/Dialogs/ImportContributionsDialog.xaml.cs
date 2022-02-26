using MvpApi.Common.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MvpApi.Uwp.Dialogs
{
    public sealed partial class ImportContributionsDialog : ContentDialog
    {
        private ContributionsModel lastExpanded;
        
        public static readonly DependencyProperty SelectedContributionsProperty = DependencyProperty.Register(
            "SelectedContributions", typeof(ObservableCollection<ContributionsModel>), typeof(ImportContributionsDialog), new PropertyMetadata(default(ObservableCollection<ContributionsModel>)));

        public ObservableCollection<ContributionsModel> SelectedContributions
        {
            get => (ObservableCollection<ContributionsModel>) GetValue(SelectedContributionsProperty);
            set => SetValue(SelectedContributionsProperty, value);
        }

        public ImportContributionsDialog(IList<ContributionsModel> items)
        {
            InitializeComponent();
            
            DataGrid1.ItemsSource = items;
            SelectAllCheckBox.IsChecked = true;
        }

        private void SelectAllCheckBox_OnChecked(object sender, RoutedEventArgs e)
        {
            DataGrid1.SelectAll();
        }

        private void SelectAllCheckBox_OnUnchecked(object sender, RoutedEventArgs e)
        {
            DataGrid1.DeselectAll();
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
            SelectedContributions = new ObservableCollection<ContributionsModel>();

            foreach (var selectedItem in DataGrid1.SelectedItems)
            {
                if (selectedItem is ContributionsModel itemToAdd)
                {
                    SelectedContributions.Add(itemToAdd);
                }
            }
            
            this.Hide();
        }

        private void ImportContributionsDialog_OnSecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.SelectedContributions = null;
            this.Hide();
        }
    }
}
