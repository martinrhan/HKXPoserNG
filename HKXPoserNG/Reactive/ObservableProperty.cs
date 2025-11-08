using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;

namespace HKXPoserNG.Reactive;

public interface IReadOnlyObservableProperty<T> : IObservable<T> {
    T Value { get; }
}

public class ObservableProperty<T> : IReadOnlyObservableProperty<T> {
    public ObservableProperty(T value) {
        Value = value;
    }
    public T Value {
        get => field;
        set {
            if (Equals(field, value))
                return;
            field = value;
            foreach (var observer in observers.ToArray())
                observer.OnNext(value);
        }
    }

    private List<IObserver<T>> observers = new List<IObserver<T>>();
    public IDisposable Subscribe(IObserver<T> observer) {
        observers.Add(observer);
        observer.OnNext(Value);
        return Disposable.Create(() => observers.Remove(observer));
    }
}

public record struct ValueChangedTuple<T>(T OldValue, T NewValue);