using HKXPoserNG.Extensions;
using NiflySharp;
using NiflySharp.Blocks;
using PropertyChanged.SourceGenerator;
using SingletonSourceGenerator.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Vortice.Direct3D11;
using Vortice.Win32;

namespace HKXPoserNG.ViewModels;

[Singleton]
public partial class Skeleton {
    public Skeleton() {
        NifFile nifFile = new NifFile();
        nifFile.Load(Path.Combine(PathConstants.DataDirectory, "skeleton.nif"));
        NiNode[] nodes = nifFile.Blocks.OfType<NiNode>().ToArray();
        bones = new Bone[nodes.Length];
        for (int i = 0; i < nodes.Length; i++) {
            NiNode node = nodes[i];
            Bone bone = new Bone() {
                Index = i,
                Name = node.Name.String,
                LocalTransform = new Transform(node.Translation, node.Rotation, node.Scale)
            };
            if (bones == null)
                bones = new Bone[nodes.Length];
            bones[i] = bone;
            dictionary[bone.Name] = bone;
        }
        for (int i = 0; i < nodes.Length; i++) {
            NiNode node = nodes[i];
            Bone bone = bones[i];
            foreach (var childNode in node.Children.GetBlocks(nifFile).OfType<NiNode>()) {
                Bone childBone = dictionary[childNode.Name.String];
                bone.Children.Add(childBone);
                childBone.Parent = bone;
            }
        }
        BoneVertexBuffer = DXObjects.D3D11Device.CreateBuffer(
            (uint)(Bones.Count * Marshal.SizeOf<Vector3>()),
            BindFlags.VertexBuffer,
            ResourceUsage.Dynamic,
            CpuAccessFlags.Write
        );
        List<short> list_lineIndices = new();
        void Recursion(Bone bone) {
            foreach (Bone child in bone.Children) {
                list_lineIndices.Add((short)bone.Index);
                list_lineIndices.Add((short)child.Index);
                Recursion(child);
            }
        }
        Recursion(Root[0]);
        LineIndexBuffer = DXObjects.D3D11Device.CreateBuffer(list_lineIndices.ToArray(), BindFlags.IndexBuffer, ResourceUsage.Immutable);
    }

    public string? Name { get; private set; }

    private Bone[] bones;
    public IReadOnlyList<Bone> Bones => bones!;

    public Bone[] Root => [Bones![0]];

    private Dictionary<string, Bone> dictionary = new();
    public IReadOnlyDictionary<string, Bone> Dictionary => dictionary;

    public ID3D11Buffer LineIndexBuffer { get; private set; }
    public ID3D11Buffer BoneVertexBuffer { get; private set; }

    public void UpdateBonePositions() {
        Vector3[] result = new Vector3[Bones.Count];
        for (int i = 0; i < Bones.Count; i++) {
            Vector4 vec = new(0, 0, 0, 1);
            vec = Vector4.Transform(vec, Bones[i].WorldTransformMatrix);
            result[i] = vec.AsVector3();
        }
        DXObjects.D3D11Device.ImmediateContext.WriteBuffer(BoneVertexBuffer, result);
    }

    [Notify]
    private Bone? selectedBone;
}
