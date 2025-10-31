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
    
    public Transform LocalOriginal => Animation.Instance.Poses[Animation.Instance.CurrentFrame].Transforms.GetOrDefault(Index, Transform.Identity);
    
    public Transform LocalModification {
        get => AnimationEditor.Instance.GetCurrentBoneLocalModification(Index);
        set => AnimationEditor.Instance.SetCurrentBoneLocalModification(Index, value);
    }
    public bool IsCurrentlyModifiable => AnimationEditor.Instance.IsBoneCurrentlyModifiable(Index);
    

    public Transform LocalModified => LocalModification * LocalOriginal;
    public Transform GlobalModified => Skeleton.Instance.BoneGlobalModifiedTransforms.GetOrDefault(Index, Transform.Identity);

    [Notify]
    private bool hide = false;
}
