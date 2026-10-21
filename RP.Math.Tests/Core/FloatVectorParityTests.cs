namespace RP.Math.Tests.Core
{
    using System;
    using System.Globalization;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using RP.Math;

    /// <summary>
    /// Tests for the Vector2d-name parity surface and added capabilities on the float vector family
    /// (<see cref="Vector2"/>/<see cref="Vector3"/>/<see cref="Vector4"/>): alias equivalence, the new
    /// projection/rejection/reflection/slerp/interpolate maths, and the edge cases that try to break them.
    /// </summary>
    [TestClass]
    public sealed class FloatVectorParityTests
    {
        private const float Tol = 1e-5f;

        // ---------------------------------------------------------------- Alias equivalence

        [TestMethod]
        public void Vector2_Aliases_MatchTerseForms()
        {
            var a = new Vector2(3, 4);
            var b = new Vector2(-2, 5);

            Assert.AreEqual(Vector2.Dot(a, b), Vector2.DotProduct(a, b));
            Assert.AreEqual(a.Dot(b), a.DotProduct(b));
            Assert.AreEqual(Vector2.Cross(a, b), Vector2.CrossProduct(a, b));
            Assert.AreEqual(Vector2.Cross(a, b), a.CrossProduct(b));
            Assert.AreEqual(a.Length, a.Magnitude);
            Assert.AreEqual(a.LengthSquared, a.MagnitudeSquared);
            Assert.AreEqual(Vector2.Min(a, b), Vector2.ComponentMin(a, b));
            Assert.AreEqual(Vector2.Max(a, b), Vector2.ComponentMax(a, b));
            Assert.AreEqual(a.Abs(), a.AbsComponents());
            Assert.AreEqual(a.IsUnit(), a.IsUnitVector());
            Assert.AreEqual(Vector2.Distance(a, b), a.Distance(b));
            Assert.AreEqual(Vector2.DistanceSquared(a, b), a.DistanceSquared(b));
            Assert.IsTrue(Vector2.UnitX.Equals(Vector2.UnitX, Tol));
        }

        [TestMethod]
        public void Vector3_Aliases_MatchTerseForms()
        {
            var a = new Vector3(3, 4, 5);
            var b = new Vector3(-2, 5, 1);

            Assert.AreEqual(Vector3.Dot(a, b), Vector3.DotProduct(a, b));
            Assert.AreEqual(a.Dot(b), a.DotProduct(b));
            Assert.AreEqual(Vector3.Cross(a, b), Vector3.CrossProduct(a, b));
            Assert.AreEqual(a.Cross(b), a.CrossProduct(b));
            Assert.AreEqual(a.Length, a.Magnitude);
            Assert.AreEqual(a.LengthSquared, a.MagnitudeSquared);
            Assert.AreEqual(Vector3.Min(a, b), Vector3.ComponentMin(a, b));
            Assert.AreEqual(Vector3.Max(a, b), Vector3.ComponentMax(a, b));
            Assert.AreEqual(a.Project(b), a.Projection(b));
            Assert.AreEqual(a.Reject(b), a.Rejection(b));
            Assert.AreEqual(a.Abs(), a.AbsComponents());
            Assert.AreEqual(a.IsUnit(), a.IsUnitVector());
            Assert.AreEqual(Vector3.Distance(a, b), a.Distance(b));
        }

        [TestMethod]
        public void Vector4_Aliases_MatchTerseForms()
        {
            var a = new Vector4(3, 4, 5, 6);
            var b = new Vector4(-2, 5, 1, 0);

            Assert.AreEqual(Vector4.Dot(a, b), Vector4.DotProduct(a, b));
            Assert.AreEqual(a.Dot(b), a.DotProduct(b));
            Assert.AreEqual(a.Length, a.Magnitude);
            Assert.AreEqual(a.LengthSquared, a.MagnitudeSquared);
            Assert.AreEqual(Vector4.Min(a, b), Vector4.ComponentMin(a, b));
            Assert.AreEqual(Vector4.Max(a, b), Vector4.ComponentMax(a, b));
            Assert.AreEqual(a.Abs(), a.AbsComponents());
            Assert.AreEqual(Vector4.Distance(a, b), a.Distance(b));
        }

        // ---------------------------------------------------------------- Projection / rejection

        [TestMethod]
        public void Projection_Plus_Rejection_Reconstructs_And_RejectionIsPerpendicular()
        {
            var v2 = new Vector2(3, 4);
            var d2 = new Vector2(1, 0);
            Assert.IsTrue((v2.Projection(d2) + v2.Rejection(d2)).ApproximatelyEquals(v2, Tol));
            Assert.AreEqual(0f, Vector2.Dot(v2.Rejection(d2), d2), Tol);

            var v3 = new Vector3(2, 3, 4);
            var d3 = new Vector3(0, 1, 0);
            Assert.IsTrue((v3.Projection(d3) + v3.Rejection(d3)).ApproximatelyEquals(v3, Tol));
            Assert.AreEqual(0f, Vector3.Dot(v3.Rejection(d3), d3), Tol);

            var v4 = new Vector4(2, 3, 4, 5);
            var d4 = new Vector4(0, 0, 1, 0);
            Assert.IsTrue((v4.Projection(d4) + v4.Rejection(d4)).ApproximatelyEquals(v4, Tol));
            Assert.AreEqual(0f, Vector4.Dot(v4.Rejection(d4), d4), Tol);
        }

        [TestMethod]
        public void Projection_ZeroDirection_ReturnsZero()
        {
            Assert.AreEqual(Vector2.Zero, new Vector2(3, 4).Projection(Vector2.Zero));
            Assert.AreEqual(Vector4.Zero, new Vector4(3, 4, 5, 6).Projection(Vector4.Zero));
        }

        // ---------------------------------------------------------------- Reflection / reflect

        [TestMethod]
        public void Reflect_AboutNormal_FlipsTheNormalComponent()
        {
            // Incoming velocity hitting a wall whose normal is +Y: Y flips, X/Z keep.
            Assert.AreEqual(new Vector2(1, 1), new Vector2(1, -1).Reflect(Vector2.UnitY));
            Assert.AreEqual(new Vector3(1, 1, 2), new Vector3(1, -1, 2).Reflect(Vector3.UnitY));
            Assert.AreEqual(new Vector4(1, 1, 2, 3), new Vector4(1, -1, 2, 3).Reflect(Vector4.UnitY));
        }

        [TestMethod]
        public void Reflection_AboutLine_PreservesMagnitude()
        {
            var v2 = new Vector2(3, 4);
            Assert.AreEqual(v2.Length, v2.Reflection(new Vector2(1, 2)).Length, Tol);

            var v3 = new Vector3(3, 4, 5);
            Assert.AreEqual(v3.Length, v3.Reflection(new Vector3(1, 2, 3)).Length, Tol);

            var v4 = new Vector4(3, 4, 5, 6);
            Assert.AreEqual(v4.Length, v4.Reflection(new Vector4(1, 2, 3, 4)).Length, Tol);
        }

        [TestMethod]
        public void Reflection_AcrossOwnDirection_ReturnsSelf()
        {
            var v = new Vector2(2, 5);
            Assert.IsTrue(v.Reflection(v).ApproximatelyEquals(v, Tol));
        }

        // ---------------------------------------------------------------- Slerp

        [TestMethod]
        public void Slerp_Midpoint_IsUnitLength_At45Degrees()
        {
            var mid = Vector3.Slerp(Vector3.UnitX, Vector3.UnitY, 0.5f);
            Assert.AreEqual(1f, mid.Length, Tol);
            // 45° between UnitX and the midpoint.
            Assert.AreEqual((float)(Math.PI / 4), Vector3.Angle(Vector3.UnitX, mid), 1e-4f);

            var mid2 = Vector2.Slerp(Vector2.UnitX, Vector2.UnitY, 0.5f);
            Assert.AreEqual(1f, mid2.Length, Tol);
        }

        [TestMethod]
        public void Slerp_Antiparallel_DoesNotProduceNaN()
        {
            var r = Vector3.Slerp(Vector3.UnitX, -Vector3.UnitX, 0.5f);
            Assert.IsFalse(r.IsNaN());

            var r2 = Vector2.Slerp(Vector2.UnitX, -Vector2.UnitX, 0.5f);
            Assert.IsFalse(r2.IsNaN());

            var r4 = Vector4.Slerp(Vector4.UnitX, -Vector4.UnitX, 0.5f);
            Assert.IsFalse(r4.IsNaN());
        }

        [TestMethod]
        public void Slerp_Endpoints_AreExact()
        {
            Assert.IsTrue(Vector3.Slerp(Vector3.UnitX, Vector3.UnitY, 0f).ApproximatelyEquals(Vector3.UnitX, Tol));
            Assert.IsTrue(Vector3.Slerp(Vector3.UnitX, Vector3.UnitY, 1f).ApproximatelyEquals(Vector3.UnitY, Tol));
        }

        // ---------------------------------------------------------------- Interpolate vs Lerp

        [TestMethod]
        public void Interpolate_OutOfRange_Throws_ButLerpAndExtrapolationDoNot()
        {
            var a = Vector3.Zero;
            var b = Vector3.One;

            Action over = () => Vector3.Interpolate(a, b, 1.5f);
            Action under = () => Vector3.Interpolate(a, b, -0.5f);
            Assert.ThrowsException<ArgumentOutOfRangeException>(over);
            Assert.ThrowsException<ArgumentOutOfRangeException>(under);

            // Lerp extrapolates without complaint.
            Assert.AreEqual(new Vector3(1.5f, 1.5f, 1.5f), Vector3.Lerp(a, b, 1.5f));
            // Explicit allowExtrapolation does too.
            Assert.AreEqual(new Vector3(1.5f, 1.5f, 1.5f), Vector3.Interpolate(a, b, 1.5f, true));
            // In-range is fine.
            Assert.AreEqual(new Vector3(0.25f, 0.25f, 0.25f), Vector3.Interpolate(a, b, 0.25f));
        }

        [TestMethod]
        public void Interpolate_OutOfRange_Throws_ForVector2AndVector4()
        {
            Action a2 = () => Vector2.Interpolate(Vector2.Zero, Vector2.One, 2f);
            Action a4 = () => Vector4.Interpolate(Vector4.Zero, Vector4.One, -1f);
            Assert.ThrowsException<ArgumentOutOfRangeException>(a2);
            Assert.ThrowsException<ArgumentOutOfRangeException>(a4);

            // Instance extrapolating form is allowed.
            Assert.AreEqual(new Vector4(2, 2, 2, 2), Vector4.Zero.Interpolate(Vector4.One, 2f, true));
        }

        // ---------------------------------------------------------------- Vector3 cross / mixed products

        [TestMethod]
        public void Vector3_CrossProduct_IsRightHanded()
        {
            Assert.AreEqual(Vector3.UnitZ, Vector3.CrossProduct(Vector3.UnitX, Vector3.UnitY));
            Assert.AreEqual(Vector3.UnitX, Vector3.CrossProduct(Vector3.UnitY, Vector3.UnitZ));
            Assert.AreEqual(Vector3.UnitY, Vector3.CrossProduct(Vector3.UnitZ, Vector3.UnitX));
        }

        [TestMethod]
        public void Vector3_MixedProduct_EqualsScalarTripleAndDeterminant()
        {
            var a = new Vector3(1, 2, 3);
            var b = new Vector3(4, 5, 6);
            var c = new Vector3(7, 8, 10);

            // Determinant of [a; b; c].
            float det =
                a.X * (b.Y * c.Z - b.Z * c.Y)
                - a.Y * (b.X * c.Z - b.Z * c.X)
                + a.Z * (b.X * c.Y - b.Y * c.X);

            Assert.AreEqual(det, Vector3.MixedProduct(a, b, c), 1e-3f);
            Assert.AreEqual(Vector3.MixedProduct(a, b, c), a.MixedProduct(b, c));

            // Unit basis box has volume 1.
            Assert.AreEqual(1f, Vector3.MixedProduct(Vector3.UnitX, Vector3.UnitY, Vector3.UnitZ));
        }

        // ---------------------------------------------------------------- Vector2 rotation / signed angle

        [TestMethod]
        public void Vector2_Rotate_QuarterTurn_MapsXtoY()
        {
            var r = new Vector2(1, 0).Rotate((float)(Math.PI / 2));
            Assert.IsTrue(r.ApproximatelyEquals(new Vector2(0, 1), 1e-6f));
        }

        [TestMethod]
        public void Vector2_SignedAngle_HasCorrectSign()
        {
            // From +X to +Y is +90° (counter-clockwise); the reverse is -90°.
            Assert.AreEqual((float)(Math.PI / 2), Vector2.SignedAngle(Vector2.UnitX, Vector2.UnitY), 1e-5f);
            Assert.AreEqual(-(float)(Math.PI / 2), Vector2.SignedAngle(Vector2.UnitY, Vector2.UnitX), 1e-5f);
        }

        [TestMethod]
        public void Vector2_PerpendicularCW_And_CCW_AreOpposite()
        {
            var v = new Vector2(2, 3);
            Assert.AreEqual(-v.PerpendicularCW(), v.Perpendicular());
        }

        // ---------------------------------------------------------------- Vector4 dehomogenize

        [TestMethod]
        public void Vector4_Dehomogenize_PerformsPerspectiveDivide()
        {
            Assert.AreEqual(new Vector3(1, 2, 3), new Vector4(2, 4, 6, 2).Dehomogenize());
            Assert.IsTrue(new Vector4(10, 20, 30, 10).Dehomogenize().ApproximatelyEquals(new Vector3(1, 2, 3), Tol));
        }

        // ---------------------------------------------------------------- IsPerpendicular

        [TestMethod]
        public void IsPerpendicular_DetectsRightAngles()
        {
            Assert.IsTrue(Vector2.UnitX.IsPerpendicular(Vector2.UnitY));
            Assert.IsFalse(new Vector2(1, 1).IsPerpendicular(new Vector2(1, 0)));
            Assert.IsTrue(Vector3.UnitX.IsPerpendicular(Vector3.UnitZ));
            Assert.IsTrue(Vector4.UnitX.IsPerpendicular(Vector4.UnitW));
        }

        // ---------------------------------------------------------------- IFormattable

        [TestMethod]
        public void IFormattable_ComponentSelector_And_NumericFormat()
        {
            var inv = CultureInfo.InvariantCulture;

            var v2 = new Vector2(1.5f, 2.5f);
            Assert.AreEqual("1.5", v2.ToString("x", inv));
            Assert.AreEqual("2.5", v2.ToString("y", inv));
            Assert.AreEqual("(1.5, 2.5)", v2.ToString("F1", inv));

            var v3 = new Vector3(1f, 2f, 3f);
            Assert.AreEqual("3", v3.ToString("z", inv));
            Assert.AreEqual("(1.00, 2.00, 3.00)", v3.ToString("F2", inv));

            var v4 = new Vector4(1f, 2f, 3f, 4f);
            Assert.AreEqual("4", v4.ToString("w", inv));
            Assert.AreEqual("(1, 2, 3, 4)", ((IFormattable)v4).ToString(null, inv));
        }

        // ---------------------------------------------------------------- Constants

        [TestMethod]
        public void Constants_HaveExpectedValues()
        {
            Assert.AreEqual(float.MinValue, Vector2.MinValue.X);
            Assert.AreEqual(float.MaxValue, Vector2.MaxValue.Y);
            Assert.AreEqual(float.Epsilon, Vector2.Epsilon.X);
            Assert.IsTrue(Vector2.NaN.IsNaN());

            Assert.AreEqual(float.MaxValue, Vector3.MaxValue.Z);
            Assert.IsTrue(Vector3.NaN.IsNaN());

            Assert.AreEqual(float.MinValue, Vector4.MinValue.W);
            Assert.IsTrue(Vector4.NaN.IsNaN());
        }

        // ---------------------------------------------------------------- Component maths / rounding

        [TestMethod]
        public void ComponentMaths_BehaveElementWise()
        {
            Assert.AreEqual(new Vector3(1, 2, 3), new Vector3(1, 4, 9).SqrtComponents());
            Assert.AreEqual(new Vector3(4, 9, 16), new Vector3(2, 3, 4).SqrComponents());
            Assert.IsTrue(new Vector3(2, 3, 4).PowComponents(2f).ApproximatelyEquals(new Vector3(4, 9, 16), Tol));
            Assert.AreEqual(6f, new Vector3(1, 2, 3).SumComponents());
            Assert.AreEqual(14f, new Vector3(1, 2, 3).SumComponentSqrs());
        }

        [TestMethod]
        public void Round_RoundsEachComponent()
        {
            Assert.IsTrue(new Vector2(1.234f, 5.678f).Round(1).ApproximatelyEquals(new Vector2(1.2f, 5.7f), Tol));
            Assert.AreEqual(new Vector4(2, 2, 4, 4), new Vector4(1.5f, 2.5f, 3.5f, 4.5f).Round(MidpointRounding.ToEven));
        }

        // ---------------------------------------------------------------- Vector4 added gaps

        [TestMethod]
        public void Vector4_ClampMagnitude_And_MoveTowards()
        {
            Assert.AreEqual(1f, new Vector4(3, 4, 0, 0).ClampMagnitude(1f).Length, Tol);
            Assert.AreEqual(new Vector4(0.1f, 0, 0, 0), new Vector4(0.1f, 0, 0, 0).ClampMagnitude(1f));

            Assert.AreEqual(new Vector4(3, 0, 0, 0), Vector4.MoveTowards(Vector4.Zero, new Vector4(10, 0, 0, 0), 3f));
            // Never overshoots.
            Assert.AreEqual(new Vector4(2, 0, 0, 0), Vector4.MoveTowards(Vector4.Zero, new Vector4(2, 0, 0, 0), 5f));
        }
    }
}
