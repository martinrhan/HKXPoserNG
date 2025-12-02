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
}
