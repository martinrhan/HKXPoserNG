using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using HKXPoserNG.Reactive;
using HKXPoserNG.ViewModels;

namespace HKXPoserNG.Views;

public partial class AnimationEditorAffectedBonesView : UserControl {
    public AnimationEditorAffectedBonesView() {
        InitializeComponent();
        this[DataContextProperty.Bind()] = AnimationEditor.Instance.GetPropertyValueObservable(
            nameof(AnimationEditor.SelectedModificationTrack),
            ae => ae.SelectedModificationTrack
        ).ToBinding();
    }     
}