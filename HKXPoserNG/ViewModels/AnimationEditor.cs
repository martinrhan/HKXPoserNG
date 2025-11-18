using Avalonia.Collections;
using Avalonia.Controls.Primitives;
using HKXPoserNG.Mvvm;
using HKXPoserNG.Reactive;
using PropertyChanged.SourceGenerator;
using SingletonSourceGenerator.Attributes;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace HKXPoserNG.ViewModels;

[Singleton]
public partial class AnimationEditor {
    public AnimationEditor() {
        GetObservable_SelectedKeyFrameIndex().Subscribe(ChangedSelectedKeyFrame);
        var observable_hasSelecetedModificationTrack = this.GetPropertyObservable(nameof(SelectedModificationTrack), ae => ae.SelectedModificationTrack != null);
        MenuItems = [
            new(Animation.Instance.AnimationChangedObservable.Select(u => true)) {
                Header = "Add Modification Track",
                Command = AddModificationTrackCommand,
            },
            new(observable_hasSelecetedModificationTrack){
                Header = "Remove Modification Track",
                Command = RemoveModificationTrackCommand
            },
            new(Observable.CombineLatest(
                observable_hasSelecetedModificationTrack,
                this.GetPropertyObservable(nameof(SelectedKeyFrame), ae => ae.SelectedKeyFrame == -1),
                (hasSelecetedModificationTrack, hasNoSelectedKeyFrame) => hasSelecetedModificationTrack && hasNoSelectedKeyFrame
            )){
                Header = "Add Key Frame",
                Command = AddKeyFrameCommand,
            },
            new(this.GetPropertyObservable(nameof(SelectedKeyFrame), ae => ae.SelectedKeyFrame != -1)){
                Header = "Remove Key Frame",
                Command= RemoveKeyFrameCommand,
            }
        ];
    }
    public Transform GetBoneLocalModification(int frame, int index) {
        return Transform.Identity;
    }
    public Transform GetCurrentBoneLocalModification(int index) {
        return GetBoneLocalModification(Animation.Instance.CurrentFrame, index);
    }
    public void SetBoneLocalModification(AnimationModificationTrack track, int keyFrameIndex, int boneIndex, Transform value) {

    }

    internal void SetCurrentBoneLocalModification(int index, Transform value) {
    }

    [Notify]
    private string? projectName = "Unamed HKXPoserNG Project";

    private AvaloniaList<AnimationModificationTrack> modificationTracks = new();
    public IAvaloniaReadOnlyList<AnimationModificationTrack> ModificationTracks => modificationTracks;

    [Notify]
    private AnimationModificationTrack? selectedModificationTrack;

    public MenuItemViewModel[] MenuItems { get; }

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
        modificationTracks.Add(track);
        SelectedModificationTrack = track;
    });
    private SimpleCommand RemoveModificationTrackCommand => new(() => {
        SelectedModificationTrack!.Dispose();
        modificationTracks.Remove(SelectedModificationTrack);
        SelectedModificationTrack = modificationTracks.FirstOrDefault();
    });
    private SimpleCommand AddKeyFrameCommand => new(() => SelectedModificationTrack!.AddKeyFrame(Animation.Instance.CurrentFrame));
    private SimpleCommand RemoveKeyFrameCommand => new(() => SelectedModificationTrack!.RemoveKeyFrame(Animation.Instance.CurrentFrame));

    public int SelectedKeyFrame { get; private set; }
    private IObservable<int> GetObservable_SelectedKeyFrameIndex() {
        var observable_collectionChanged =
            this.GetPropertyObservable(nameof(SelectedModificationTrack), ae => ae.SelectedModificationTrack?.KeyFrames).
            Select(list => list?.
                GetCollectionChangedObservable().
                StartWith(new EventPattern<NotifyCollectionChangedEventArgs>(list, new(NotifyCollectionChangedAction.Reset))) ??
                Observable.Return<EventPattern<NotifyCollectionChangedEventArgs>?>(null)
            ).
            Switch();
        var observable_currentFrame = Animation.Instance.GetPropertyObservable(nameof(Animation.CurrentFrame), a => a.CurrentFrame);
        return Observable.CombineLatest(observable_collectionChanged, observable_currentFrame,
            (ep, cf) => (ep?.Sender as IAvaloniaReadOnlyList<int>)?.FirstOrDefault(kf => kf == cf, -1) ?? -1
        );
    }
    private void ChangedSelectedKeyFrame(int newValue) {
        SelectedKeyFrame = newValue;
        PropertyChanged?.Invoke(this, new(nameof(SelectedKeyFrame)));
    }
}
