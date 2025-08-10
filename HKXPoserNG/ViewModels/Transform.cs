using HKXPoserNG.Extensions;
using System;
using System.IO;
using System.Numerics;

public class Transform
{
    public Vector3 translation;
    public Quaternion rotation;
    public float scale;

    public Transform()
    {
        this.translation = Vector3.Zero;
        this.rotation = Quaternion.Identity;
        this.scale = 1.0f;
    }

    public Transform(Vector3 translation, Quaternion rotation, float scale)
    {
        this.translation = translation;
        this.rotation = rotation;
        this.scale = scale;
    }

    public static Transform operator *(Transform t1, float amount)
    {
        return new Transform(
            t1.translation * amount,
            Quaternion.Slerp(Quaternion.Identity, t1.rotation, amount),
            (float)Math.Pow(t1.scale, amount));
    }

    public static Transform operator *(Transform t1, Transform t2)
    {
        return new Transform(
            t1.translation + Vector3.Transform(t2.translation, t1.rotation) * t1.scale,
            t1.rotation * t2.rotation,
            t1.scale * t2.scale);
    }

    public void Dump()
    {
        Console.WriteLine("Translation: {0:F6} {1:F6} {2:F6}", translation.X, translation.Y, translation.Z);
        Console.WriteLine("Rotation: {0:F6} {1:F6} {2:F6} {3:F6}", rotation.W, rotation.X, rotation.Y, rotation.Z);
        Console.WriteLine("Scale: {0:F6}", scale);
    }

    public void Read(BinaryReader reader)
    {
        Vector4 t;
        Vector4 scale;

        reader.ReadVector4(out t);
        reader.ReadQuaternion(out this.rotation);
        reader.ReadVector4(out scale);

        this.translation = new Vector3(t.X, t.Y, t.Z);
        this.scale = scale.Z;
    }

    public void Write(BinaryWriter writer)
    {
        Vector4 t = new Vector4(this.translation, 0);
        Vector4 scale = new Vector4(this.scale, this.scale, this.scale, 0);

        writer.Write(ref t);
        writer.Write(ref rotation);
        writer.Write(ref scale);
    }
}
