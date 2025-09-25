using HKXPoserNG.Extensions;
using System.Numerics;

namespace HKXPoserNG.Test;

[TestClass]
public sealed class TransformTest {

    [TestMethod]
    public void TestMultiply() {
        Vector3 tl0 = new Vector3(1, 2, 3);
        Matrix4x4 r0 = Matrix4x4.CreateFromYawPitchRoll(0.1f, 0.2f, 0.3f);
        float s0 = 1.5f;
        Vector3 tl1 = new Vector3(4, 5, 6);
        Matrix4x4 r1 = Matrix4x4.CreateFromYawPitchRoll(0.4f, 0.5f, 0.6f);
        float s1 = 2.5f;

        Transform tf0 = new Transform(tl0, Quaternion.CreateFromRotationMatrix(r0), s0);
        Transform tf1 = new Transform(tl1, Quaternion.CreateFromRotationMatrix(r1), s1);
        Transform tf_product = tf0 * tf1;

        Matrix4x4 m0 =
            Matrix4x4.CreateScale(s0) *
            r0 *
            Matrix4x4.CreateTranslation(tl0);

        Matrix4x4 m1 =
            Matrix4x4.CreateScale(s1) *
            r1 *
            Matrix4x4.CreateTranslation(tl1);
        Matrix4x4 m_product = m0 * m1;

        Assert.IsTrue(NumericsExtensions.AreApproximatelyEqual(m_product, tf_product.Matrix));
    }
}
