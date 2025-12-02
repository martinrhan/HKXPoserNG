using Avalonia.Collections;
using PropertyChanged.SourceGenerator;
using System;
using System.Linq;

namespace HKXPoserNG.ViewModels.AnimationModificationTrackRestricted;

public partial class KeyFrame : IKeyFrame {
    internal KeyFrame(AnimationModificationTrack parent) {
        transforms = [.. Enumerable.Repeat(Transform.Identity, parent.AffectedBones.Count)];
    }
    [Notify]
    private int frame;

    internal AvaloniaList<Transform> transforms;
    public IAvaloniaReadOnlyList<Transform> Transforms => transforms;

    public override string ToString() {
        return frame.ToString(); 
    }
}

public partial class KeyFrameInterval : IKeyFrameInterval{
    [Notify]
    private Func<Transform, Transform, float, Transform>? interpolationFunction;

    internal KeyFrameInterval Copy() {
        return new KeyFrameInterval {
            interpolationFunction = this.interpolationFunction,
        };
    }
}
