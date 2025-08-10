using Avalonia.Controls;
using HKXPoserNG.ViewModels;
using Avalonia.Platform.Storage;
using System.IO;
using System;
using System.Linq;

namespace HKXPoserNG.Views;

public partial class MainView : UserControl {
    public MainView() {
        MainViewModel.Instance.OpenFileDialogFunc = () => {
            var task_openFile = TopLevel.GetTopLevel(this)!.StorageProvider.OpenFilePickerAsync(new() );
            return task_openFile.ContinueWith(t => Continuation(t.Result.FirstOrDefault()));
            FileInfo? Continuation(IStorageFile? file) {
                if (file is null) return null;
                string? path = file.TryGetLocalPath();
                if (path is null)
                    throw new InvalidOperationException("Failed to get local path from storage file.");
                return new FileInfo(path);
            } 
        };
        DataContext = MainViewModel.Instance;
        InitializeComponent();
    }
}
