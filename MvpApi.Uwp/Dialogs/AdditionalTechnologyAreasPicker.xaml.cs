using System.Diagnostics;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using MvpApi.Common.Models;
using MvpApi.Uwp.ViewModels;

namespace MvpApi.Uwp.Dialogs
{
    public sealed partial class AdditionalTechnologyAreasPicker : ContentDialog
    {
        public ContributionDetailViewModel Context { get; set; }
        private CollectionViewSource _cvs;

        public AdditionalTechnologyAreasPicker()
        {
            DataContext = Context;

            InitializeComponent();
            
            TechnologyAreasListView.SelectionChanged += TechnologyAreasListView_OnSelectionChanged;

            Opened += AdditionalTechnologyAreasPicker_Opened;
        }

        private void AdditionalTechnologyAreasPicker_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            if (_cvs == null)
            {
                BusyIndicator.IsActive = true;

                _cvs = new CollectionViewSource
                {
                    Source = Context.CategoryAreas,
                    IsSourceGrouped = true,
                    ItemsPath = new PropertyPath("ContributionAreas")
                };

                TechnologyAreasListView.ItemsSource = _cvs.View;

                BusyIndicator.IsActive = false;
            }
            
            foreach (var contributionTechnologyModel in Context.SelectedContribution.AdditionalTechnologies)
            {
                Debug.WriteLine($"Item Added");

                TechnologyAreasListView.SelectedItems.Add(contributionTechnologyModel);
            }
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Hide();
        }

        private void TechnologyAreasListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BusyIndicator.IsActive = true;

            foreach (var item in e.RemovedItems)
            {
                if (Context.SelectedContribution.AdditionalTechnologies.Contains(item as ContributionTechnologyModel))
                {
                    Context.SelectedContribution.AdditionalTechnologies.Remove(item as ContributionTechnologyModel);
                }
            }

            foreach (var item in e.AddedItems)
            {
                if (!Context.SelectedContribution.AdditionalTechnologies.Contains(item as ContributionTechnologyModel))
                {
                    Context.SelectedContribution.AdditionalTechnologies.Add(item as ContributionTechnologyModel);
                }
            }
            
            if (Context.SelectedContribution.AdditionalTechnologies.Count > 2)
            {
                AlertTextBlock.Text = "You can only have 2 additional technologies selected.";
                AlertGrid.Visibility = Visibility.Visible;

                TechnologyAreasListView.SelectedItems.Remove(TechnologyAreasListView.SelectedItems.LastOrDefault());
            }

            BusyIndicator.IsActive = false;
        }

        private void CloseAlertButton_OnClick(object sender, RoutedEventArgs e)
        {
            AlertGrid.Visibility = Visibility.Collapsed;
        }
    }
}
