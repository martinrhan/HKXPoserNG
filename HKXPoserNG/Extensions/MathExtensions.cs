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
}