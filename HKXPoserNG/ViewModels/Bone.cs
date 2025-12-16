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

namespace HKXPoserNG.ViewModels;

public partial class Bone {
    public Bone() {
        Dispatcher.UIThread.Post(() => {
            Skeleton.Instance.
                GetPropertyValueObservable(nameof(Skeleton.SelectedBone), s => s.SelectedBone).
                Subscribe(selectedBone => {
                    IsSelected = selectedBone == this;
                    OnPropertyChanged(new(nameof(IsSelected)));
                });
            AnimationEditor.Instance.
                GetPropertyValueObservable(nameof(AnimationEditor.SelectedModificationTrack), ae => ae.SelectedModificationTrack).
                Select(smt => smt?.AffectedBones.GetCollectionChangedObservable() ?? Observable.Return<EventPattern<NotifyCollectionChangedEventArgs>>(new(null, new(NotifyCollectionChangedAction.Reset)))).
                Switch().
                Subscribe(ep => {
                    IsAffectedBySelectedModificationTrack = (ep.Sender as IAvaloniaReadOnlyList<Bone>)?.Contains(this) ?? false;
                    OnPropertyChanged(new(nameof(IsAffectedBySelectedModificationTrack)));
                });
        });
    }

    public string Name { get; init; } = string.Empty;
    public int Index { get; init; }
    public Bone? Parent { get; set; }
    public List<Bone> Children { get; } = new List<Bone>();

    public Transform LocalOriginal => Animation.Instance.Poses[Animation.Instance.CurrentFrame].Transforms.GetOrDefault(Index, Transform.Identity);

    public Transform LocalModification {
        get => AnimationEditor.Instance.GetCurrentBoneLocalModification(Index);
        set => AnimationEditor.Instance.SetCurrentBoneLocalModificationIfPossible(Index, value);
    }

    public Transform LocalModified => LocalModification * LocalOriginal;
    public Transform GlobalModified => Skeleton.Instance.BoneGlobalModifiedTransforms.GetOrDefault(Index, Transform.Identity);

    [Notify]
    private bool hide = false;

    public bool IsAffectedBySelectedModificationTrack { get; private set; }
    public bool IsSelected { get; private set; } = false;

    public override string ToString() {
        return Index.ToString() + "  " + Name;
    }
}
