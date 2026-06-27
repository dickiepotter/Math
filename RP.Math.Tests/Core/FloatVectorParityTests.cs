namespace RP.Math.Tests.Core
{
    using System;
    using System.Globalization;
    using FluentAssertions;
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

            Vector2.DotProduct(a, b).Should().Be(Vector2.Dot(a, b));
            a.DotProduct(b).Should().Be(a.Dot(b));
            Vector2.CrossProduct(a, b).Should().Be(Vector2.Cross(a, b));
            a.CrossProduct(b).Should().Be(Vector2.Cross(a, b));
            a.Magnitude.Should().Be(a.Length);
            a.MagnitudeSquared.Should().Be(a.LengthSquared);
            Vector2.ComponentMin(a, b).Should().Be(Vector2.Min(a, b));
            Vector2.ComponentMax(a, b).Should().Be(Vector2.Max(a, b));
            a.AbsComponents().Should().Be(a.Abs());
            a.IsUnitVector().Should().Be(a.IsUnit());
            a.Distance(b).Should().Be(Vector2.Distance(a, b));
            a.DistanceSquared(b).Should().Be(Vector2.DistanceSquared(a, b));
            Vector2.UnitX.Equals(Vector2.UnitX, Tol).Should().BeTrue();
        }

        [TestMethod]
        public void Vector3_Aliases_MatchTerseForms()
        {
            var a = new Vector3(3, 4, 5);
            var b = new Vector3(-2, 5, 1);

            Vector3.DotProduct(a, b).Should().Be(Vector3.Dot(a, b));
            a.DotProduct(b).Should().Be(a.Dot(b));
            Vector3.CrossProduct(a, b).Should().Be(Vector3.Cross(a, b));
            a.CrossProduct(b).Should().Be(a.Cross(b));
            a.Magnitude.Should().Be(a.Length);
            a.MagnitudeSquared.Should().Be(a.LengthSquared);
            Vector3.ComponentMin(a, b).Should().Be(Vector3.Min(a, b));
            Vector3.ComponentMax(a, b).Should().Be(Vector3.Max(a, b));
            a.Projection(b).Should().Be(a.Project(b));
            a.Rejection(b).Should().Be(a.Reject(b));
            a.AbsComponents().Should().Be(a.Abs());
            a.IsUnitVector().Should().Be(a.IsUnit());
            a.Distance(b).Should().Be(Vector3.Distance(a, b));
        }

        [TestMethod]
        public void Vector4_Aliases_MatchTerseForms()
        {
            var a = new Vector4(3, 4, 5, 6);
            var b = new Vector4(-2, 5, 1, 0);

            Vector4.DotProduct(a, b).Should().Be(Vector4.Dot(a, b));
            a.DotProduct(b).Should().Be(a.Dot(b));
            a.Magnitude.Should().Be(a.Length);
            a.MagnitudeSquared.Should().Be(a.LengthSquared);
            Vector4.ComponentMin(a, b).Should().Be(Vector4.Min(a, b));
            Vector4.ComponentMax(a, b).Should().Be(Vector4.Max(a, b));
            a.AbsComponents().Should().Be(a.Abs());
            a.Distance(b).Should().Be(Vector4.Distance(a, b));
        }

        // ---------------------------------------------------------------- Projection / rejection

        [TestMethod]
        public void Projection_Plus_Rejection_Reconstructs_And_RejectionIsPerpendicular()
        {
            var v2 = new Vector2(3, 4);
            var d2 = new Vector2(1, 0);
            (v2.Projection(d2) + v2.Rejection(d2)).ApproximatelyEquals(v2, Tol).Should().BeTrue();
            Vector2.Dot(v2.Rejection(d2), d2).Should().BeApproximately(0f, Tol);

            var v3 = new Vector3(2, 3, 4);
            var d3 = new Vector3(0, 1, 0);
            (v3.Projection(d3) + v3.Rejection(d3)).ApproximatelyEquals(v3, Tol).Should().BeTrue();
            Vector3.Dot(v3.Rejection(d3), d3).Should().BeApproximately(0f, Tol);

            var v4 = new Vector4(2, 3, 4, 5);
            var d4 = new Vector4(0, 0, 1, 0);
            (v4.Projection(d4) + v4.Rejection(d4)).ApproximatelyEquals(v4, Tol).Should().BeTrue();
            Vector4.Dot(v4.Rejection(d4), d4).Should().BeApproximately(0f, Tol);
        }

        [TestMethod]
        public void Projection_ZeroDirection_ReturnsZero()
        {
            new Vector2(3, 4).Projection(Vector2.Zero).Should().Be(Vector2.Zero);
            new Vector4(3, 4, 5, 6).Projection(Vector4.Zero).Should().Be(Vector4.Zero);
        }

        // ---------------------------------------------------------------- Reflection / reflect

        [TestMethod]
        public void Reflect_AboutNormal_FlipsTheNormalComponent()
        {
            // Incoming velocity hitting a wall whose normal is +Y: Y flips, X/Z keep.
            new Vector2(1, -1).Reflect(Vector2.UnitY).Should().Be(new Vector2(1, 1));
            new Vector3(1, -1, 2).Reflect(Vector3.UnitY).Should().Be(new Vector3(1, 1, 2));
            new Vector4(1, -1, 2, 3).Reflect(Vector4.UnitY).Should().Be(new Vector4(1, 1, 2, 3));
        }

        [TestMethod]
        public void Reflection_AboutLine_PreservesMagnitude()
        {
            var v2 = new Vector2(3, 4);
            v2.Reflection(new Vector2(1, 2)).Length.Should().BeApproximately(v2.Length, Tol);

            var v3 = new Vector3(3, 4, 5);
            v3.Reflection(new Vector3(1, 2, 3)).Length.Should().BeApproximately(v3.Length, Tol);

            var v4 = new Vector4(3, 4, 5, 6);
            v4.Reflection(new Vector4(1, 2, 3, 4)).Length.Should().BeApproximately(v4.Length, Tol);
        }

        [TestMethod]
        public void Reflection_AcrossOwnDirection_ReturnsSelf()
        {
            var v = new Vector2(2, 5);
            v.Reflection(v).ApproximatelyEquals(v, Tol).Should().BeTrue();
        }

        // ---------------------------------------------------------------- Slerp

        [TestMethod]
        public void Slerp_Midpoint_IsUnitLength_At45Degrees()
        {
            var mid = Vector3.Slerp(Vector3.UnitX, Vector3.UnitY, 0.5f);
            mid.Length.Should().BeApproximately(1f, Tol);
            // 45° between UnitX and the midpoint.
            Vector3.Angle(Vector3.UnitX, mid).Should().BeApproximately((float)(Math.PI / 4), 1e-4f);

            var mid2 = Vector2.Slerp(Vector2.UnitX, Vector2.UnitY, 0.5f);
            mid2.Length.Should().BeApproximately(1f, Tol);
        }

        [TestMethod]
        public void Slerp_Antiparallel_DoesNotProduceNaN()
        {
            var r = Vector3.Slerp(Vector3.UnitX, -Vector3.UnitX, 0.5f);
            r.IsNaN().Should().BeFalse();

            var r2 = Vector2.Slerp(Vector2.UnitX, -Vector2.UnitX, 0.5f);
            r2.IsNaN().Should().BeFalse();

            var r4 = Vector4.Slerp(Vector4.UnitX, -Vector4.UnitX, 0.5f);
            r4.IsNaN().Should().BeFalse();
        }

        [TestMethod]
        public void Slerp_Endpoints_AreExact()
        {
            Vector3.Slerp(Vector3.UnitX, Vector3.UnitY, 0f).ApproximatelyEquals(Vector3.UnitX, Tol).Should().BeTrue();
            Vector3.Slerp(Vector3.UnitX, Vector3.UnitY, 1f).ApproximatelyEquals(Vector3.UnitY, Tol).Should().BeTrue();
        }

        // ---------------------------------------------------------------- Interpolate vs Lerp

        [TestMethod]
        public void Interpolate_OutOfRange_Throws_ButLerpAndExtrapolationDoNot()
        {
            var a = Vector3.Zero;
            var b = Vector3.One;

            Action over = () => Vector3.Interpolate(a, b, 1.5f);
            Action under = () => Vector3.Interpolate(a, b, -0.5f);
            over.Should().Throw<ArgumentOutOfRangeException>();
            under.Should().Throw<ArgumentOutOfRangeException>();

            // Lerp extrapolates without complaint.
            Vector3.Lerp(a, b, 1.5f).Should().Be(new Vector3(1.5f, 1.5f, 1.5f));
            // Explicit allowExtrapolation does too.
            Vector3.Interpolate(a, b, 1.5f, true).Should().Be(new Vector3(1.5f, 1.5f, 1.5f));
            // In-range is fine.
            Vector3.Interpolate(a, b, 0.25f).Should().Be(new Vector3(0.25f, 0.25f, 0.25f));
        }

        [TestMethod]
        public void Interpolate_OutOfRange_Throws_ForVector2AndVector4()
        {
            Action a2 = () => Vector2.Interpolate(Vector2.Zero, Vector2.One, 2f);
            Action a4 = () => Vector4.Interpolate(Vector4.Zero, Vector4.One, -1f);
            a2.Should().Throw<ArgumentOutOfRangeException>();
            a4.Should().Throw<ArgumentOutOfRangeException>();

            // Instance extrapolating form is allowed.
            Vector4.Zero.Interpolate(Vector4.One, 2f, true).Should().Be(new Vector4(2, 2, 2, 2));
        }

        // ---------------------------------------------------------------- Vector3 cross / mixed products

        [TestMethod]
        public void Vector3_CrossProduct_IsRightHanded()
        {
            Vector3.CrossProduct(Vector3.UnitX, Vector3.UnitY).Should().Be(Vector3.UnitZ);
            Vector3.CrossProduct(Vector3.UnitY, Vector3.UnitZ).Should().Be(Vector3.UnitX);
            Vector3.CrossProduct(Vector3.UnitZ, Vector3.UnitX).Should().Be(Vector3.UnitY);
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

            Vector3.MixedProduct(a, b, c).Should().BeApproximately(det, 1e-3f);
            a.MixedProduct(b, c).Should().Be(Vector3.MixedProduct(a, b, c));

            // Unit basis box has volume 1.
            Vector3.MixedProduct(Vector3.UnitX, Vector3.UnitY, Vector3.UnitZ).Should().Be(1f);
        }

        // ---------------------------------------------------------------- Vector2 rotation / signed angle

        [TestMethod]
        public void Vector2_Rotate_QuarterTurn_MapsXtoY()
        {
            var r = new Vector2(1, 0).Rotate((float)(Math.PI / 2));
            r.ApproximatelyEquals(new Vector2(0, 1), 1e-6f).Should().BeTrue();
        }

        [TestMethod]
        public void Vector2_SignedAngle_HasCorrectSign()
        {
            // From +X to +Y is +90° (counter-clockwise); the reverse is -90°.
            Vector2.SignedAngle(Vector2.UnitX, Vector2.UnitY).Should().BeApproximately((float)(Math.PI / 2), 1e-5f);
            Vector2.SignedAngle(Vector2.UnitY, Vector2.UnitX).Should().BeApproximately(-(float)(Math.PI / 2), 1e-5f);
        }

        [TestMethod]
        public void Vector2_PerpendicularCW_And_CCW_AreOpposite()
        {
            var v = new Vector2(2, 3);
            v.Perpendicular().Should().Be(-v.PerpendicularCW());
        }

        // ---------------------------------------------------------------- Vector4 dehomogenize

        [TestMethod]
        public void Vector4_Dehomogenize_PerformsPerspectiveDivide()
        {
            new Vector4(2, 4, 6, 2).Dehomogenize().Should().Be(new Vector3(1, 2, 3));
            new Vector4(10, 20, 30, 10).Dehomogenize().ApproximatelyEquals(new Vector3(1, 2, 3), Tol).Should().BeTrue();
        }

        // ---------------------------------------------------------------- IsPerpendicular

        [TestMethod]
        public void IsPerpendicular_DetectsRightAngles()
        {
            Vector2.UnitX.IsPerpendicular(Vector2.UnitY).Should().BeTrue();
            new Vector2(1, 1).IsPerpendicular(new Vector2(1, 0)).Should().BeFalse();
            Vector3.UnitX.IsPerpendicular(Vector3.UnitZ).Should().BeTrue();
            Vector4.UnitX.IsPerpendicular(Vector4.UnitW).Should().BeTrue();
        }

        // ---------------------------------------------------------------- IFormattable

        [TestMethod]
        public void IFormattable_ComponentSelector_And_NumericFormat()
        {
            var inv = CultureInfo.InvariantCulture;

            var v2 = new Vector2(1.5f, 2.5f);
            v2.ToString("x", inv).Should().Be("1.5");
            v2.ToString("y", inv).Should().Be("2.5");
            v2.ToString("F1", inv).Should().Be("(1.5, 2.5)");

            var v3 = new Vector3(1f, 2f, 3f);
            v3.ToString("z", inv).Should().Be("3");
            v3.ToString("F2", inv).Should().Be("(1.00, 2.00, 3.00)");

            var v4 = new Vector4(1f, 2f, 3f, 4f);
            v4.ToString("w", inv).Should().Be("4");
            ((IFormattable)v4).ToString(null, inv).Should().Be("(1, 2, 3, 4)");
        }

        // ---------------------------------------------------------------- Constants

        [TestMethod]
        public void Constants_HaveExpectedValues()
        {
            Vector2.MinValue.X.Should().Be(float.MinValue);
            Vector2.MaxValue.Y.Should().Be(float.MaxValue);
            Vector2.Epsilon.X.Should().Be(float.Epsilon);
            Vector2.NaN.IsNaN().Should().BeTrue();

            Vector3.MaxValue.Z.Should().Be(float.MaxValue);
            Vector3.NaN.IsNaN().Should().BeTrue();

            Vector4.MinValue.W.Should().Be(float.MinValue);
            Vector4.NaN.IsNaN().Should().BeTrue();
        }

        // ---------------------------------------------------------------- Component maths / rounding

        [TestMethod]
        public void ComponentMaths_BehaveElementWise()
        {
            new Vector3(1, 4, 9).SqrtComponents().Should().Be(new Vector3(1, 2, 3));
            new Vector3(2, 3, 4).SqrComponents().Should().Be(new Vector3(4, 9, 16));
            new Vector3(2, 3, 4).PowComponents(2f).ApproximatelyEquals(new Vector3(4, 9, 16), Tol).Should().BeTrue();
            new Vector3(1, 2, 3).SumComponents().Should().Be(6f);
            new Vector3(1, 2, 3).SumComponentSqrs().Should().Be(14f);
        }

        [TestMethod]
        public void Round_RoundsEachComponent()
        {
            new Vector2(1.234f, 5.678f).Round(1).ApproximatelyEquals(new Vector2(1.2f, 5.7f), Tol).Should().BeTrue();
            new Vector4(1.5f, 2.5f, 3.5f, 4.5f).Round(MidpointRounding.ToEven)
                .Should().Be(new Vector4(2, 2, 4, 4));
        }

        // ---------------------------------------------------------------- Vector4 added gaps

        [TestMethod]
        public void Vector4_ClampMagnitude_And_MoveTowards()
        {
            new Vector4(3, 4, 0, 0).ClampMagnitude(1f).Length.Should().BeApproximately(1f, Tol);
            new Vector4(0.1f, 0, 0, 0).ClampMagnitude(1f).Should().Be(new Vector4(0.1f, 0, 0, 0));

            Vector4.MoveTowards(Vector4.Zero, new Vector4(10, 0, 0, 0), 3f)
                .Should().Be(new Vector4(3, 0, 0, 0));
            // Never overshoots.
            Vector4.MoveTowards(Vector4.Zero, new Vector4(2, 0, 0, 0), 5f)
                .Should().Be(new Vector4(2, 0, 0, 0));
        }
    }
}
