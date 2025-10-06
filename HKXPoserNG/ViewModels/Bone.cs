using HKXPoserNG.Extensions;
using PropertyChanged.SourceGenerator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace HKXPoserNG.ViewModels;

public partial class Bone {
    public string Name { get; init; } = string.Empty;
    public int Index { get; init; }
    public Bone? Parent { get; set; }
    public List<Bone> Children { get; } = new List<Bone>();

    //public Transform BaseLocalTransform { get; init; } = Transform.Identity;

    //public Transform AnimationLocalTransform => Animation.Instance.Poses[Animation.Instance.CurrentFrame].Transforms.GetOrDefault(Index, Transform.Identity);

    //public Transform LocalTransform => BaseLocalTransform * AnimationLocalTransform;
    
    public Transform LocalTransform => Animation.Instance.Poses[Animation.Instance.CurrentFrame].Transforms.GetOrDefault(Index, Transform.Identity);

    public Transform GlobalTransform => Skeleton.Instance.BoneGlobalTransforms.GetOrDefault(Index, Transform.Identity);

    [Notify]
    private bool hide = false;
}
