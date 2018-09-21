using System.Collections;
using System.Collections.Specialized;
using Windows.UI.Xaml.Controls;

namespace MvpApi.Uwp.Helpers
{
    public class SelectedItemsBinder
    {
        private readonly ListView listView;
        private readonly IList collection;

        public SelectedItemsBinder(ListView listView, IList collection)
        {
            this.listView = listView;
            this.collection = collection;

            this.listView.SelectedItems.Clear();

            foreach (var item in this.collection)
            {
                this.listView.SelectedItems.Add(item);
            }
        }

        public void Bind()
        {
            listView.SelectionChanged += ListView_SelectionChanged;

            if (collection is INotifyCollectionChanged observable)
            {
                observable.CollectionChanged += Collection_CollectionChanged;
            }
        }

        public void UnBind()
        {
            if (listView != null)
                listView.SelectionChanged -= ListView_SelectionChanged;

            if (collection is INotifyCollectionChanged observable)
            {
                observable.CollectionChanged -= Collection_CollectionChanged;
            }
        }

        private void Collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (var item in e.NewItems ?? new object[0])
            {
                if (!listView.SelectedItems.Contains(item))
                    listView.SelectedItems.Add(item);
            }

            foreach (var item in e.OldItems ?? new object[0])
            {
                listView.SelectedItems.Remove(item);
            }
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var item in e.AddedItems ?? new object[0])
            {
                if (!collection.Contains(item))
                    collection.Add(item);
            }

            foreach (var item in e.RemovedItems ?? new object[0])
            {
                collection.Remove(item);
            }
        }
    }
}
