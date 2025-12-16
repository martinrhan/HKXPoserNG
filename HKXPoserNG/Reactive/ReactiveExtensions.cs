using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using Avalonia.Collections;
using System.Collections.Specialized;

namespace HKXPoserNG.Reactive;

public static class ReactiveExtensions {
    public static IObservable<EventPattern<PropertyChangedEventArgs>> GetPropertyChangedObservable(this INotifyPropertyChanged source, string propertyName) {
        return Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
            h => source.PropertyChanged += h,
            h => source.PropertyChanged -= h).
            Where(e => e.EventArgs.PropertyName == propertyName);
    }

    public static IObservable<T> StartWithFunc<T>(this IObservable<T> source, Func<T> getter) {
        return Observable.Create<T>(observer => {
            observer.OnNext(getter());
            return source.Subscribe(observer);
        });
    }

    public static IObservable<TProperty> GetPropertyValueObservable<TSource, TProperty>(this TSource source, string propertyName, Func<TSource, TProperty> propertyGetter) where TSource : INotifyPropertyChanged {
        return source.GetPropertyChangedObservable(propertyName)
            .Select(_ => propertyGetter(source))
            .StartWithFunc(() => propertyGetter(source));
    }

    public static IObservable<EventPattern<NotifyCollectionChangedEventArgs>> GetCollectionChangedObservable<T>(this IAvaloniaReadOnlyList<T> list) {
        return Observable.FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
            h => list.CollectionChanged += h,
            h => list.CollectionChanged -= h);
    }

    public static IObservable<T> WhereNotNull<T>(this IObservable<T?> source) {
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
        return source.Where<T>(t => t is not null);
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
    }

    public static IObservable<(TItem, T)> GetItemsObservable<TList, TItem, T>(this TList list, Func<TItem, IObservable<T>> observableGetter) where TList : notnull, IReadOnlyList<TItem>, INotifyCollectionChanged where TItem : notnull {
        return new CollectionItemsObservable<TList, TItem, T>(list, observableGetter);
    }

    public static IObservable<(TItem, T)> GetItemsObservable<TItem, T>(this IAvaloniaReadOnlyList<TItem> list, Func<TItem, IObservable<T>> observableGetter) where TItem : notnull {
        return GetItemsObservable<IAvaloniaReadOnlyList<TItem>, TItem, T>(list, observableGetter);
    }

    private class CollectionItemsObservable<TList, TItem, T> : IObservable<(TItem, T)> where TList : notnull, IReadOnlyList<TItem>, INotifyCollectionChanged where TItem : notnull {
        public CollectionItemsObservable(TList list, Func<TItem, IObservable<T>> observableGetter) {
            this.list = list;
            this.observableGetter = observableGetter;
        }
        private TList list;
        private Func<TItem, IObservable<T>> observableGetter;

        public IDisposable Subscribe(IObserver<(TItem, T)> observer) {
            Dictionary<TItem, IDisposable> subscriptions = new();
            void AddSubscription(TItem item) {
                IDisposable subscription = observableGetter(item).Select(o => (item, o)).Subscribe(observer);
                subscriptions.Add(item, subscription);
            }   
            void RemoveSubscription(TItem item) {
                subscriptions[item].Dispose();
                subscriptions.Remove(item);
            }
            foreach (TItem item in list) AddSubscription(item);
            list.CollectionChanged += List_CollectionChanged;
            void List_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
                switch (e.Action) {
                    case NotifyCollectionChangedAction.Add:
                        foreach (TItem item in e.NewItems!) AddSubscription(item);
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        foreach (TItem item in e.OldItems!) RemoveSubscription(item);
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        foreach (TItem item in e.OldItems!) RemoveSubscription(item);
                        foreach (TItem item in e.NewItems!) AddSubscription(item);
                        break;
                    case NotifyCollectionChangedAction.Move:
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        foreach (TItem item in e.OldItems!) RemoveSubscription(item);
                        foreach (TItem item in e.NewItems!) AddSubscription(item);
                        break;
                }
            }
            void Unsubscribe() {
                foreach (IDisposable subscription in subscriptions.Values) {
                    subscription.Dispose();
                }
                list.CollectionChanged -= List_CollectionChanged;
            }
            return Disposable.Create(Unsubscribe);
        }
    }

    
}

public class ObserverBox<T> : IDisposable{
    public ObserverBox(IObservable<T> observable) {
        subscription = observable.Subscribe(t => Value = t);
    }

    public T? Value { get; private set; }

    private IDisposable subscription;

    public void Dispose() {
        subscription.Dispose();
    }
}