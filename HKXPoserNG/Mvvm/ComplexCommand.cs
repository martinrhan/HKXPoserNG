using System;
using System.Windows.Input;

namespace HKXPoserNG.Mvvm;

public class ComplexCommand : ICommand {
    public ComplexCommand(Action execute, Func<bool> canExecute) {
        this.execute = execute ?? (() => { });
        this.canExecute = canExecute;
    }
    public event EventHandler? CanExecuteChanged;
     
    public void NotifyCanExecuteChanged() {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    private Func<bool> canExecute;
    public bool CanExecute(object? parameter) {
        return canExecute();
    }

    private Action execute;
    public void Execute(object? parameter) {
        execute();
    }
}