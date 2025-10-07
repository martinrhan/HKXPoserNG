using Avalonia.Input;
using Avalonia.Threading;
using HKXPoserNG.Extensions;
using HKXPoserNG.Mvvm;
using Material.Icons.Avalonia;
using SingletonSourceGenerator.Attributes;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Material.Icons;

namespace HKXPoserNG.ViewModels;

[Singleton]
public partial class MenuViewModel {
    public MenuViewModel() {
    }
    public MenuItemViewModel[] Items { get; } = [
        new (){
            Header = "File",
            Items = [
                new () {
                    Header = "Open",
                    Command = new SimpleCommand(Open),
                    HotKey = new (Key.O, KeyModifiers.Control),
                    Icon = new (){Kind = MaterialIconKind.FolderOpen}
                },
                new () {
                    Header = "Save",
                    Command = null,
                    HotKey = new (Key.S, KeyModifiers.Control),
                    Icon = new (){Kind = MaterialIconKind.ContentSave}
                },
            ]
        },
    ];

    public static Func<Task<FileInfo?>>? OpenFileDialogFunc { get; set; }
    private static void Open() {
        if (OpenFileDialogFunc is null)
            throw new InvalidOperationException("OpenFileDialogFunc is not set.");
        Dispatcher.UIThread.WaitForTaskAndContinue(
            OpenFileDialogFunc(),
            (fileInfo) => {
                if (fileInfo is null) return;
                if (!string.Equals(fileInfo.Extension, ".hkx", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("The selected file is not a .hkx file. Please select a valid .hkx file.");
                if (!fileInfo.Exists)
                    throw new FileNotFoundException("The selected file does not exist.", fileInfo.FullName);
                Animation.Instance.LoadFromHKX(fileInfo);
            }
        );
    }
}

public class MenuItemViewModel {
    public string Header { get; set; } = string.Empty;
    public MenuItemViewModel[]? Items { get; set; }
    public ICommand? Command { get; set; }
    public KeyGesture? HotKey { get; set; }
    public MaterialIcon? Icon { get; set; }
}