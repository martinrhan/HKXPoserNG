using System;
using System.Collections.Generic;

namespace HKXPoserNG.Extensions;

public static class DictionaryExtensions {
    public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dict, IEnumerable<TKey> keys, IEnumerable<TValue> values) {
        using var keyEnum = keys.GetEnumerator();
        using var valueEnum = values.GetEnumerator();
        while (keyEnum.MoveNext() && valueEnum.MoveNext()) {
            dict.Add(keyEnum.Current, valueEnum.Current);
        }
    }

    public static void ToDictionaryAddRange<TSource, TKey, TValue>(this IEnumerable<TSource> source, IDictionary<TKey, TValue> dict, Func<TSource, TKey> keySelector, Func<TSource, TValue> valueSelector) {
        foreach (var item in source) {
            dict.Add(keySelector(item), valueSelector(item));
        }
    }

    public static void ToDictionaryAddRangeIgnoreDuplicateKeys<TSource, TKey, TValue>(this IEnumerable<TSource> source, IDictionary<TKey, TValue> dict, Func<TSource, TKey> keySelector, Func<TSource, TValue> valueSelector) {
        foreach (var item in source) {
            TKey key = keySelector(item);
            dict.TryAdd(key, valueSelector(item));
        }
    }
}