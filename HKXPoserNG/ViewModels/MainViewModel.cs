using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using HKXPoserNG.Extensions;
using SingletonSourceGenerator.Attributes;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HKXPoserNG.ViewModels;

[Singleton]
public partial class MainViewModel {
    public Func<Task<FileInfo?>>? OpenFileDialogFunc { get; set; }

    [RelayCommand]
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
                    string path_copy = Path.Combine(GlobalConstants.HKXDirectory, fileInfo.Name);
                    fileInfo.CopyTo(path_copy, true);
                    //After combating weirld bug that hkdump cannot work, I found that the input cannot be in same directory, and also cannot somewhere too far.
                    string name_no_ext = fileInfo.Name[..^4];
                    string path_bin = Path.Combine(GlobalConstants.TempDirectory, $"{name_no_ext}.bin");
                    ExternalPrograms.HKDump(path_copy, path_bin);
                    OpenedAnimation = new HKAAnimation();
                    OpenedAnimation.Load(path_bin);
                }
            );
    }

    [RelayCommand]
    private void Save() {
    }

    public HKAAnimation? OpenedAnimation { get; private set; }
}
