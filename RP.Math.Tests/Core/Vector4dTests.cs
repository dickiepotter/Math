namespace RP.Math.Tests.Core
{
    using System;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using RP.Math;
    using RP.Math.Exceptions;

    /// <summary>
    /// Edge-case-driven tests for <see cref="Vector4d"/>, the 4-D double-precision vector, including the
    /// homogeneous-coordinate helpers and the dimension-independent Kahan angle.
    /// </summary>
    [TestClass]
    public sealed class Vector4dTests
    {
        private const double Tol = 1e-12;

        #region Construction and accessors

        [TestMethod]
        public void Constructor_And_Accessors()
        {
            var v = new Vector4d(1, 2, 2, 4); // magnitude = sqrt(1+4+4+16) = 5
            v.X.Should().Be(1);
            v.W.Should().Be(4);
            v.Magnitude.Should().BeApproximately(5, Tol);
            v[2].Should().Be(2);
            v[3].Should().Be(4);
            v.Array.Should().Equal(1.0, 2.0, 2.0, 4.0);
            v.XYZ.Should().Be(new Vector3d(1, 2, 2));
        }

        [TestMethod]
        public void Constructor_FromVector3dAndW()
        {
            new Vector4d(new Vector3d(1, 2, 3), 4).Should().Be(new Vector4d(1, 2, 3, 4));
        }

        [TestMethod]
        public void Indexer_OutOfRange_Throws()
        {
            var v = new Vector4d(1, 2, 3, 4);
            Action act = () => { var _ = v[4]; };
            act.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void Array_Constructor_WrongLength_Throws()
        {
            Action act = () => new Vector4d(new[] { 1.0, 2.0, 3.0 });
            act.Should().Throw<ArgumentException>();
        }

        #endregion

        #region Operators and products

        [TestMethod]
        public void Arithmetic_And_Dot()
        {
            var a = new Vector4d(1, 2, 3, 4);
            var b = new Vector4d(4, 3, 2, 1);

            (a + b).Should().Be(new Vector4d(5, 5, 5, 5));
            (b - a).Should().Be(new Vector4d(3, 1, -1, -3));
            (a * 2).Should().Be(new Vector4d(2, 4, 6, 8));
            (a * b).Should().Be(new Vector4d(4, 6, 6, 4)); // component-wise
            Vector4d.DotProduct(a, b).Should().Be(4 + 6 + 6 + 4);
            a.Dot(b).Should().Be(20); // alias
        }

        [TestMethod]
        public void Comparison_Operators_AreByMagnitude()
        {
            (new Vector4d(1, 0, 0, 0) < new Vector4d(0, 0, 0, 2)).Should().BeTrue();
            (new Vector4d(3, 0, 0, 0) >= new Vector4d(0, 0, 0, 3)).Should().BeTrue();
        }

        [TestMethod]
        public void EqualityOperator_IsExact_And_NaNIsNotEqualToItself()
        {
            (new Vector4d(1, 2, 3, 4) == new Vector4d(1, 2, 3, 4)).Should().BeTrue();
            (Vector4d.NaN == Vector4d.NaN).Should().BeFalse();
        }

        #endregion

        #region Normalisation edge cases

        [TestMethod]
        public void Normalize_ProducesUnitVector()
        {
            var n = new Vector4d(1, 2, 2, 4).Normalize();
            n.Magnitude.Should().BeApproximately(1, Tol);
            n.Equals(new Vector4d(0.2, 0.4, 0.4, 0.8), Tol).Should().BeTrue();
        }

        [TestMethod]
        public void Normalize_Zero_Throws()
        {
            Action act = () => Vector4d.Origin.Normalize();
            act.Should().Throw<NormalizeVectorException>();
        }

        [TestMethod]
        public void NormalizeOrDefault_Zero_ReturnsOrigin_NaN_ReturnsNaN()
        {
            Vector4d.Origin.NormalizeOrDefault().Should().Be(Vector4d.Origin);
            Vector4d.NaN.NormalizeOrDefault().IsNaN().Should().BeTrue();
        }

        [TestMethod]
        public void Normalize_AxisAlignedInfinity_IsASpecialCase()
        {
            var n = new Vector4d(0, double.PositiveInfinity, 0, 0).NormalizeOrDefault();
            n.Equals(Vector4d.YAxis, Tol).Should().BeTrue();
        }

        [TestMethod]
        public void Normalize_HalfInfiniteVector_Throws()
        {
            Action act = () => new Vector4d(double.PositiveInfinity, 5, 0, 0).Normalize();
            act.Should().Throw<NormalizeVectorException>();
        }

        #endregion

        #region Interpolation

        [TestMethod]
        public void Interpolate_Midpoint_And_RangeCheck()
        {
            Vector4d.Interpolate(Vector4d.Origin, new Vector4d(2, 4, 6, 8), 0.5)
                .Should().Be(new Vector4d(1, 2, 3, 4));

            Action act = () => Vector4d.Interpolate(Vector4d.Origin, Vector4d.One, -0.5);
            act.Should().Throw<ArgumentOutOfRangeException>();

            Vector4d.Lerp(Vector4d.Origin, new Vector4d(2, 2, 2, 2), 2)
                .Should().Be(new Vector4d(4, 4, 4, 4)); // extrapolation
        }

        [TestMethod]
        public void Slerp_QuarterTurn_StaysUnit()
        {
            var mid = Vector4d.Slerp(Vector4d.XAxis, Vector4d.YAxis, 0.5);
            mid.Magnitude.Should().BeApproximately(1, Tol);
        }

        [TestMethod]
        public void Slerp_Antiparallel_FallsBackToLerp_NoNaN()
        {
            Vector4d.Slerp(Vector4d.XAxis, -Vector4d.XAxis, 0.5).IsNaN().Should().BeFalse();
        }

        #endregion

        #region Angle (Kahan)

        [TestMethod]
        public void Angle_KnownCases_IncludingAntiparallel()
        {
            Vector4d.Angle(Vector4d.XAxis, Vector4d.YAxis).Should().BeApproximately(Math.PI / 2, Tol);
            Vector4d.Angle(Vector4d.XAxis, Vector4d.XAxis).Should().Be(0);
            // The Kahan form must return exactly π (no NaN) for opposite vectors.
            Vector4d.Angle(Vector4d.XAxis, -Vector4d.XAxis).Should().BeApproximately(Math.PI, Tol);
        }

        [TestMethod]
        public void Angle_NearlyParallel_IsAccurate()
        {
            // A tiny angle where the acos(dot) form would lose all precision; Kahan keeps it.
            var a = new Vector4d(1, 0, 0, 0);
            var b = new Vector4d(1, 1e-7, 0, 0);
            Vector4d.Angle(a, b).Should().BeApproximately(1e-7, 1e-12);
        }

        #endregion

        #region Projection, rejection, reflection

        [TestMethod]
        public void Projection_Plus_Rejection_Reconstructs_Original()
        {
            var v = new Vector4d(1, 2, 3, 4);
            var dir = new Vector4d(0, 1, 0, 0);
            v.Projection(dir).Should().Be(new Vector4d(0, 2, 0, 0));
            (v.Projection(dir) + v.Rejection(dir)).Equals(v, Tol).Should().BeTrue();
        }

        [TestMethod]
        public void Reflect_AboutNormal_FlipsTheNormalComponent()
        {
            new Vector4d(1, -1, 0, 0).Reflect(Vector4d.YAxis)
                .Equals(new Vector4d(1, 1, 0, 0), Tol).Should().BeTrue();
        }

        #endregion

        #region Component-wise, clamp, move, rounding

        [TestMethod]
        public void ComponentMinMax_Clamp_ClampMagnitude_MoveTowards()
        {
            var a = new Vector4d(1, 5, 1, 5);
            var b = new Vector4d(4, 2, 4, 2);
            Vector4d.ComponentMin(a, b).Should().Be(new Vector4d(1, 2, 1, 2));
            Vector4d.ComponentMax(a, b).Should().Be(new Vector4d(4, 5, 4, 5));

            new Vector4d(2, 0, 0, 0).ClampMagnitude(1).Magnitude.Should().BeApproximately(1, Tol);
            Vector4d.MoveTowards(Vector4d.Origin, new Vector4d(10, 0, 0, 0), 4)
                .Should().Be(new Vector4d(4, 0, 0, 0));
        }

        [TestMethod]
        public void Round_MidpointModes()
        {
            var v = new Vector4d(0.5, 1.5, 2.5, 3.5);
            Vector4d.Round(v, MidpointRounding.ToEven).Should().Be(new Vector4d(0, 2, 2, 4));
            Vector4d.Round(v, MidpointRounding.AwayFromZero).Should().Be(new Vector4d(1, 2, 3, 4));
        }

        #endregion

        #region Homogeneous coordinates

        [TestMethod]
        public void Dehomogenize_DoesThePerspectiveDivide()
        {
            new Vector4d(2, 4, 6, 2).Dehomogenize().Should().Be(new Vector3d(1, 2, 3));
        }

        [TestMethod]
        public void Dehomogenize_Direction_GivesInfinity()
        {
            // w == 0 is a point at infinity (a pure direction).
            var d = new Vector4d(1, 0, 0, 0).Dehomogenize();
            double.IsInfinity(d.X).Should().BeTrue();
        }

        #endregion

        #region Conversions and equality

        [TestMethod]
        public void Conversions_WithFloatSibling()
        {
            Vector4d widened = new Vector4(1.5f, 2.5f, 3.5f, 4.5f); // implicit
            widened.Should().Be(new Vector4d(1.5, 2.5, 3.5, 4.5));

            var narrowed = (Vector4)new Vector4d(1.5, 2.5, 3.5, 4.5); // explicit
            narrowed.Should().Be(new Vector4(1.5f, 2.5f, 3.5f, 4.5f));

            var (x, y, z, w) = new Vector4d(5, 6, 7, 8);
            (x, y, z, w).Should().Be((5.0, 6.0, 7.0, 8.0));
        }

        [TestMethod]
        public void Equals_NaN_And_Tolerance_And_Hash()
        {
            Vector4d.NaN.Equals(Vector4d.NaN).Should().BeTrue();
            new Vector4d(1, 2, 3, 4).Equals(new Vector4d(1, 2, 3, 4.0000001), 1e-3).Should().BeTrue();
            new Vector4d(1, 2, 3, 4).GetHashCode().Should().Be(new Vector4d(1, 2, 3, 4).GetHashCode());
        }

        [TestMethod]
        public void CompareTo_Tolerance_BothInfinite_IsZero()
        {
            var infA = new Vector4d(double.PositiveInfinity, 0, 0, 0);
            var infB = new Vector4d(0, 0, 0, double.PositiveInfinity);
            infA.CompareTo(infB, 1e-6).Should().Be(0);
        }

        #endregion

        #region Decisions and constants

        [TestMethod]
        public void Decisions_And_Constants()
        {
            Vector4d.WAxis.IsUnitVector().Should().BeTrue();
            Vector4d.XAxis.IsPerpendicular(Vector4d.YAxis).Should().BeTrue();
            Vector4d.NaN.IsNaN().Should().BeTrue();
            Vector4d.Origin.IsZero().Should().BeTrue();

            Vector4d.Zero.Should().Be(Vector4d.Origin);
            Vector4d.One.Should().Be(new Vector4d(1, 1, 1, 1));
            Vector4d.UnitW.Should().Be(Vector4d.WAxis);
        }

        #endregion
    }
}
