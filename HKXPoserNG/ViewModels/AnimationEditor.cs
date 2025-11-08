using Avalonia.Collections;
using Avalonia.Controls.Primitives;
using HKXPoserNG.Mvvm;
using PropertyChanged.SourceGenerator;
using SingletonSourceGenerator.Attributes;
using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;

namespace HKXPoserNG.ViewModels;

[Singleton]
public partial class AnimationEditor {
    public AnimationEditor() {
        MenuItems = [
            new() {
                Header = "Add Modification Track",
                Command = AddModificationTrackCommand,
                CanExecute = false
            },
            new(){
                Header = "Add Key Frame",
                Command = AddKeyFrameCommand,
                CanExecute = false
            }
        ];
        Animation.Instance.AnimationChanged.Subscribe(_ => MenuItems[0].CanExecute = true);
        Animation.Instance.PropertyChanged += (_, e) => {
            if (e.PropertyName == nameof(Animation.CurrentFrame)) {
                NotifySelectedKeyFrameChanged();
            }
        };
    }
    public Transform GetBoneLocalModification(int frame, int index) {
        return Transform.Identity;
    }
    public Transform GetCurrentBoneLocalModification(int index) {
        return GetBoneLocalModification(Animation.Instance.CurrentFrame, index);
    }
    public void SetBoneLocalModification(AnimationModificationKeyFrame keyFrame, int index, Transform value) {

    }

    internal void SetCurrentBoneLocalModification(int index, Transform value) {
    }

    [Notify]
    private string? projectName = "Unamed HKXPoserNG Project";

    private AvaloniaList<AnimationModificationTrack> modificationTracks = new();
    public IAvaloniaReadOnlyList<AnimationModificationTrack> ModificationTracks => modificationTracks;

    public AnimationModificationTrack? SelectedModificationTrack {
        get => modificationTracks.FirstOrDefault(t => t.IsSelected);
        set {
            foreach (var track in modificationTracks) {
                track.IsSelected = track == value;
            }
            OnSelectedModificationTrackChanged();
        }
    }
    private void OnSelectedModificationTrackChanged() {
        NotifySelectedKeyFrameChanged();
        PropertyChanged?.Invoke(this, new(nameof(SelectedModificationTrack)));
    }

    public MenuItemViewModel[] MenuItems { get; }
    private MenuItemViewModel GetAddKeyFrameMenuItem() => MenuItems[1];

    private SimpleCommand AddModificationTrackCommand => new(() => {
        var track = new AnimationModificationTrack();
        string name = "New Track";
        bool HasSameName() => modificationTracks.Any(t => t.Name == name);
        if (HasSameName()) {
            int suffix = 0;
            do {
                name = "New Track " + ++suffix;
            } while (HasSameName());
        }
        track.Name = name;
        track.KeyFrames.CollectionChanged += (_, _) => {
            NotifySelectedKeyFrameChanged();
        };
        modificationTracks.Add(track);
        SelectedModificationTrack = track;
    });

    private SimpleCommand AddKeyFrameCommand => new(() => SelectedModificationTrack!.AddKeyFrame(Animation.Instance.CurrentFrame));
    private bool CanAddKeyFrame() => SelectedKeyFrame == null;

    public AnimationModificationKeyFrame? SelectedKeyFrame => SelectedModificationTrack?.KeyFrames.FirstOrDefault(kf => kf.FrameIndex == Animation.Instance.CurrentFrame);
    private void NotifySelectedKeyFrameChanged() {
        GetAddKeyFrameMenuItem().CanExecute = CanAddKeyFrame();
        PropertyChanged?.Invoke(this, new(nameof(SelectedKeyFrame)));
    }
}

