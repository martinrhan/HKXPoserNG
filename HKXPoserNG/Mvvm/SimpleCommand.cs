using System;
using System.Reactive;
using System.Windows.Input;

namespace HKXPoserNG.Mvvm;

public class SimpleCommand : ICommand {
    public SimpleCommand(Action execute) {
        this.execute = execute ?? (() => { });
    }
    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) {
        return true;
    }

    private Action execute;
    public void Execute(object? parameter) {
        execute();
    }
}

public class ObserverCommand : ICommand {
    public ObserverCommand(Action execute, bool initialCanExecute) {
        this.execute = execute ?? (() => { });
        canExecute = initialCanExecute;
        CanExecuteObserver = Observer.Create<bool>(
            b => {
                canExecute = b;
                CanExecuteChanged?.Invoke(this, new());
            }
        );
    }
    public event EventHandler? CanExecuteChanged;
     
    public IObserver<bool> CanExecuteObserver { get; }

    private bool canExecute;
    public bool CanExecute(object? parameter) {
        return canExecute;
    }

    private Action execute;
    public void Execute(object? parameter) {
        execute();
    }
}