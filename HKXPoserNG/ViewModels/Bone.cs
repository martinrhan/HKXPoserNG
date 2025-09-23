using HKXPoserNG.Extensions;
using PropertyChanged.SourceGenerator;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace HKXPoserNG.ViewModels;

public partial class Bone {
    public string Name { get; init; } = string.Empty;
    public int Index { get; init; }
    public Bone? Parent { get; set; }
    public List<Bone> Children { get; } = new List<Bone>();

    public Transform LocalTransform { get; set; } = Transform.Identity;

    public Matrix4x4 WorldTransformMatrix => Parent == null ? LocalTransform.Matrix : LocalTransform.Matrix * Parent.WorldTransformMatrix;

    [Notify]
    private bool hide = false;
}
