using Avalonia.Collections;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using HKXPoserNG.Mvvm;
using HKXPoserNG.Reactive;
using PropertyChanged.SourceGenerator;
using SingletonSourceGenerator.Attributes;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Sockets;
using System.Reactive;
using System.Reactive.Linq;

namespace HKXPoserNG.ViewModels;

[Singleton]
public partial class AnimationEditor {
    public AnimationEditor() {
        var observable_hasSelecetedModificationTrack = this.GetPropertyValueObservable(nameof(SelectedModificationTrack), ae => ae.SelectedModificationTrack != null);
        MenuItems = [
            new(Animation.Instance.AnimationChangedObservable.Select(u => true)) {
                Header = "Add Modification Track",
                Command = AddModificationTrackCommand,
                HotKey = new KeyGesture(Key.D1)
            },
            new(observable_hasSelecetedModificationTrack){
                Header = "Remove Modification Track",
                Command = RemoveModificationTrackCommand,
                HotKey = new KeyGesture(Key.D2)
            },
            new(
                Observable.CombineLatest(
                    observable_hasSelecetedModificationTrack,
                    this.GetPropertyValueObservable(nameof(SelectedKeyFrame), ae => ae.SelectedKeyFrame == null),
                    (hasSelecetedModificationTrack, hasNoSelectedKeyFrame) => hasSelecetedModificationTrack && hasNoSelectedKeyFrame
                )
            ){
                Header = "Add Key Frame",
                Command = AddKeyFrameCommand,
                HotKey = new KeyGesture(Key.D3)
            },
            new(this.GetPropertyValueObservable(nameof(SelectedKeyFrame), ae => ae.SelectedKeyFrame != null)){
                Header = "Remove Key Frame",
                Command= RemoveKeyFrameCommand,
                HotKey = new KeyGesture(Key.D4)
            },
            new(
                this.GetPropertyValueObservable(
                    nameof(SelectedKeyFrameInterval),
                    ae => ae.SelectedKeyFrameInterval?.GetPropertyValueObservable(
                            nameof(IKeyFrameInterval.InterpolationFunction),
                            kfi => kfi.InterpolationFunction == null
                        ) ?? Observable.Return(false)
                ).Switch()
            ){
                Header = "Add Interpolation",
                Command = AddInterpolationCommand,
                HotKey = new KeyGesture(Key.D5)
            },
            new(
                this.GetPropertyValueObservable(
                    nameof(SelectedKeyFrameInterval),
                    ae => ae.SelectedKeyFrameInterval?.GetPropertyValueObservable(
                            nameof(IKeyFrameInterval.InterpolationFunction),
                            kfi => kfi.InterpolationFunction != null
                        ) ?? Observable.Return(false)
                ).Switch()
            ){
                Header = "Remove Interpolation",
                Command = RemoveInterpolationCommand,
                HotKey = new KeyGesture(Key.D6)
            }
        ];
        var observable_selectedModificationTrack = this.GetPropertyValueObservable(nameof(SelectedModificationTrack), ae => ae.SelectedModificationTrack);
        var observable_keyFramesCollectionChanged =
            observable_selectedModificationTrack.Select(smt =>
                smt?.KeyFrames.GetCollectionChangedObservable() ?? Observable.Empty<EventPattern<NotifyCollectionChangedEventArgs>?>()
            ).Switch();
        var observable_currentFrame = Animation.Instance.GetPropertyValueObservable(nameof(Animation.CurrentFrame), a => a.CurrentFrame);
        Observable.CombineLatest(
            observable_selectedModificationTrack, observable_keyFramesCollectionChanged, observable_currentFrame,
            (smt, ep, cf) => smt?.KeyFrames.FirstOrDefault(kf => kf.Frame == cf)
        ).Subscribe(newValue => {
            SelectedKeyFrame = newValue;
            PropertyChanged?.Invoke(this, new(nameof(SelectedKeyFrame)));
        });
        Observable.CombineLatest(
            observable_currentFrame,
            observable_selectedModificationTrack,
            observable_selectedModificationTrack
            .Select(smt => smt?.KeyFrameIntervals.GetCollectionChangedObservable() ?? Observable.Empty<EventPattern<NotifyCollectionChangedEventArgs>>())
            .Switch(),
            (cf, smt, ep_kf) => {
                if (smt == null) return null;
                return smt.GetIntervalAtFrame(cf);
            }
        ).Subscribe(newValue => {
            SelectedKeyFrameInterval = newValue;
            PropertyChanged?.Invoke(this, new(nameof(SelectedKeyFrameInterval)));
        });
    }
    [Notify]
    private string? projectName = "Unamed HKXPoserNG Project";

    private AvaloniaList<AnimationModificationTrack> modificationTracks = new();
    public IAvaloniaReadOnlyList<AnimationModificationTrack> ModificationTracks => modificationTracks;

    [Notify]
    private AnimationModificationTrack? selectedModificationTrack;

    public MenuItemViewModel[] MenuItems { get; }

    private SimpleCommand AddModificationTrackCommand => field ??= new(() => {
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
        modificationTracks.Add(track);
        SelectedModificationTrack = track;
    });
    private SimpleCommand RemoveModificationTrackCommand => field ??= new(() => {
        SelectedModificationTrack!.Dispose();
        modificationTracks.Remove(SelectedModificationTrack);
        SelectedModificationTrack = modificationTracks.FirstOrDefault();
    });
    private SimpleCommand AddKeyFrameCommand => field ??= new(() => SelectedModificationTrack!.AddKeyFrame(Animation.Instance.CurrentFrame));
    private SimpleCommand RemoveKeyFrameCommand => field ??= new(() => SelectedModificationTrack!.RemoveKeyFrameAtFrame(Animation.Instance.CurrentFrame));
    private SimpleCommand AddInterpolationCommand => field ??= new(() => SelectedKeyFrameInterval?.InterpolationFunction = KeyFrameIntervalInterpolationFunctions.Linear);
    private SimpleCommand RemoveInterpolationCommand => field ??= new(() => SelectedKeyFrameInterval?.InterpolationFunction = null);

    public IKeyFrame? SelectedKeyFrame { get; private set; } = null;
    public IKeyFrameInterval? SelectedKeyFrameInterval { get; private set; } = null;

    public Transform GetBoneLocalModificationInSelectedTrack(Bone bone) {
        if (SelectedModificationTrack == null) return Transform.Identity;
        return SelectedModificationTrack.GetTransform(Animation.Instance.CurrentFrame, bone);
    }
    public void SetBoneLocalModificationInSelectedTrack(Bone bone, Transform value) {
        SelectedModificationTrack!.SetTransform(SelectedKeyFrame!, bone, value);
    }
    public void SetBoneLocalModificationInSelectedTrackIfPossible(Bone bone, Transform value) {
        if (SelectedModificationTrack == null || SelectedKeyFrame == null) return;
        SelectedModificationTrack.SetTransformIfPossible(SelectedKeyFrame, bone, value);
    }
    public Transform GetBoneLocalModificationAggregate(Bone bone) {
        Transform aggregate = Transform.Identity;
        foreach (var track in ModificationTracks) {
            aggregate *= track.GetTransform(Animation.Instance.CurrentFrame, bone);
        }
        return aggregate;
    }
}
