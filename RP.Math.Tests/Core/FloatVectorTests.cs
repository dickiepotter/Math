namespace RP.Math.Tests.Core
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using RP.Math;

    /// <summary>
    /// Tests for the float vector family (<see cref="Vector2"/>/<see cref="Vector3"/>/<see cref="Vector4"/>),
    /// including the degenerate cases the completionist principle requires.
    /// </summary>
    [TestClass]
    public sealed class FloatVectorTests
    {
        private const float Tol = 1e-5f;

        // ---- Vector3 ----

        [TestMethod]
        public void Vector3_Arithmetic()
        {
            var a = new Vector3(1, 2, 3);
            var b = new Vector3(4, 5, 6);

            Assert.AreEqual(new Vector3(5, 7, 9), (a + b));
            Assert.AreEqual(new Vector3(3, 3, 3), (b - a));
            Assert.AreEqual(new Vector3(2, 4, 6), (a * 2f));
            Assert.AreEqual(new Vector3(2, 4, 6), (2f * a));
            Assert.AreEqual(new Vector3(-1, -2, -3), (-a));
        }

        [TestMethod]
        public void Vector3_Cross_IsRightHanded()
        {
            Assert.AreEqual(Vector3.UnitZ, Vector3.Cross(Vector3.UnitX, Vector3.UnitY));
            Assert.AreEqual(Vector3.UnitX, Vector3.Cross(Vector3.UnitY, Vector3.UnitZ));
            // Anti-commutative.
            Assert.AreEqual(-Vector3.UnitZ, Vector3.Cross(Vector3.UnitY, Vector3.UnitX));
        }

        [TestMethod]
        public void Vector3_DotAndLength()
        {
            Assert.AreEqual(0f, Vector3.Dot(new Vector3(1, 0, 0), new Vector3(0, 1, 0)));
            Assert.AreEqual(5f, new Vector3(3, 4, 0).Length, Tol);
            Assert.AreEqual(25f, new Vector3(3, 4, 0).LengthSquared);
        }

        [TestMethod]
        public void Vector3_Normalize_ProducesUnitLength()
        {
            var n = new Vector3(0, 3, 4).Normalize();
            Assert.AreEqual(1f, n.Length, Tol);
            Assert.IsTrue(n.IsUnit());
        }

        [TestMethod]
        public void Vector3_Normalize_ZeroThrows_OrDefaultReturnsZero()
        {
            Action act = () => Vector3.Zero.Normalize();
            Assert.ThrowsException<DivideByZeroException>(act);

            Assert.AreEqual(Vector3.Zero, Vector3.Zero.NormalizeOrDefault());
        }

        [TestMethod]
        public void Vector3_Angle_IsStableAtTheParallelPoles()
        {
            var x = Vector3.UnitX;
            Assert.AreEqual(0f, Vector3.Angle(x, x), 1e-4f);          // parallel
            Assert.AreEqual((float)Math.PI, Vector3.Angle(x, -x), 1e-4f); // anti-parallel, no NaN
            Assert.AreEqual((float)(Math.PI / 2), Vector3.Angle(x, Vector3.UnitY), 1e-4f);
            Assert.AreNotEqual(float.NaN, Vector3.Angle(x, -x));
        }

        [TestMethod]
        public void Vector3_ProjectPlusReject_ReconstructsTheVector()
        {
            var v = new Vector3(2, 3, 4);
            var dir = new Vector3(0, 1, 0);
            Assert.IsTrue((v.Project(dir) + v.Reject(dir)).ApproximatelyEquals(v));
            Assert.AreEqual(new Vector3(0, 3, 0), v.Project(dir));
        }

        [TestMethod]
        public void Vector3_Reflect_BouncesOffASurface()
        {
            var incoming = new Vector3(1, -1, 0);
            var bounced = incoming.Reflect(new Vector3(0, 1, 0));
            Assert.IsTrue(bounced.ApproximatelyEquals(new Vector3(1, 1, 0)));
        }

        [TestMethod]
        public void Vector3_MinMaxClamp_AreComponentWise()
        {
            var a = new Vector3(1, 5, 3);
            var b = new Vector3(4, 2, 6);
            Assert.AreEqual(new Vector3(1, 2, 3), Vector3.Min(a, b));
            Assert.AreEqual(new Vector3(4, 5, 6), Vector3.Max(a, b));
            Assert.AreEqual(new Vector3(3, 0, 2), Vector3.Clamp(new Vector3(5, -5, 2), Vector3.Zero, new Vector3(3, 3, 3)));
        }

        [TestMethod]
        public void Vector3_ClampMagnitude_CapsLength()
        {
            var limited = new Vector3(3, 4, 0).ClampMagnitude(2.5f);
            Assert.AreEqual(2.5f, limited.Length, Tol);
        }

        [TestMethod]
        public void Vector3_MoveTowards_NeverOvershoots()
        {
            Assert.AreEqual(new Vector3(3, 0, 0), Vector3.MoveTowards(Vector3.Zero, new Vector3(10, 0, 0), 3f));
            Assert.AreEqual(new Vector3(2, 0, 0), Vector3.MoveTowards(Vector3.Zero, new Vector3(2, 0, 0), 5f));
        }

        [TestMethod]
        public void Vector3_Conversions_WidenImplicitlyNarrowExplicitly()
        {
            Vector3d wide = new Vector3(1.5f, 2.5f, 3.5f); // implicit widening
            Assert.AreEqual(1.5, wide.X);

            var narrow = (Vector3)new Vector3d(1.5, 2.5, 3.5); // explicit narrowing
            Assert.AreEqual(new Vector3(1.5f, 2.5f, 3.5f), narrow);

            System.Numerics.Vector3 sys = new Vector3(1, 2, 3); // lossless interop
            Assert.AreEqual(1f, sys.X);
            Assert.AreEqual(new Vector3(1, 2, 3), ((Vector3)sys));
        }

        [TestMethod]
        public void Vector3_IsNaN_DetectsBadComponents()
        {
            Assert.IsTrue(new Vector3(float.NaN, 0, 0).IsNaN());
            Assert.IsFalse(new Vector3(1, 2, 3).IsNaN());
        }

        // ---- Vector2 ----

        [TestMethod]
        public void Vector2_Cross_IsSignedPerpDot()
        {
            Assert.AreEqual(1f, Vector2.Cross(Vector2.UnitX, Vector2.UnitY));  // left turn
            Assert.AreEqual(-1f, Vector2.Cross(Vector2.UnitY, Vector2.UnitX)); // right turn
            Assert.AreEqual(0f, Vector2.Cross(Vector2.UnitX, Vector2.UnitX));  // parallel
        }

        [TestMethod]
        public void Vector2_Perpendicular_RotatesNinetyDegrees()
        {
            Assert.AreEqual(new Vector2(0, 1), new Vector2(1, 0).Perpendicular());
        }

        [TestMethod]
        public void Vector2_NormalizeAndAngle()
        {
            Assert.AreEqual(1f, new Vector2(3, 4).Normalize().Length, Tol);
            Assert.AreEqual((float)(Math.PI / 2), Vector2.Angle(Vector2.UnitX, Vector2.UnitY), 1e-4f);
        }

        // ---- Vector4 ----

        [TestMethod]
        public void Vector4_DotLengthAndXYZ()
        {
            Assert.AreEqual(1f, Vector4.Dot(new Vector4(1, 2, 3, 4), new Vector4(1, 0, 0, 0)));
            Assert.IsTrue(new Vector4(0, 0, 0, 0).IsZero());
            Assert.AreEqual(new Vector3(1, 2, 3), new Vector4(new Vector3(1, 2, 3), 1f).XYZ);
        }

        [TestMethod]
        public void Vector4_Lerp()
        {
            Assert.AreEqual(new Vector4(0.5f, 0.5f, 0.5f, 0.5f), Vector4.Lerp(Vector4.Zero, Vector4.One, 0.5f));
        }
    }
}
