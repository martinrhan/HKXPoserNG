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
using System.Reactive;
using System.Reactive.Linq;

namespace HKXPoserNG.ViewModels;

[Singleton]
public partial class AnimationEditor {
    public AnimationEditor() {
        GetObservable_SelectedKeyFrameIndex().Subscribe(ChangedSelectedKeyFrame);
        GetObservable_SelectedInterpolationInterval().Subscribe(ChangedSelectedKeyFrameIntervalIndex);
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
    }
    public Transform GetBoneLocalModification(Bone bone) {
        if (SelectedModificationTrack == null || SelectedKeyFrame == null) return Transform.Identity;
        return SelectedModificationTrack.GetTransform(SelectedKeyFrame, bone);
    }
    public void SetBoneLocalModification(Bone bone, Transform value) {
        SelectedModificationTrack!.SetTransform(SelectedKeyFrame!, bone, value);
    }
    public void SetBoneLocalModificationIfPossible(Bone bone, Transform value) {
        if (SelectedModificationTrack == null || SelectedKeyFrame == null) return;
        SelectedModificationTrack.SetTransform(SelectedKeyFrame, bone, value); 
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
    private SimpleCommand RemoveKeyFrameCommand => field ??= new(() => SelectedModificationTrack!.RemoveKeyFrame(Animation.Instance.CurrentFrame));
    private SimpleCommand AddInterpolationCommand => field ??= new(() => SelectedKeyFrameInterval?.InterpolationFunction = KeyFrameIntervalInterpolationFunctions.Linear);
    private SimpleCommand RemoveInterpolationCommand => field ??= new(() => SelectedKeyFrameInterval?.InterpolationFunction = null);

    private IObservable<AnimationModificationTrack?> observable_selectedModificationTrack =>
        field ??= this.GetPropertyValueObservable(nameof(SelectedModificationTrack), ae => ae.SelectedModificationTrack);
    private IObservable<EventPattern<NotifyCollectionChangedEventArgs>?> observable_keyFramesCollectionChanged =>
        field ??= observable_selectedModificationTrack.
            Select(smt => smt?.KeyFrames.
                GetCollectionChangedObservable() ??
                Observable.Empty<EventPattern<NotifyCollectionChangedEventArgs>?>()
            ).
            Switch();
    private IObservable<int> observable_currentFrame => field ??=
        Animation.Instance.GetPropertyValueObservable(nameof(Animation.CurrentFrame), a => a.CurrentFrame);

    public IKeyFrame? SelectedKeyFrame { get; private set; } = null;
    private IObservable<IKeyFrame?> GetObservable_SelectedKeyFrameIndex() {
        return Observable.CombineLatest(observable_selectedModificationTrack, observable_keyFramesCollectionChanged, observable_currentFrame,
            (smt, ep, cf) => smt?.KeyFrames.FirstOrDefault(kf => kf.Frame == cf)
        );
    }
    private void ChangedSelectedKeyFrame(IKeyFrame? newValue) {
        SelectedKeyFrame = newValue;
        PropertyChanged?.Invoke(this, new(nameof(SelectedKeyFrame)));
    }

    public IKeyFrameInterval? SelectedKeyFrameInterval { get; private set; } = null;
    private IObservable<IKeyFrameInterval?> GetObservable_SelectedInterpolationInterval() {
        return Observable.CombineLatest(
            observable_currentFrame,
            observable_selectedModificationTrack,
            observable_selectedModificationTrack.
            Select(smt => smt?.KeyFrameIntervals.GetCollectionChangedObservable() ?? Observable.Empty<EventPattern<NotifyCollectionChangedEventArgs>>()).
            Switch(),
            (cf, smt, ep_kf) => {
                if (smt == null) return null;
                return smt.GetIntervalAtFrame(cf);
            }
        );
    }
    private void ChangedSelectedKeyFrameIntervalIndex(IKeyFrameInterval? newValue) {
        SelectedKeyFrameInterval = newValue;
        PropertyChanged?.Invoke(this, new(nameof(SelectedKeyFrameInterval)));
    }

}
