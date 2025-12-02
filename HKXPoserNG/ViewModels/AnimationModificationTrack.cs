using Avalonia.Collections;
using PropertyChanged.SourceGenerator;
using System;
using System.Linq;
using System.Collections.Specialized;
using System.Collections.Generic;
using HKXPoserNG.Reactive;
using System.Reflection.Emit;
using HKXPoserNG.Extensions;
using HKXPoserNG.ViewModels.AnimationModificationTrackRestricted;
using System.ComponentModel;

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

    private AvaloniaList<KeyFrame> keyFrames = new();
    public IAvaloniaReadOnlyList<IKeyFrame> KeyFrames => keyFrames;
    public KeyFrame? GetKeyFrameAtFrame(int frame) {
        return keyFrames.FirstOrDefault(kf => kf.Frame == frame);
    }

    private AvaloniaList<KeyFrameInterval> keyFrameIntervals = new();
    public IAvaloniaReadOnlyList<IKeyFrameInterval> KeyFrameIntervals => keyFrameIntervals;

    public void AddAffectedBone(Bone bone) {
        affectedBones.Add(bone);
        foreach (var keyFrame in keyFrames) {
            keyFrame.transforms.Add(Transform.Identity);
        }
    }
    public bool RemoveAffectedBone(Bone bone) {
        int i = affectedBones.IndexOf(bone);
        if (i == -1) return false;
        affectedBones.RemoveAt(i);
        foreach (var keyFrame in keyFrames) {
            keyFrame.transforms.RemoveAt(i);
        }
        return true;
    }
    public void AddKeyFrame(int frame) {
        int i_insert = Enumerable.Range(0, keyFrames.Count).FirstOrDefault(i => frame < keyFrames[i].Frame, keyFrames.Count);
        KeyFrame keyFrame = new(this) { Frame = frame };
        keyFrames.Insert(i_insert, keyFrame);
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
                    }
                    break;
            }
        }
    }
    public void RemoveKeyFrame(int frame) {
        int i_remove = Enumerable.Range(0, keyFrames.Count).FirstOrDefault(i => keyFrames[i].Frame == frame, -1);
        if (i_remove == -1) throw new InvalidOperationException();
        keyFrames.RemoveAt(i_remove);
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
    public void SetTransform(IKeyFrame keyFrame, Bone bone, Transform value) {
        SetTransform(keyFrame, affectedBones.IndexOf(bone), value);
    }
    public void SetTransform(IKeyFrame keyFrame, int i_affectedBone, Transform value) {
        ((KeyFrame)keyFrame).transforms[i_affectedBone] = value;
    }

    private IDisposable subscription;
    public void Dispose() {
        subscription.Dispose();
    }

    public void SetCurrentBoneLocalModification(Transform value) {
        if (AnimationEditor.Instance.SelectedKeyFrame == null) throw new InvalidOperationException();
        if (Skeleton.Instance.SelectedBone == null) throw new InvalidOperationException();
        KeyFrame keyFrame = (KeyFrame)AnimationEditor.Instance.SelectedKeyFrame;
        int i_bone = affectedBones.IndexOf(Skeleton.Instance.SelectedBone);
        keyFrame.transforms[i_bone] = value;
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