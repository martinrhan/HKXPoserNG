using Avalonia.Collections;
using HKXPoserNG.Extensions;
using HKXPoserNG.Reactive;
using HKXPoserNG.ViewModels.AnimationModificationTrackRestricted;
using PropertyChanged.SourceGenerator;
using System;
using System.ComponentModel;
using System.Linq;

namespace HKXPoserNG.ViewModels;

public partial class AnimationModificationTrack : IDisposable {
    public AnimationModificationTrack() {
        subscription = AnimationEditor.Instance.
             GetPropertyValueObservable(nameof(AnimationEditor.SelectedModificationTrack), ae => ae.SelectedModificationTrack).
             Subscribe(smt => this.IsSelected = smt == this);
    }
    [Notify]
    private string name = string.Empty;

    [Notify(set: Setter.Private)]
    private bool isSelected = false;

    private AvaloniaList<Bone> affectedBones = new();
    public IAvaloniaReadOnlyList<Bone> AffectedBones => affectedBones;

    private AvaloniaList<KeyFrame> keyFrames = new();
    public IAvaloniaReadOnlyList<IKeyFrame> KeyFrames => keyFrames;
    public KeyFrame? GetKeyFrameAtFrame(int frame) {
        return keyFrames.FirstOrDefault(kf => kf.Frame == frame);
    }

    private AvaloniaList<KeyFrameInterval> keyFrameIntervals = new();
    public IAvaloniaReadOnlyList<IKeyFrameInterval> KeyFrameIntervals => keyFrameIntervals;

    public void AddAffectedBone(Bone bone) {
        int i_insert = Enumerable.Range(0, affectedBones.Count).FirstOrDefault(i => bone.Index < affectedBones[i].Index, affectedBones.Count);
        affectedBones.Insert(i_insert, bone);
        foreach (var keyFrame in keyFrames) {
            keyFrame.transforms.Insert(i_insert, Transform.Identity);
        }
    }
    public bool RemoveAffectedBone(Bone bone) {
        int i = affectedBones.IndexOf(bone);
        if (i == -1) return false;
        affectedBones.RemoveAt(i);
        foreach (var keyFrame in keyFrames) {
            keyFrame.transforms.RemoveAt(i);
        }
        observable_transformChanged.Notify(new(this, bone, 0, int.MaxValue));
        return true;
    }
    public IKeyFrame AddKeyFrame(int frame) {
        int i_insert = Enumerable.Range(0, keyFrames.Count).FirstOrDefault(i => frame < keyFrames[i].Frame, keyFrames.Count);
        KeyFrame keyFrame = new(this) { Frame = frame };
        keyFrames.Insert(i_insert, keyFrame);
        bool isInsertingIntoInterpolatedInterval = false;
        if (keyFrames.Count > 1) {
            switch (i_insert) {
                case 0: // first
                    keyFrameIntervals.Insert(0, new());
                    break;
                case int x when x == keyFrames.Count - 1: // last 
                    keyFrameIntervals.Add(new());
                    break;
                default: // middle
                    int i_splittedInterval = i_insert - 1;
                    var splittedInterval = keyFrameIntervals[i_splittedInterval];
                    keyFrameIntervals.Insert(i_insert, splittedInterval.Copy());
                    if (splittedInterval.InterpolationFunction != null) {
                        Func<int, Transform> selector = bi => {
                            var t0 = keyFrames[i_insert - 1].transforms[bi];
                            var t1 = keyFrames[i_insert + 1].transforms[bi];
                            float t = (frame - keyFrames[i_insert - 1].Frame) / (keyFrames[i_insert + 1].Frame - keyFrames[i_insert - 1].Frame);
                            return splittedInterval.InterpolationFunction(t0, t1, t);
                        };
                        keyFrame.transforms.Populate(selector);
                        isInsertingIntoInterpolatedInterval = true;
                    }
                    break;
            }
        }
        if (isInsertingIntoInterpolatedInterval) {
            observable_transformChanged.Notify(new(this, null, keyFrames[i_insert - 1].Frame + 1, keyFrames[i_insert + 1].Frame - 1));
        } else {
            observable_transformChanged.Notify(new(this, null, frame));
        }
        return keyFrame;
    }
    public void RemoveKeyFrameAtFrame(int frame) {
        int i_remove = Enumerable.Range(0, keyFrames.Count).FirstOrDefault(i => keyFrames[i].Frame == frame, -1);
        if (i_remove == -1) throw new InvalidOperationException();
        RemoveKeyFrameAtIndex(i_remove);
    }
    public void RemoveKeyFrame(IKeyFrame keyFrame) {
        RemoveKeyFrameAtIndex(KeyFrames.IndexOf(keyFrame));
    }
    public void RemoveKeyFrameAtIndex(int i_keyFrame) {
        IKeyFrame keyFrame = keyFrames[i_keyFrame];
        int frameStart, frameEnd;
        switch (i_keyFrame) {
            case 0:
                frameStart = keyFrame.Frame;
                frameEnd = keyFrameIntervals[0].InterpolationFunction == null ? keyFrames[0].Frame : keyFrames[1].Frame - 1;
                keyFrames.RemoveAt(0);
                keyFrameIntervals.RemoveAt(0);
                break;
            case int x when x == keyFrames.Count - 1:
                frameStart = keyFrameIntervals[^1].InterpolationFunction == null ? keyFrames[^1].Frame : keyFrames[^2].Frame + 1;
                frameEnd = keyFrame.Frame;
                keyFrames.RemoveAt(keyFrames.Count - 1);
                keyFrameIntervals.RemoveAt(keyFrameIntervals.Count - 1);
                break;
            default:
                (IKeyFrameInterval? interval0, IKeyFrameInterval? interval1) = GetIntervalsBesideKeyFrame(keyFrames[i_keyFrame]);
                frameStart = interval0?.InterpolationFunction == null ? keyFrame.Frame : keyFrames[i_keyFrame - 1].Frame + 1;
                frameEnd = interval1?.InterpolationFunction == null ? keyFrame.Frame : keyFrames[i_keyFrame + 1].Frame - 1;
                keyFrames.RemoveAt(i_keyFrame);
                keyFrameIntervals.RemoveAt(i_keyFrame);
                break;
        }
        observable_transformChanged.Notify(new(this, null, frameStart, frameEnd));
    }
    public int GetIntervalIndexAtFrame(int frame) {
        if (keyFrames.Count < 2) return -1;
        for (int i = 0; i < keyFrames.Count - 1; i++) {
            if (keyFrames[i].Frame < frame && frame < keyFrames[i + 1].Frame) {
                return i;
            }
        }
        return -1;
    }
    public IKeyFrameInterval? GetIntervalAtFrame(int frame) {
        int i_interval = GetIntervalIndexAtFrame(frame);
        if (i_interval == -1) return null;
        return keyFrameIntervals[i_interval];
    }
    public (IKeyFrame, IKeyFrame) GetKeyFramesBesideInterval(IKeyFrameInterval interval) {
        int i_interval = keyFrameIntervals.IndexOf((KeyFrameInterval)interval);
        return (keyFrames[i_interval], keyFrames[i_interval + 1]);
    }
    public (IKeyFrameInterval?, IKeyFrameInterval?) GetIntervalsBesideKeyFrame(IKeyFrame keyFrame) {
        int i_keyFrame = keyFrames.IndexOf((KeyFrame)keyFrame);
        IKeyFrameInterval? interval0 = i_keyFrame > 0 ? keyFrameIntervals[i_keyFrame - 1] : null;
        IKeyFrameInterval? interval1 = i_keyFrame < keyFrames.Count - 1 ? keyFrameIntervals[i_keyFrame] : null;
        return (interval0, interval1);
    }
    public bool IsIntervalAtomic(IKeyFrameInterval interval) {
        int i_interval = keyFrameIntervals.IndexOf((KeyFrameInterval)interval);
        return keyFrames[i_interval + 1].Frame - keyFrames[i_interval].Frame == 1;
    }
    public Transform GetTransform(int frame, Bone bone) {
        int i_bone = affectedBones.IndexOf(bone);
        if (i_bone == -1) return Transform.Identity;
        KeyFrame? keyFrame = GetKeyFrameAtFrame(frame);
        if (keyFrame != null) {
            return GetTransform(keyFrame, i_bone);
        }
        int i_interval = GetIntervalIndexAtFrame(frame);
        if (i_interval == -1) return Transform.Identity;
        var interval = keyFrameIntervals[i_interval];
        if (interval.InterpolationFunction != null) {
            var (kf0, kf1) = GetKeyFramesBesideInterval(interval);
            float t = (frame - kf0.Frame) / (float)(kf1.Frame - kf0.Frame);
            return interval.InterpolationFunction(GetTransform(kf0, i_bone), GetTransform(kf1, i_bone), t);
        } else {
            return Transform.Identity;
        }
    }
    public Transform GetTransform(IKeyFrame keyFrame, Bone bone) {
        int i_bone = affectedBones.IndexOf(bone);
        if (i_bone == -1) return Transform.Identity;
        return GetTransform(keyFrame, i_bone);
    }
    public Transform GetTransform(IKeyFrame keyFrame, int i_affectedBone) {
        return ((KeyFrame)keyFrame).transforms[i_affectedBone];
    }
    public void SetTransformIfPossible(IKeyFrame keyFrame, Bone bone, Transform value) {
        int i_bone = affectedBones.IndexOf(bone);
        if (i_bone == -1) return;
        SetTransform(keyFrame, i_bone, value);
    }
    public void SetTransform(IKeyFrame keyFrame, Bone bone, Transform value) {
        SetTransform(keyFrame, affectedBones.IndexOf(bone), value);
    }
    public void SetTransform(IKeyFrame keyFrame, int i_affectedBone, Transform value) {
        ((KeyFrame)keyFrame).transforms[i_affectedBone] = value;
        (IKeyFrameInterval? interval0, IKeyFrameInterval? interval1) = GetIntervalsBesideKeyFrame(keyFrame);
        if (interval0?.InterpolationFunction == null && interval1?.InterpolationFunction == null)
            observable_transformChanged.Notify(new(this, affectedBones[i_affectedBone], keyFrame.Frame));
        else {
            int i_keyFrame = keyFrames.IndexOf((KeyFrame)keyFrame);
            int frameStart = interval0?.InterpolationFunction == null ? keyFrame.Frame : keyFrames[i_keyFrame - 1].Frame + 1;
            int frameEnd = interval1?.InterpolationFunction == null ? keyFrame.Frame : keyFrames[i_keyFrame + 1].Frame - 1;
            observable_transformChanged.Notify(new(this, affectedBones[i_affectedBone], frameStart, frameEnd));
        }
    }

    private SimpleObservable<ModificationTrackTransformChangedEventArgs> observable_transformChanged = new();
    public IObservable<ModificationTrackTransformChangedEventArgs> TransformChangedObservable => observable_transformChanged;

    private IDisposable subscription;
    public void Dispose() {
        subscription.Dispose();
    }

}

public class ModificationTrackTransformChangedEventArgs : EventArgs {
    public AnimationModificationTrack ModificationTrack { get; }
    public Bone? Bone { get; }
    public bool IsAllBoneAffected => Bone == null;
    public bool ContainsBone(Bone bone) => IsAllBoneAffected || Bone == bone;
    public int FrameStart { get; }
    public int FrameEnd { get; }
    public bool IsWholeTrackAffected => FrameStart == 0 && FrameEnd == int.MaxValue;
    public bool ContainsFrame(int frame) => FrameStart <= frame && frame <= FrameEnd;
    public ModificationTrackTransformChangedEventArgs(AnimationModificationTrack modificationTrack, Bone? affectedBone, int frame) {
        ModificationTrack = modificationTrack;
        Bone = affectedBone;
        FrameStart = frame;
        FrameEnd = frame;
    }
    public ModificationTrackTransformChangedEventArgs(AnimationModificationTrack modificationTrack, Bone? affectedBone, int frameStart, int frameEnd) {
        ModificationTrack = modificationTrack;
        Bone = affectedBone;
        FrameStart = frameStart;
        FrameEnd = frameEnd;
    }
}

public static class KeyFrameIntervalInterpolationFunctions {
    public static Func<Transform, Transform, float, Transform> Linear { get; } = (t0, t1, f) => {
        return Transform.Lerp(t0, t1, f);
    };
}

public interface IKeyFrame : INotifyPropertyChanged {
    public int Frame { get; set; }
    public IAvaloniaReadOnlyList<Transform> Transforms { get; }
}

public interface IKeyFrameInterval : INotifyPropertyChanged {
    public Func<Transform, Transform, float, Transform>? InterpolationFunction { get; set; }
}