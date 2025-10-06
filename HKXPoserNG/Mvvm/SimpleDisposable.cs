using System;

namespace HKXPoserNG.Mvvm;

public class SimpleDisposable : IDisposable {
    private Action disposeAction;
    public SimpleDisposable(Action disposeAction) {
        this.disposeAction = disposeAction;
    }
    public void Dispose() {
        disposeAction();
    }
}