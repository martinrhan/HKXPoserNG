using Avalonia.Threading;
using HKXPoserNG.Extensions;
using HKXPoserNG.Reactive;
using PropertyChanged.SourceGenerator;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Numerics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Avalonia.Collections;
using System.Diagnostics;

namespace HKXPoserNG.ViewModels;

public partial class Bone {
    public Bone() {
        Dispatcher.UIThread.Post(() => {
            Animation.Instance
            .GetPropertyValueObservable(nameof(Animation.CurrentFrame), a => a.CurrentFrame)
            .Subscribe(_ => {
                LocalOriginal = Animation.Instance.Poses[Animation.Instance.CurrentFrame].Transforms.GetOrDefault(Index, Transform.Identity);
                OnPropertyChanged(new(nameof(LocalOriginal)));
                OnPropertyChanged(new(nameof(LocalModificationInSelectedTrack)));
                OnPropertyChanged(new(nameof(LocalModified)));
            });
            Observable.CombineLatest(
                AnimationEditor.Instance.GetPropertyValueObservable(nameof(AnimationEditor.Instance.SelectedModificationTrack), ae => ae.SelectedModificationTrack)
                .Select(smt => smt?.TransformChangedObservable ?? Observable.Empty<ModificationTrackTransformChangedEventArgs>())
                .Switch()
                .Where(mttcea => mttcea.ContainsFrame(Animation.Instance.CurrentFrame) && mttcea.ContainsBone(this)),
                Animation.Instance.GetPropertyValueObservable(nameof(Animation.CurrentFrame), a => a.CurrentFrame),
                (mttcea, cf) => mttcea.ModificationTrack.GetTransform(cf, this)
            ).Subscribe(transform => {
                if (Transform.ApproximatelyEquals(LocalModificationInSelectedTrack, transform)) return;
                LocalModificationInSelectedTrack = transform;
                OnPropertyChanged(new(nameof(LocalModificationInSelectedTrack)));
                OnPropertyChanged(new(nameof(LocalModified)));
            });
            Observable.CombineLatest(
                AnimationEditor.Instance.ModificationTracks
                .GetItemsObservable(t => t.TransformChangedObservable)
                .Select(t => t.Item2)
                .Where(mttcea => mttcea.ContainsFrame(Animation.Instance.CurrentFrame) && mttcea.ContainsBone(this)),
                Animation.Instance.GetPropertyValueObservable(nameof(Animation.CurrentFrame), a => a.CurrentFrame),
                (mttcea, cf) => AnimationEditor.Instance.GetBoneLocalModificationAggregate(this)
            ).Subscribe(transform => {
                if (Transform.ApproximatelyEquals(LocalModificationAggregate, transform)) return;
                LocalModificationAggregate = transform;
                OnPropertyChanged(new(nameof(LocalModificationAggregate)));
            });
            Skeleton.Instance
            .GetPropertyValueObservable(nameof(Skeleton.SelectedBone), s => s.SelectedBone)
            .Subscribe(selectedBone => {
                IsSelected = selectedBone == this;
                OnPropertyChanged(new(nameof(IsSelected)));
            });
            AnimationEditor.Instance
            .GetPropertyValueObservable(nameof(AnimationEditor.SelectedModificationTrack), ae => ae.SelectedModificationTrack)
            .Select(smt => smt?.AffectedBones.GetCollectionChangedObservable() ?? Observable.Return<EventPattern<NotifyCollectionChangedEventArgs>>(new(null, new(NotifyCollectionChangedAction.Reset))))
            .Switch()
            .Subscribe(ep => {
                bool value = (ep.Sender as IAvaloniaReadOnlyList<Bone>)?.Contains(this) ?? false;
                if (IsAffectedBySelectedModificationTrack == value) return;
                IsAffectedBySelectedModificationTrack = value;
                OnPropertyChanged(new(nameof(IsAffectedBySelectedModificationTrack)));
            });
        });
    }

    public string Name { get; init; } = string.Empty;
    public int Index { get; init; }
    public Bone? Parent { get; set; }
    public List<Bone> Children { get; } = new List<Bone>();

    public Transform LocalOriginal { get; private set; } = Transform.Identity;

    public Transform LocalModificationInSelectedTrack { get; private set; } = Transform.Identity;
    public void SetLocalModification(Transform value) => AnimationEditor.Instance.SetBoneLocalModificationInSelectedTrackIfPossible(this, value);

    public Transform LocalModificationAggregate { get; private set; } = Transform.Identity;

    public Transform LocalModified => LocalModificationAggregate * LocalOriginal;
    public Transform GlobalModified => Skeleton.Instance.BoneGlobalModifiedTransforms.GetOrDefault(Index, Transform.Identity);

    [Notify]
    private bool hide = false;

    public bool IsSelected { get; private set; } = false;
    public bool IsAffectedBySelectedModificationTrack { get; private set; }

    public override string ToString() {
        return Index.ToString() + "  " + Name;
    }
}
