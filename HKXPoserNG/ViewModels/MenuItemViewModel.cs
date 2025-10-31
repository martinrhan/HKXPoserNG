using Avalonia.Input;
using Material.Icons.Avalonia;
using PropertyChanged.SourceGenerator;
using System.Windows.Input;

namespace HKXPoserNG.ViewModels;

public partial class MenuItemViewModel {
    public string Header { get; set; } = string.Empty;
    public MenuItemViewModel[]? Items { get; set; }
    public ICommand? Command { get; set; }
    public KeyGesture? HotKey { get; set; }
    public MaterialIcon? Icon { get; set; }

    [Notify]
    private bool canExecute = true;//Avalonia MenuItem and Button does not support the CanExecute in ICommand.
}