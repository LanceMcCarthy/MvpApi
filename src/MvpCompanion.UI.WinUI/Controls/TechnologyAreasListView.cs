// This is a custom control for Technology Category Areas that two-way synced the SelectedItems collection
// There's currently a bug with multiple selection but it worked so well otherwise that I'll keep the source for future consideration
// Example XAML usage...
//<controls:TechnologyAreasListView
//    x:Name="AdditionalTechnologiesListView"
//    ItemsSource="{Binding Source={StaticResource awardCategoriesCvs}}"
//    AdditionalTechnologies="{Binding SelectedContribution.AdditionalTechnologies}"
//    Style="{StaticResource AdditionalTechnologyAreasListViewStyle}"
//    SelectionMode="Multiple" />

//using System;
//using System.Collections.ObjectModel;
//using System.Collections.Specialized;
//using System.Diagnostics;
//using System.Linq;
//using Windows.UI.Popups;
//using Windows.UI.Xaml;
//using Windows.UI.Xaml.Controls;
//using MvpApi.Common.Models;
//using MvpApi.Uwp.Helpers;

namespace MvpCompanion.UI.Common.Controls
{
    //public class TechnologyAreasListView : ListView
    //{
    //    public TechnologyAreasListView()
    //    {
    //        DefaultStyleKey = typeof(ListView);
    //        this.SelectionChanged += TechnologyAreasListView_SelectionChanged;
    //    }

    //    public static readonly DependencyProperty AdditionalTechnologiesProperty = DependencyProperty.Register(
    //        "AdditionalTechnologies",
    //        typeof(ObservableCollection<ContributionTechnologyModel>),
    //        typeof(TechnologyAreasListView),
    //        new PropertyMetadata(default(ContributionsModel), OnSelectedValuesChanged));

    //    /// <summary>
    //    /// The AdditionalTechnologies collection to sync with the ListView selections
    //    /// </summary>
    //    public ObservableCollection<ContributionTechnologyModel> AdditionalTechnologies
    //    {
    //        get => (ObservableCollection<ContributionTechnologyModel>)GetValue(AdditionalTechnologiesProperty);
    //        set => SetValue(AdditionalTechnologiesProperty, value);
    //    }

    //    private static void OnSelectedValuesChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
    //    {
    //        if (d is TechnologyAreasListView lv)
    //        {
    //            void Handler(object s, NotifyCollectionChangedEventArgs e) => OnAdditionalTechnologiesChanged(d, e);

    //            if (args.OldValue is INotifyCollectionChanged objects)
    //            {
    //                objects.CollectionChanged -= Handler;

    //                lv.SelectedItems.Clear();
    //            }

    //            if (args.NewValue is INotifyCollectionChanged collection)
    //            {
    //                if (lv.SelectedItems.Any())
    //                    lv.SelectedItems.Clear();

    //                foreach (ContributionTechnologyModel technology in lv.AdditionalTechnologies)
    //                {
    //                    foreach (ContributionTechnologyModel subArea in lv.Items)
    //                    {
    //                        if (subArea.Id != technology.Id)
    //                            continue;

    //                        if (!lv.SelectedItems.Contains(technology))
    //                            lv.SelectedItems.Add(technology);
    //                    }
    //                }

    //                collection.CollectionChanged += Handler;
    //            }

    //            Debug.WriteLine($"SelectedItems Count: {lv.SelectedItems.Count}");
    //        }
    //    }

    //    private void TechnologyAreasListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    //    {
    //        if (AdditionalTechnologies == null)
    //            return;
            
    //        foreach (var item in e.RemovedItems)
    //        {
    //            if (AdditionalTechnologies.Contains(item as ContributionTechnologyModel))
    //            {
    //                AdditionalTechnologies.Remove(item as ContributionTechnologyModel);
    //            }
    //        }

    //        foreach (var item in e.AddedItems)
    //        {
    //            if (!AdditionalTechnologies.Contains(item as ContributionTechnologyModel))
    //            {
    //                AdditionalTechnologies.Add(item as ContributionTechnologyModel);
    //            }
    //        }

    //        if (this.AdditionalTechnologies.Count > 2)
    //        {
    //            TaskUtilities.RunOnDispatcherThreadSync(async () =>
    //            {
    //                await new MessageDialog("You can only have 2 additional technologies selected").ShowAsync();
    //            });

    //            this.AdditionalTechnologies.Remove(AdditionalTechnologies.LastOrDefault());
    //        }
    //    }

    //    private static void OnAdditionalTechnologiesChanged(object sender, NotifyCollectionChangedEventArgs e)
    //    {
    //        if (sender is TechnologyAreasListView lv)
    //        {
    //            if (e.OldItems != null)
    //            {
    //                foreach (var item in e.OldItems)
    //                {
    //                    if (lv.SelectedItems.Contains(item))
    //                    {
    //                        lv.SelectedItems.Remove(item);
    //                    }
    //                }
    //            }

    //            if (e.NewItems != null)
    //            {
    //                foreach (var item in e.NewItems)
    //                {
    //                    if (!lv.SelectedItems.Contains(item))
    //                    {
    //                        lv.SelectedItems.Add(item);
    //                    }
    //                }
    //            }
    //        }
    //    }
    //}
}