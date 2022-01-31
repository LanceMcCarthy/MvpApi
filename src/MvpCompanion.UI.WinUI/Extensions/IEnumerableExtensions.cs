using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MvpCompanion.UI.WinUI.Extensions;

public static class EnumerableExtensions
{
    public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> list)
    {
        return new ObservableCollection<T>(list);
    }

    public static bool TryAdd<K, V>(this IDictionary<K, V> dictionary, K key, V value)
    {
        if (dictionary.ContainsKey(key))
        {
            return false;
        }

        try
        {
            dictionary.Add(key, value);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static int AddRange<T>(this ObservableCollection<T> list, IEnumerable<T> items, bool clearFirst = false)
    {
        if (clearFirst)
        {
            list.Clear();
        }

        foreach (T item in items)
        {
            list.Add(item);
        }

        return list.Count;
    }

    public static T AddAndReturn<T>(this IList<T> list, T item)
    {
        list.Add(item);
        return item;
    }

    public static void ForEach<T>(this IEnumerable<T> list, Action<T> action)
    {
        foreach (T item in list)
        {
            action?.Invoke(item);
        }
    }
}