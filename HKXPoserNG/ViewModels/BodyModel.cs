using HKXPoserNG.Extensions;
using NiflySharp;
using NiflySharp.Blocks;
using NiflySharp.Enums;
using NiflySharp.Structs;
using SingletonSourceGenerator.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Vortice.Direct3D11;

namespace HKXPoserNG.ViewModels;

[Singleton]
public partial class BodyModel {
    public BodyModel() {
        List<Mesh> meshList = new();

        NifFile LoadNifFile(string fileName) {
            NifFile nifFile = new NifFile();
            nifFile.Load(Path.Combine(PathConstants.DataDirectory, "meshes", fileName));
            meshList.AddRange(nifFile.Blocks.OfType<BSTriShape>().Select(b => new Mesh(nifFile, b)));
            return nifFile;
        }

        NifFile nifFile_femalebody = LoadNifFile("femalebody_0.nif");
        NifFile nifFile_femalefeet = LoadNifFile("femalefeet_0.nif");
        NifFile nifFile_femalehands = LoadNifFile("femalehands_0.nif");
        NifFile nifFile_femalehead = LoadNifFile("femalehead.nif");

        meshes = meshList.ToArray();
    }

    private Mesh[] meshes;
    public IReadOnlyList<Mesh> Meshes => meshes;
}

public class Mesh {
    public Mesh(NifFile nifFile, BSTriShape bSTriShape) {
        BSTriShape = bSTriShape;
        BSDismemberSkinInstance = (BSDismemberSkinInstance)nifFile.Blocks[BSTriShape.SkinInstanceRef.Index];
        NiSkinData = (NiSkinData)nifFile.Blocks[BSDismemberSkinInstance.Data.Index];
        NiSkinPartition = (NiSkinPartition)nifFile.Blocks[BSDismemberSkinInstance.SkinPartition.Index];
        BSLightingShaderProperty = (BSLightingShaderProperty)nifFile.Blocks[BSTriShape.ShaderPropertyRef.Index];
        BSShaderTextureSet = (BSShaderTextureSet)nifFile.Blocks[BSLightingShaderProperty.TextureSetRef.Index];

        Matrix4x4 GetTransform() {
            var m_s = Matrix4x4.CreateScale(BSTriShape.Scale);
            var r = BSTriShape.Rotation;
            var m_r = new Matrix4x4(
                r.M11, r.M12, r.M13, 0,
                r.M21, r.M22, r.M23, 0,
                r.M31, r.M32, r.M33, 0,
                0, 0, 0, 1);
            var m_t = Matrix4x4.CreateTranslation(BSTriShape.Translation);
            return m_s * m_r * m_t;
        }
        var transform = GetTransform();

        int vertexCount = BSTriShape.VertexCount;
        var vertices = new Vector3[vertexCount];
        var uvs = new HalfTexCoord[vertexCount];
        for (int i = 0; i < vertexCount; i++) {
            BSVertexDataSSE vertexData = BSTriShape.VertexDataSSE[i];
            Vector3 vertex = vertexData.Vertex;
            vertex = Vector4.Transform(vertex, transform).AsVector3();
            vertices[i] = vertex;
            uvs[i] = vertexData.UV;
        }
        ID3D11Device device = DXObjects.D3D11Device;
        VertexBuffer = device.CreateBuffer(vertices, BindFlags.VertexBuffer, ResourceUsage.Immutable);
        UVBuffer = device.CreateBuffer(uvs, BindFlags.VertexBuffer, ResourceUsage.Immutable);

        var weights = new float[vertexCount * 4];
        var boneIndices = new byte[vertexCount * 4];
        foreach (var partition in NiSkinPartition.Partitions) {
            for (int i_vert_part = 0; i_vert_part < partition.NumVertices; i_vert_part++) {
                int i_vert_mesh = partition.VertexMap[i_vert_part];
                int i_vert_mesh_4 = i_vert_mesh * 4;
                int i_vert_part_4 = i_vert_part * 4;
                void SetWeightAndBoneIndex(int i_mesh, int i_part) {
                    weights[i_mesh] = partition.VertexWeights[i_part];
                    boneIndices[i_mesh] = partition.BoneIndices[i_part];
                }
                SetWeightAndBoneIndex(i_vert_mesh_4++, i_vert_part_4++);
                SetWeightAndBoneIndex(i_vert_mesh_4++, i_vert_part_4++);
                SetWeightAndBoneIndex(i_vert_mesh_4++, i_vert_part_4++);
                SetWeightAndBoneIndex(i_vert_mesh_4, i_vert_part_4);
            }
        }
        WeightBuffer = device.CreateBuffer(weights, BindFlags.VertexBuffer, ResourceUsage.Immutable);
        BoneIndexBuffer = device.CreateBuffer(boneIndices, BindFlags.VertexBuffer, ResourceUsage.Immutable);

        partitionMeshes = new PartitionMesh[NiSkinPartition.Partitions.Count];
        partitionMeshes.Populate(i => new(this, i));

        var normals = new Vector3[vertexCount];
        foreach (var partition in NiSkinPartition.Partitions) {
            for (int i_tri = 0; i_tri < partition.NumTriangles; i_tri++) {
                var tri = partition.Triangles[i_tri];
                Vector3 v0 = vertices[tri.V1];
                Vector3 v1 = vertices[tri.V2];
                Vector3 v2 = vertices[tri.V3];
                normals[tri.V1] += Vector3.Cross(v1 - v0, v2 - v0);
                normals[tri.V2] += Vector3.Cross(v2 - v1, v0 - v1);
                normals[tri.V3] += Vector3.Cross(v0 - v2, v1 - v2);
            }
        }
        for (int i = 0; i < vertexCount; i++) {
            normals[i] = Vector3.Normalize(normals[i]);
        }
        NormalBuffer = device.CreateBuffer(normals, BindFlags.VertexBuffer, ResourceUsage.Immutable);

        //Texture = Texture.GetOrCreate(BSShaderTextureSet.Textures[0].Content);

        IEnumerable<string> boneNames = BSDismemberSkinInstance.Bones.GetBlocks(nifFile).Select(b => b.Name.String);
        boneMap = boneNames.Select(name => Skeleton.Instance.Dictionary[name].Index).ToArray();

        boneLocals = BSDismemberSkinInstance.Bones.GetBlocks(nifFile).Select(n => new Transform(n.Translation, n.Rotation, n.Scale)).ToArray();

        for (int i = 0; i < boneLocals.Length; i++) {
            Transform boneLocal = boneLocals[i];
            int boneIndex = boneMap[i];
            Transform boneLocal1 = Skeleton.Instance.Bones[boneIndex].LocalTransform;
            if (boneLocal != boneLocal1) {
                Debug.WriteLine($"Bone local transform mismatch {i}");
                Debug.WriteLine(boneLocal);
                Debug.WriteLine(boneLocal1);
                //Skeleton.Instance.Bones[boneIndex].LocalTransform *= boneLocal;
            }
        }
    }
    public BSTriShape BSTriShape { get; }
    public BSDismemberSkinInstance BSDismemberSkinInstance { get; }
    public NiSkinData NiSkinData { get; }
    public NiSkinPartition NiSkinPartition { get; }
    public BSLightingShaderProperty BSLightingShaderProperty { get; }
    public BSShaderTextureSet BSShaderTextureSet { get; }

    public ID3D11Buffer VertexBuffer { get; }
    public ID3D11Buffer UVBuffer { get; }
    public ID3D11Buffer NormalBuffer { get; }
    public ID3D11Buffer WeightBuffer { get; }
    public ID3D11Buffer BoneIndexBuffer { get; }

    private PartitionMesh[] partitionMeshes;
    public IReadOnlyList<PartitionMesh> PartitionMeshes => partitionMeshes;

    public Texture? Texture { get; }

    private int[] boneMap;
    public IReadOnlyList<int> BoneMap => boneMap;

    private Transform[] boneLocals;
    public IReadOnlyList<Transform> BoneLocals => boneLocals;

    public BSShaderFlags SLSF1 => BSLightingShaderProperty.ShaderFlags;
    public BSShaderFlags2 SLSF2 => BSLightingShaderProperty.ShaderFlags2;

}

public class PartitionMesh {
    public PartitionMesh(Mesh mesh, int partitionIndex) {

        SkinPartition skinPartition = mesh.NiSkinPartition.Partitions[partitionIndex];

        boneMap = skinPartition.Bones.ToArray();

        ID3D11Device device = DXObjects.D3D11Device;
        TriangleIndexBuffer = device.CreateBuffer(skinPartition.Triangles.ToArray(), BindFlags.IndexBuffer, ResourceUsage.Immutable, structureByteStride: sizeof(short));
    }

    private ushort[] boneMap;
    public IReadOnlyList<ushort> BoneMap => boneMap;

    public ID3D11Buffer TriangleIndexBuffer { get; }
}

