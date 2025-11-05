using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using HKXPoserNG.ViewModels;

namespace HKXPoserNG.Views;

public partial class AnimationEditorAffectedBonesView : UserControl {
    public AnimationEditorAffectedBonesView() {
        InitializeComponent();
        AnimationEditor.Instance.PropertyChanged += (_, e) => {
            if (e.PropertyName == nameof(AnimationEditor.Instance.SelectedModificationTrack)) {
                DataContext = AnimationEditor.Instance.SelectedModificationTrack;
            }
        };
        DataContext = AnimationEditor.Instance.SelectedModificationTrack;
    }


}