using Avalonia.Collections;
using PropertyChanged.SourceGenerator;
using System;
using System.Linq;
using System.Collections.Specialized;
using System.Collections.Generic;
using HKXPoserNG.Reactive;

namespace HKXPoserNG.ViewModels;

public partial class AnimationModificationTrack : IDisposable {
    public AnimationModificationTrack() {
        subscription = AnimationEditor.Instance.
             GetPropertyObservable(nameof(AnimationEditor.SelectedModificationTrack), ae => ae.SelectedModificationTrack).
             Subscribe(smt => this.IsSelected = smt == this);
    }
    [Notify]
    private string name = string.Empty;

    [Notify(set: Setter.Private)]
    private bool isSelected = false;

    private AvaloniaList<Bone> affectedBones = new();
    public IAvaloniaReadOnlyList<Bone> AffectedBones => affectedBones;

    private AvaloniaList<int> keyFrames = new();
    public IAvaloniaReadOnlyList<int> KeyFrames => keyFrames;

    private List<List<Transform>> transforms = new();
    public IReadOnlyList<IReadOnlyList<Transform>> Transforms => transforms;

    private AvaloniaList<Func<Transform, Transform, float, Transform>> interpolationFunctions = new();
    public IAvaloniaReadOnlyList<Func<Transform, Transform, float, Transform>> InterpolationFunctions => interpolationFunctions;

    public void AddAffectedBone(Bone bone) {
        affectedBones.Add(bone);
        foreach (var list in transforms) {
            list.Add(Transform.Identity);
        }
    }
    public bool RemoveAffectedBone(Bone bone) {
        int i = affectedBones.IndexOf(bone);
        if (i == -1) return false;
        affectedBones.RemoveAt(i);
        foreach (var list in transforms) {
            list.RemoveAt(i);
        }
        return true;
    }

    public void AddKeyFrame(int frameIndex) {
        int i_insert = Enumerable.Range(0, keyFrames.Count).FirstOrDefault(i => frameIndex < keyFrames[i]);
        keyFrames.Insert(i_insert, frameIndex);
        List<Transform> list = [.. Enumerable.Repeat(Transform.Identity, AffectedBones.Count)];
        transforms.Insert(i_insert, list);
    }

    private IDisposable subscription;
    public void Dispose() {
        subscription.Dispose();
    }
}

public static class AnimationIntervalInterpolationFunctions {
    public static Func<Transform, Transform, float, Transform> Linear { get; } = (t0, t1, f) => {
        return Transform.Lerp(t0, t1, f);
    };
}
