using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;

namespace MvpApi.Uwp.Behaviors
{
    public class SelectedItemsBindingBehavior : Behavior<ListViewBase>
    {
        private bool selectionChangedInProgress;

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SelectionChanged += OnListViewSelectionChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.SelectionChanged -= OnListViewSelectionChanged;
        }

        public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.Register(
            "SelectedItems",
            typeof(IList),
            typeof(SelectedItemsBindingBehavior),
            new PropertyMetadata(new ObservableCollection<object>(), PropertyChangedCallback));

        public IList SelectedItems
        {
            get => (IList)GetValue(SelectedItemsProperty);
            set => SetValue(SelectedItemsProperty, value);
        }

        private static void PropertyChangedCallback(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            if (o is SelectedItemsBindingBehavior behavior)
            {
                void Handler(object s, NotifyCollectionChangedEventArgs e) => OnBoundSelectedItemsChanged(behavior, e);

                if (args.OldValue is INotifyCollectionChanged objects)
                {
                    objects.CollectionChanged -= Handler;

                    behavior.AssociatedObject.SelectedItems.Clear();
                }

                if (args.NewValue is INotifyCollectionChanged collection)
                {
                    foreach (var item in behavior.SelectedItems)
                    {
                        if (!behavior.AssociatedObject.SelectedItems.Contains(item))
                        {
                            behavior.AssociatedObject.SelectedItems.Add(item);
                        }
                    }

                    collection.CollectionChanged += Handler;
                }
            }
        }

        /// <summary>
        /// Handles selection changes in the ListView
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (selectionChangedInProgress)
                return;

            selectionChangedInProgress = true;

            foreach (var item in e.RemovedItems)
            {
                if (SelectedItems.Contains(item))
                {
                    SelectedItems.Remove(item);
                }
            }

            foreach (var item in e.AddedItems)
            {
                if (!SelectedItems.Contains(item))
                {
                    SelectedItems.Add(item);
                }
            }

            selectionChangedInProgress = false;
        }

        /// <summary>
        /// Handles item changes in the bound collection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnBoundSelectedItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (sender is SelectedItemsBindingBehavior behavior)
            {
                var listSelectedItems = behavior.AssociatedObject.SelectedItems;

                if (e.OldItems != null)
                {
                    foreach (var item in e.OldItems)
                    {
                        if (listSelectedItems.Contains(item))
                        {
                            listSelectedItems.Remove(item);
                        }
                    }
                }

                if (e.NewItems != null)
                {
                    foreach (var item in e.NewItems)
                    {
                        if (!listSelectedItems.Contains(item))
                        {
                            listSelectedItems.Add(item);
                        }
                    }
                }
            }
        }
    }
}
