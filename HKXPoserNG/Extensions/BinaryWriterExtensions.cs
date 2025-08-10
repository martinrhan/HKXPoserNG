using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;

namespace HKXPoserNG.Extensions;

public static class BinaryWriterExtensions {
    public static void Write(this BinaryWriter writer, ref Vector4 v) {
        writer.Write(v.X);
        writer.Write(v.Y);
        writer.Write(v.Z);
        writer.Write(v.W);
    }

    public static void Write(this BinaryWriter writer, ref Quaternion q) {
        writer.Write(q.X);
        writer.Write(q.Y);
        writer.Write(q.Z);
        writer.Write(q.W);
    }

    public static void WriteHeaderString(this BinaryWriter writer, string s) {
        foreach (byte i in Encoding.Default.GetBytes(s))
            writer.Write(i);

        writer.Write((byte)10);
    }

    public static void WriteCString(this BinaryWriter writer, string s) {
        foreach (byte i in Encoding.Default.GetBytes(s))
            writer.Write(i);

        writer.Write((byte)0);
    }
}
