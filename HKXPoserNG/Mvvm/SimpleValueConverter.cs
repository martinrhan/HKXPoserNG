using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace HKXPoserNG.Mvvm;

public class SimpleValueConverter<U, V> : IValueConverter {
    public SimpleValueConverter(Func<U, V> convert, Func<V, U>? convertBack) {
        this.convert = convert;
        this.convertBack = convertBack;
    }

    private readonly Func<U, V> convert;
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (targetType is not V) return null;
        if (value is U u)
            return convert(u);
        else 
            return null;
    }

    private readonly Func<V, U>? convertBack;
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (convertBack is null) return null;
        if (targetType is not V) return null;
        if (value is  V v)
            return convertBack(v);
        else
            return null;
    }
}