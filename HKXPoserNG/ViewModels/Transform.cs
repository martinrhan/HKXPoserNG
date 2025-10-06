using HKXPoserNG.Extensions;
using NiflySharp.Structs;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Numerics;

public readonly struct Transform : IEquatable<Transform> {
    public static Transform Identity { get; } = new Transform(Vector3.Zero, Quaternion.Identity, 1);

    public float Scale { get; }
    public Quaternion Rotation { get; }
    public Vector3 Translation { get; }

    public Matrix4x4 Matrix =>
        Matrix4x4.CreateScale(Scale) * Matrix4x4.CreateFromQuaternion(Rotation) * Matrix4x4.CreateTranslation(Translation);


    public Transform() : this(Vector3.Zero, Quaternion.Identity, 1) { }

    public Transform(Vector3 translation, Quaternion rotation, float scale) {
        this.Translation = translation;
        this.Rotation = rotation;
        this.Scale = scale;
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

    public bool Equals(Transform other) {
        return this.Translation == other.Translation &&
               this.Rotation == other.Rotation &&
               this.Scale == other.Scale;
    }

    public override bool Equals([NotNullWhen(true)] object? obj) {
        return obj is Transform other && Equals(other);
    }

    public override int GetHashCode() {
        return HashCode.Combine(Translation, Rotation, Scale);
    }

    public override string ToString() {
        return $"T:{Translation} R:{Rotation} S:{Scale}";
    }

    public static bool operator ==(Transform left, Transform right) => left.Equals(right);
    public static bool operator !=(Transform left, Transform right) => !left.Equals(right);

    public static Transform operator *(Transform a, Transform b) {
        float scale = b.Scale * a.Scale;
        Quaternion rotation = Quaternion.Normalize(b.Rotation * a.Rotation);
        Vector3 translation = Vector3.Transform(a.Translation * b.Scale, b.Rotation) + b.Translation;
        return new Transform(translation, rotation, scale);
    }

    public Transform Inverse() {
        float invScale = 1.0f / Scale;
        Quaternion invRotation = Quaternion.Conjugate(Rotation);
        Vector3 invTranslation = Vector3.Transform(-Translation * invScale, invRotation);
        return new Transform(invTranslation, invRotation, invScale);
    }
}