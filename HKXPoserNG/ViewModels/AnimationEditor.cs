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
                GetAddKeyFrameMenuItem().CanExecute = CanAddKeyFrame();
            }
        };
    }
    public Transform GetBoneLocalModification(int frame, int index) {
        return Transform.Identity;
    }
    public Transform GetCurrentBoneLocalModification(int index) {
        return GetBoneLocalModification(Animation.Instance.CurrentFrame, index);
    }

    internal bool IsBoneCurrentlyModifiable(int index) {
        return true;
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
        GetAddKeyFrameMenuItem().CanExecute = CanAddKeyFrame();
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
            GetAddKeyFrameMenuItem().CanExecute = CanAddKeyFrame();
        }; 
        modificationTracks.Add(track);
        SelectedModificationTrack = track;
    });

    private SimpleCommand AddKeyFrameCommand => new(() => SelectedModificationTrack!.AddKeyFrame(Animation.Instance.CurrentFrame));
    private bool CanAddKeyFrame() => SelectedModificationTrack == null ? false : SelectedModificationTrack.KeyFrames.All(kf => kf.FrameIndex != Animation.Instance.CurrentFrame);

}

public partial class AnimationModificationTrack {
    [Notify]
    private string name = string.Empty;

    [Notify]
    private bool isSelected = false;

    private AvaloniaList<Bone> affectedBones = new();
    public IAvaloniaReadOnlyList<Bone> AffectedBones => affectedBones;

    private AvaloniaList<AnimationModificationKeyFrame> keyFrames = new();
    public IAvaloniaReadOnlyList<AnimationModificationKeyFrame> KeyFrames => keyFrames;

    private AvaloniaList<Func<Transform, Transform, float, Transform>> interpolationFunctions = new();
    public IAvaloniaReadOnlyList<Func<Transform, Transform, float, Transform>> InterpolationFunctions => interpolationFunctions;

    public void AddKeyFrame(int frameIndex) {
        var keyFrame = new AnimationModificationKeyFrame {
            FrameIndex = frameIndex
        };
        keyFrame.Transforms.AddRange(Enumerable.Repeat(Transform.Identity, affectedBones.Count));
        keyFrames.Add(keyFrame);
    }
}

public class AnimationModificationKeyFrame {
    public int FrameIndex { get; set; }
    public AvaloniaList<Transform> Transforms { get; } = new();
}


public static class AnimationIntervalInterpolationFunctions {
    public static Func<Transform, Transform, float, Transform> Linear { get; } = (t0, t1, f) => {
        return Transform.Lerp(t0, t1, f);
    };
}

