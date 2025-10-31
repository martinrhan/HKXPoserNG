using Avalonia.Controls;
using HKXPoserNG.ViewModels;
using Avalonia.Platform.Storage;
using System.IO;
using System;
using System.Linq;
using Avalonia.LogicalTree;

namespace HKXPoserNG.Views;

public partial class MainView : UserControl {
    public MainView() {
        MenuViewModel.OpenFileDialogFunc = () => {
            var options = new FilePickerOpenOptions {
                Title = "Open HKX file",
                FileTypeFilter = [
                    new FilePickerFileType("HKX") { Patterns = ["*.hkx"] }
                ],
                AllowMultiple = false
            };
            var task_openFile = TopLevel.GetTopLevel(this)!.StorageProvider.OpenFilePickerAsync(options);
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
        menu.Loaded += (_, _) => {
            menu.GetLogicalChildren().OfType<MenuItem>().First().IsSubMenuOpen = true;
        };
    }
}
