using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using HKXPoserNG.ViewModels;

namespace HKXPoserNG.Views;

public partial class AnimationEditorMenu : UserControl {
    public AnimationEditorMenu() {
        this.DataContext = AnimationEditor.Instance;
        InitializeComponent();
    }
}