 using Avalonia;
using HKX2;
using HKXPoserNG.Extensions;
using HKXPoserNG.Mvvm;
using NiflySharp;
using NiflySharp.Blocks;
using PropertyChanged.SourceGenerator;
using SingletonSourceGenerator.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using Vortice.Direct3D11;
using Vortice.Win32;

namespace HKXPoserNG.ViewModels;

[Singleton]
public partial class Skeleton {
    public Skeleton() {
        using (Stream stream = File.OpenRead(Path.Combine(PathConstants.DataDirectory, "skeleton.bin")))
        using (BinaryReader reader = new BinaryReader(stream, System.Text.Encoding.Default)) {
            string head = reader.ReadHeaderString();
            uint version = reader.ReadUInt32();
            // should be 0x01000200
            int nskeletons = reader.ReadInt32();
            // should be 1 or 2
            var name = reader.ReadCString();
            // A user name to aid in identifying the skeleton

            /// Parent relationship
            int nparentIndices = reader.ReadInt32();
            var parentIndices = new short[nparentIndices];
            for (int i = 0; i < nparentIndices; i++) {
                parentIndices[i] = reader.ReadInt16();
            }

            /// Bones for this skeleton
            int nbones = reader.ReadInt32();
            var boneNames = new string[nbones];
            boneNames.Populate(i => reader.ReadCString());

            /// The reference pose for the bones of this skeleton. This pose is stored in local space.
            int nreferencePose = reader.ReadInt32();
            var referencePose = new Transform[nreferencePose];
            referencePose.Populate(i => new Transform(reader));
            Animation.Instance.poses = [new(0, referencePose, Array.Empty<float>())];

            this.bones = new Bone[nbones];
            bones.Populate(i => new Bone() {
                Index = i,
                Name = boneNames[i], 
                Parent = parentIndices[i] == -1 ? null : bones[parentIndices[i]],
            });
            foreach (Bone bone in bones) {
                if (bone.Parent != null) {
                    bone.Parent.Children.Add(bone);
                }
                boneDictionary[bone.Name] = bone;
            }

            /// The reference values for the float slots of this skeleton. This pose is stored in local space.
            int nreferenceFloats = reader.ReadInt32();
            var referenceFloats = new float[nreferenceFloats];
            referenceFloats.Populate(i => reader.ReadSingle());

            /// Floating point track slots. Often used for auxiliary float data or morph target parameters etc.
            /// This defines the target when binding animations to a particular rig.
            int nfloatSlots = reader.ReadInt32();
            var floatSlots = new string[nfloatSlots];
            floatSlots.Populate(i => reader.ReadCString());

            int nanimations = reader.ReadInt32();
        }

        UpdateBoneGlobalTransforms();

        BoneVertexBuffer = DXObjects.D3D11Device.CreateBuffer(
            (uint)(Bones.Count * Marshal.SizeOf<Vector3>()),
            BindFlags.VertexBuffer,
            ResourceUsage.Dynamic,
            CpuAccessFlags.Write
        );
        UpdateBoneVertexBuffer();
        List<short> list_lineIndices = new();
        void Recursion_SetLineIndices(Bone bone) {
            foreach (Bone child in bone.Children) {
                list_lineIndices.Add((short)bone.Index);
                list_lineIndices.Add((short)child.Index);
                Recursion_SetLineIndices(child);
            }
        }
        Recursion_SetLineIndices(Root[0]);
        LineIndexBuffer = DXObjects.D3D11Device.CreateBuffer(list_lineIndices.ToArray(), BindFlags.IndexBuffer, ResourceUsage.Immutable);

        Animation.Instance.PropertyChanged += (s, e) => {
            if (e.PropertyName == nameof(Animation.CurrentFrame)) {
                UpdateBoneGlobalTransforms();
                UpdateBoneVertexBuffer();
            }
        };
        Animation.Instance.AnimationChangedObservable.Subscribe(_ => {
            UpdateBoneGlobalTransforms();
            UpdateBoneVertexBuffer();
        });
    }

    public string? Name { get; private set; }

    private Bone[] bones;
    public IReadOnlyList<Bone> Bones => bones!;

    public Bone[] Root => [Bones![0]];

    private Dictionary<string, Bone> boneDictionary = new();
    public IReadOnlyDictionary<string, Bone> BoneDictionary => boneDictionary;

    private Transform[]? boneGlobalModifiedTransforms;
    public IReadOnlyList<Transform> BoneGlobalModifiedTransforms => boneGlobalModifiedTransforms!;

    private void UpdateBoneGlobalTransforms() {
        boneGlobalModifiedTransforms = new Transform[bones.Length];
        void Recursion_UpdateBoneGlobalTransforms(Bone bone) {
            if (bone.Parent == null) {
                boneGlobalModifiedTransforms[bone.Index] = bone.LocalModified;
            } else {
                boneGlobalModifiedTransforms[bone.Index] = bone.LocalModified * boneGlobalModifiedTransforms[bone.Parent.Index];
            }
            foreach (Bone child in bone.Children) {
                Recursion_UpdateBoneGlobalTransforms(child);
            }
        }
        Recursion_UpdateBoneGlobalTransforms(Root[0]);
    }

    public ID3D11Buffer LineIndexBuffer { get; private set; }
    public ID3D11Buffer BoneVertexBuffer { get; private set; }

    private void UpdateBoneVertexBuffer() {
        Vector3[] result = new Vector3[Bones.Count];
        for (int i = 0; i < Bones.Count; i++) {
            Vector4 vec = new(0, 0, 0, 1);
            vec = Vector4.Transform(vec, BoneGlobalModifiedTransforms[i].Matrix);
            result[i] = vec.AsVector3();
        }
        DXObjects.D3D11Device.ImmediateContext.WriteBuffer(BoneVertexBuffer, result);
    }

    [Notify]
    private Bone? selectedBone;


}
