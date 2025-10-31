using System.Numerics;
using Vortice.Mathematics;
using HKXPoserNG.Extensions;

namespace HKXPoserNG.Test;

[TestClass]
public sealed class QuaternionTests {
    [TestMethod]
    public void TestToEuler() {
        Quaternion q = Quaternion.CreateFromYawPitchRoll(.2f, .1f, .3f);
        Vector3 euler = q.ToEuler();
        Assert.IsTrue(MathFExtensions.AreApproximatelyEqual(euler.X, .1f));
        Assert.IsTrue(MathFExtensions.AreApproximatelyEqual(euler.Y, .2f));
        Assert.IsTrue(MathFExtensions.AreApproximatelyEqual(euler.Z, .3f));
        
    }
}