using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive;
using System.Reactive.Linq;
using Avalonia.Collections;
using System.Collections.Specialized;
using System.Reactive.Disposables;

namespace HKXPoserNG.Reactive;

public static class ReactiveExtensions {
    public static IObservable<EventPattern<PropertyChangedEventArgs>> GetObservable(this INotifyPropertyChanged source, string propertyName) {
        return Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
            h => source.PropertyChanged += h,
            h => source.PropertyChanged -= h).
            Where(e => e.EventArgs.PropertyName == propertyName);
    }

    public static IObservable<TProperty> GetPropertyObservable<TSource, TProperty>(this TSource source, string propertyName, Func<TSource, TProperty> propertyGetter) where TSource : INotifyPropertyChanged {
        return source.GetObservable(propertyName)
            .Select(_ => propertyGetter(source))
            .StartWith(propertyGetter(source));
    }

    public static IObservable<EventPattern<NotifyCollectionChangedEventArgs>> GetObservable<T>(this IAvaloniaReadOnlyList<T> list) {
        return Observable.FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
            h => list.CollectionChanged += h,
            h => list.CollectionChanged -= h);
    }

    public static IObservable<T> WhereNotNull<T>(this IObservable<T?> source) {
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
        return source.Where<T>(t => t is not null);
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
    }
}
