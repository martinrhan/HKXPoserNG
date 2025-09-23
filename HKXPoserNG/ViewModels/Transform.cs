using HKXPoserNG.Extensions;
using NiflySharp.Structs;
using System;
using System.IO;
using System.Numerics;

public struct Transform {
    public static Transform Identity { get; } = new Transform(Vector3.Zero, Quaternion.Identity, 1);

    public float Scale { get; }
    public Quaternion Rotation { get; }
    public Vector3 Translation { get; }

    public Matrix4x4 Matrix { get; private set; }

    public Transform() : this(Vector3.Zero, Quaternion.Identity, 1) { }

    public Transform(Vector3 translation, Quaternion rotation, float scale) {
        this.Translation = translation;
        this.Rotation = rotation;
        this.Scale = scale;
        Matrix =
            Matrix4x4.CreateTranslation(Translation) *
            Matrix4x4.CreateFromQuaternion(Rotation) *
            Matrix4x4.CreateScale(Scale)
            ;
    }

    public Transform(Vector3 translation, Matrix33 rotation, float scale) :
        this(translation, Quaternion.CreateFromRotationMatrix(rotation.ToMatrix4x4()), scale) {
        Quaternion q = Quaternion.CreateFromRotationMatrix(rotation.ToMatrix4x4());
        Matrix4x4 m = Matrix4x4.CreateFromQuaternion(Rotation);
    }
     
    public Transform(BinaryReader reader) {
        Vector4 t;
        Quaternion rotation;
        Vector4 scale;

        reader.ReadVector4(out t);
        reader.ReadQuaternion(out rotation);
        reader.ReadVector4(out scale);

        this.Translation = new Vector3(t.X, t.Y, t.Z);
        this.Rotation = rotation;
        this.Scale = scale.Z;
        Matrix =
            Matrix4x4.CreateScale(Scale) *
            Matrix4x4.CreateFromQuaternion(Rotation) *
            Matrix4x4.CreateTranslation(Translation);
    }

    public void Dump() {
        Console.WriteLine("Translation: {0:F6} {1:F6} {2:F6}", Translation.X, Translation.Y, Translation.Z);
        Console.WriteLine("Rotation: {0:F6} {1:F6} {2:F6} {3:F6}", Rotation.W, Rotation.X, Rotation.Y, Rotation.Z);
        Console.WriteLine("Scale: {0:F6}", Scale);
    }

    public void Write(BinaryWriter writer) {
        Vector4 t = new Vector4(this.Translation, 0);
        Vector4 scale = new Vector4(this.Scale, this.Scale, this.Scale, 0);
        writer.Write(t);
        writer.Write(Rotation);
        writer.Write(scale);
    }

}