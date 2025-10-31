using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Vortice.Mathematics;

namespace HKXPoserNG.Extensions;

public static class MathFExtensions {

    public static float Square(this float x) => x * x;

    public static bool AreApproximatelyEqual(float a, float b, float tolerance = 0.0001f) {
        return MathF.Abs(a - b) <= tolerance;
    }
}

public static class IReadOnlyListExtensions {
    public static T GetOrDefault<T>(this IReadOnlyList<T> list, int index, T defaultValue = default!) {
        if (index < 0 || index >= list.Count)
            return defaultValue;
        return list[index];
    }
}