using System;
using System.Collections;
using System.Collections.Generic;

namespace HKXPoserNG.Extensions;

public static class ArrayExtensions {
    public static void Populate<T>(this T[] array, Func<int, T> func) {
        for (int i = 0; i < array.Length; i++) {
            array[i] = func(i);
        }
    }
    public static void Populate<T>(this IList<T> array, Func<int, T> func) {
        for (int i = 0; i < array.Count; i++) {
            array[i] = func(i);
        }
    }
    public static int IndexOf<T>(this IEnumerable<T> enumerable, T value) {
        int index = 0;
        foreach (var item in enumerable) {
            if (EqualityComparer<T>.Default.Equals(item, value)) {
                return index;
            }
            index++;
        }
        return -1;
    }
}
