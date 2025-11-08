using HKXPoserNG.Mvvm;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;

namespace HKXPoserNG.Reactive;

public class SimpleObservable<T> : IObservable<T> {
    private List<IObserver<T>> observers = new();

    public IDisposable Subscribe(IObserver<T> observer) {
        observers.Add(observer);
        return Disposable.Create(() => observers.Remove(observer));
    }

    public void Notify(T value) {
        foreach (var observer in observers) {
            observer.OnNext(value);
        }
    }
}

