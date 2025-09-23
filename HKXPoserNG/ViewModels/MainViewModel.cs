using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using HKXPoserNG.Extensions;
using PropertyChanged.SourceGenerator;
using SingletonSourceGenerator.Attributes;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HKXPoserNG.ViewModels;

[Singleton]
public partial class MainViewModel {
    public MainViewModel() {
         string path_animation_idle = Path.Combine(PathConstants.ResourcesDirectory, "idle.bin");
        LoadedAnimation = new Animation();
        LoadedAnimation.Load(path_animation_idle);
    }

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
                    string path_out_hct = Path.Combine(PathConstants.TempDirectory, fileInfo.Name);
                    ExternalPrograms.HCT(fileInfo.FullName, path_out_hct);
                    string name_no_ext = fileInfo.Name[..^4];
                    string path_out_hkdump = Path.Combine(PathConstants.TempDirectory, $"{name_no_ext}.bin");
                    ExternalPrograms.HKDump(path_out_hct, path_out_hkdump);
                    LoadedAnimation = new Animation();
                    LoadedAnimation.Load(path_out_hkdump);
                    OnPropertyChanged(new(nameof(LoadedAnimation)));
                }
            );
    }

    [RelayCommand]
    private void Save() {
    }

    public Animation LoadedAnimation { get; private set; }

    [Notify]
    private int currentFrame = 0;
}
