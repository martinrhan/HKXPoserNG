using Avalonia.Collections;
using PropertyChanged.SourceGenerator;
using System;
using System.Linq;

namespace HKXPoserNG.ViewModels;

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
