using Avalonia.Threading;
using HKX2;
using HKXPoserNG.Extensions;
using HKXPoserNG.Mvvm;
using SingletonSourceGenerator.Attributes;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HKXPoserNG.ViewModels;

[Singleton]
public partial class MainViewModel {
    public MainViewModel() {
        OpenCommand = new SimpleCommand(Open);
        SaveCommand = new SimpleCommand(Save);
    }

    public Func<Task<FileInfo?>>? OpenFileDialogFunc { get; set; }

    public SimpleCommand OpenCommand { get; }
    private void Open() {
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

    public SimpleCommand SaveCommand { get; }
    private void Save() {
    }
}
