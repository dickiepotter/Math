namespace RP.Math.Tests.Core
{
    using System;
    using FluentAssertions;
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

            (a + b).Should().Be(new Vector3(5, 7, 9));
            (b - a).Should().Be(new Vector3(3, 3, 3));
            (a * 2f).Should().Be(new Vector3(2, 4, 6));
            (2f * a).Should().Be(new Vector3(2, 4, 6));
            (-a).Should().Be(new Vector3(-1, -2, -3));
        }

        [TestMethod]
        public void Vector3_Cross_IsRightHanded()
        {
            Vector3.Cross(Vector3.UnitX, Vector3.UnitY).Should().Be(Vector3.UnitZ);
            Vector3.Cross(Vector3.UnitY, Vector3.UnitZ).Should().Be(Vector3.UnitX);
            // Anti-commutative.
            Vector3.Cross(Vector3.UnitY, Vector3.UnitX).Should().Be(-Vector3.UnitZ);
        }

        [TestMethod]
        public void Vector3_DotAndLength()
        {
            Vector3.Dot(new Vector3(1, 0, 0), new Vector3(0, 1, 0)).Should().Be(0f);
            new Vector3(3, 4, 0).Length.Should().BeApproximately(5f, Tol);
            new Vector3(3, 4, 0).LengthSquared.Should().Be(25f);
        }

        [TestMethod]
        public void Vector3_Normalize_ProducesUnitLength()
        {
            var n = new Vector3(0, 3, 4).Normalize();
            n.Length.Should().BeApproximately(1f, Tol);
            n.IsUnit().Should().BeTrue();
        }

        [TestMethod]
        public void Vector3_Normalize_ZeroThrows_OrDefaultReturnsZero()
        {
            Action act = () => Vector3.Zero.Normalize();
            act.Should().Throw<DivideByZeroException>();

            Vector3.Zero.NormalizeOrDefault().Should().Be(Vector3.Zero);
        }

        [TestMethod]
        public void Vector3_Angle_IsStableAtTheParallelPoles()
        {
            var x = Vector3.UnitX;
            Vector3.Angle(x, x).Should().BeApproximately(0f, 1e-4f);          // parallel
            Vector3.Angle(x, -x).Should().BeApproximately((float)Math.PI, 1e-4f); // anti-parallel, no NaN
            Vector3.Angle(x, Vector3.UnitY).Should().BeApproximately((float)(Math.PI / 2), 1e-4f);
            Vector3.Angle(x, -x).Should().NotBe(float.NaN);
        }

        [TestMethod]
        public void Vector3_ProjectPlusReject_ReconstructsTheVector()
        {
            var v = new Vector3(2, 3, 4);
            var dir = new Vector3(0, 1, 0);
            (v.Project(dir) + v.Reject(dir)).ApproximatelyEquals(v).Should().BeTrue();
            v.Project(dir).Should().Be(new Vector3(0, 3, 0));
        }

        [TestMethod]
        public void Vector3_Reflect_BouncesOffASurface()
        {
            var incoming = new Vector3(1, -1, 0);
            var bounced = incoming.Reflect(new Vector3(0, 1, 0));
            bounced.ApproximatelyEquals(new Vector3(1, 1, 0)).Should().BeTrue();
        }

        [TestMethod]
        public void Vector3_MinMaxClamp_AreComponentWise()
        {
            var a = new Vector3(1, 5, 3);
            var b = new Vector3(4, 2, 6);
            Vector3.Min(a, b).Should().Be(new Vector3(1, 2, 3));
            Vector3.Max(a, b).Should().Be(new Vector3(4, 5, 6));
            Vector3.Clamp(new Vector3(5, -5, 2), Vector3.Zero, new Vector3(3, 3, 3))
                .Should().Be(new Vector3(3, 0, 2));
        }

        [TestMethod]
        public void Vector3_ClampMagnitude_CapsLength()
        {
            var limited = new Vector3(3, 4, 0).ClampMagnitude(2.5f);
            limited.Length.Should().BeApproximately(2.5f, Tol);
        }

        [TestMethod]
        public void Vector3_MoveTowards_NeverOvershoots()
        {
            Vector3.MoveTowards(Vector3.Zero, new Vector3(10, 0, 0), 3f).Should().Be(new Vector3(3, 0, 0));
            Vector3.MoveTowards(Vector3.Zero, new Vector3(2, 0, 0), 5f).Should().Be(new Vector3(2, 0, 0));
        }

        [TestMethod]
        public void Vector3_Conversions_WidenImplicitlyNarrowExplicitly()
        {
            Vector3d wide = new Vector3(1.5f, 2.5f, 3.5f); // implicit widening
            wide.X.Should().Be(1.5);

            var narrow = (Vector3)new Vector3d(1.5, 2.5, 3.5); // explicit narrowing
            narrow.Should().Be(new Vector3(1.5f, 2.5f, 3.5f));

            System.Numerics.Vector3 sys = new Vector3(1, 2, 3); // lossless interop
            sys.X.Should().Be(1f);
            ((Vector3)sys).Should().Be(new Vector3(1, 2, 3));
        }

        [TestMethod]
        public void Vector3_IsNaN_DetectsBadComponents()
        {
            new Vector3(float.NaN, 0, 0).IsNaN().Should().BeTrue();
            new Vector3(1, 2, 3).IsNaN().Should().BeFalse();
        }

        // ---- Vector2 ----

        [TestMethod]
        public void Vector2_Cross_IsSignedPerpDot()
        {
            Vector2.Cross(Vector2.UnitX, Vector2.UnitY).Should().Be(1f);  // left turn
            Vector2.Cross(Vector2.UnitY, Vector2.UnitX).Should().Be(-1f); // right turn
            Vector2.Cross(Vector2.UnitX, Vector2.UnitX).Should().Be(0f);  // parallel
        }

        [TestMethod]
        public void Vector2_Perpendicular_RotatesNinetyDegrees()
        {
            new Vector2(1, 0).Perpendicular().Should().Be(new Vector2(0, 1));
        }

        [TestMethod]
        public void Vector2_NormalizeAndAngle()
        {
            new Vector2(3, 4).Normalize().Length.Should().BeApproximately(1f, Tol);
            Vector2.Angle(Vector2.UnitX, Vector2.UnitY).Should().BeApproximately((float)(Math.PI / 2), 1e-4f);
        }

        // ---- Vector4 ----

        [TestMethod]
        public void Vector4_DotLengthAndXYZ()
        {
            Vector4.Dot(new Vector4(1, 2, 3, 4), new Vector4(1, 0, 0, 0)).Should().Be(1f);
            new Vector4(0, 0, 0, 0).IsZero().Should().BeTrue();
            new Vector4(new Vector3(1, 2, 3), 1f).XYZ.Should().Be(new Vector3(1, 2, 3));
        }

        [TestMethod]
        public void Vector4_Lerp()
        {
            Vector4.Lerp(Vector4.Zero, Vector4.One, 0.5f).Should().Be(new Vector4(0.5f, 0.5f, 0.5f, 0.5f));
        }
    }
}
