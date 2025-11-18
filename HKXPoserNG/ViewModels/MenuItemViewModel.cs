using Avalonia.Input;
using Material.Icons.Avalonia;
using System;
using System.ComponentModel;
using System.Windows.Input;

namespace HKXPoserNG.ViewModels;

public class MenuItemViewModel : INotifyPropertyChanged, IDisposable {
    public MenuItemViewModel(IObservable<bool>? canExecuteObservable = null) {
        if (canExecuteObservable == null) {
            CanExecute = true;
        } else {
            subscription = canExecuteObservable.Subscribe(b => {
                CanExecute = b;
                PropertyChanged?.Invoke(this, new(nameof(CanExecute)));
            });
        }
    }
    public string Header { get; set; } = string.Empty;
    public MenuItemViewModel[]? Items { get; set; }
    public ICommand? Command { get; set; }
    public KeyGesture? HotKey { get; set; }
    public MaterialIcon? Icon { get; set; }

    private IDisposable? subscription;
    public bool CanExecute { get; private set; }    //Avalonia MenuItem and Button does not support the CanExecute in ICommand.

    public event PropertyChangedEventHandler? PropertyChanged;

    public void Dispose() {
        subscription?.Dispose();
    }
}