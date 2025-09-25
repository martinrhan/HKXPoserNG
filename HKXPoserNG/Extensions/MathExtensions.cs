using NiflySharp.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Vortice.Mathematics;

namespace HKXPoserNG.Extensions;

public static class MathExtensions {

    public static float Square(this float x) => x * x;
}

public static class NumericsExtensions {
    public static Matrix4x4 ToMatrix4x4(this in Matrix33 mat) {
        return new Matrix4x4(
            mat.M11, mat.M12, mat.M13, 0,
            mat.M21, mat.M22, mat.M23, 0,
            mat.M31, mat.M32, mat.M33, 0,
            0, 0, 0, 1);
    }

    public static Matrix33 Transpose(this in Matrix33 mat) {
        return new Matrix33 {
            M11 = mat.M11,
            M12 = mat.M21,
            M13 = mat.M31,
            M21 = mat.M12,
            M22 = mat.M22,
            M23 = mat.M32,
            M31 = mat.M13,
            M32 = mat.M23,
            M33 = mat.M33
        };
    }

    public static bool AreApproximatelyEqual(in Matrix4x4 mat1, in Matrix4x4 mat2, float tolerance = 0.0001f) {
        return
            MathF.Abs(mat1.M11 - mat2.M11) < tolerance &&
            MathF.Abs(mat1.M12 - mat2.M12) < tolerance &&
            MathF.Abs(mat1.M13 - mat2.M13) < tolerance &&
            MathF.Abs(mat1.M14 - mat2.M14) < tolerance &&
            MathF.Abs(mat1.M21 - mat2.M21) < tolerance &&
            MathF.Abs(mat1.M22 - mat2.M22) < tolerance &&
            MathF.Abs(mat1.M23 - mat2.M23) < tolerance &&
            MathF.Abs(mat1.M24 - mat2.M24) < tolerance &&
            MathF.Abs(mat1.M31 - mat2.M31) < tolerance &&
            MathF.Abs(mat1.M32 - mat2.M32) < tolerance &&
            MathF.Abs(mat1.M33 - mat2.M33) < tolerance &&
            MathF.Abs(mat1.M34 - mat2.M34) < tolerance &&
            MathF.Abs(mat1.M41 - mat2.M41) < tolerance &&
            MathF.Abs(mat1.M42 - mat2.M42) < tolerance &&
            MathF.Abs(mat1.M43 - mat2.M43) < tolerance &&
            MathF.Abs(mat1.M44 - mat2.M44) < tolerance;
    }
}