using System;
using System.Collections.Generic;

namespace HKXPoserNG.Mvvm;

public class SimpleObservable<T> : IObservable<T> {
    private List<IObserver<T>> observers = new();

    public IDisposable Subscribe(IObserver<T> observer) {
        observers.Add(observer);
        return new SimpleDisposable(() => observers.Remove(observer));
    }

    public void Notify(T value) {
        foreach (var observer in observers) {
            observer.OnNext(value);
        }
    }
}
