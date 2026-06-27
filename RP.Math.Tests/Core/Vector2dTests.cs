namespace RP.Math.Tests.Core
{
    using System;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using RP.Math;
    using RP.Math.Exceptions;

    /// <summary>
    /// Edge-case-driven tests for <see cref="Vector2d"/>, the planar double-precision vector. These probe the
    /// numeric corners (NaN, infinity, zero length, antiparallel angles, rounding midpoints, tolerance
    /// boundaries) as well as the routine algebra.
    /// </summary>
    [TestClass]
    public sealed class Vector2dTests
    {
        private const double Tol = 1e-12;

        #region Construction and accessors

        [TestMethod]
        public void Constructor_And_Accessors()
        {
            var v = new Vector2d(3, -4);
            v.X.Should().Be(3);
            v.Y.Should().Be(-4);
            v.Magnitude.Should().BeApproximately(5, Tol);
            v.MagnitudeSquared.Should().BeApproximately(25, Tol);
            v.Length.Should().Be(v.Magnitude);
            v.LengthSquared.Should().Be(v.MagnitudeSquared);
            v[0].Should().Be(3);
            v[1].Should().Be(-4);
            v.Array.Should().Equal(3.0, -4.0);
        }

        [TestMethod]
        public void Indexer_OutOfRange_Throws()
        {
            var v = new Vector2d(1, 2);
            Action act = () => { var _ = v[2]; };
            act.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void Array_Constructor_WrongLength_Throws()
        {
            Action act = () => new Vector2d(new[] { 1.0, 2.0, 3.0 });
            act.Should().Throw<ArgumentException>();
        }

        #endregion

        #region Operators

        [TestMethod]
        public void Arithmetic_Operators()
        {
            var a = new Vector2d(1, 2);
            var b = new Vector2d(3, 4);

            (a + b).Should().Be(new Vector2d(4, 6));
            (b - a).Should().Be(new Vector2d(2, 2));
            (a * 2).Should().Be(new Vector2d(2, 4));
            (2 * a).Should().Be(new Vector2d(2, 4));
            (a * b).Should().Be(new Vector2d(3, 8)); // component-wise
            (b / 2).Should().Be(new Vector2d(1.5, 2));
            (-a).Should().Be(new Vector2d(-1, -2));
            (+a).Should().Be(a);
        }

        [TestMethod]
        public void Comparison_Operators_AreByMagnitude()
        {
            var small = new Vector2d(1, 0);
            var big = new Vector2d(0, 3);

            (small < big).Should().BeTrue();
            (big > small).Should().BeTrue();
            (small <= new Vector2d(0, 1)).Should().BeTrue(); // equal magnitude
            (small >= new Vector2d(0, 1)).Should().BeTrue();
        }

        [TestMethod]
        public void EqualityOperator_IsExact_And_NaNIsNotEqualToItself()
        {
            (new Vector2d(1, 2) == new Vector2d(1, 2)).Should().BeTrue();
            (new Vector2d(1, 2) != new Vector2d(1, 3)).Should().BeTrue();

            // Per IEEE-754, NaN != NaN — the == operator must reflect that even though Equals() does not.
            (Vector2d.NaN == Vector2d.NaN).Should().BeFalse();
        }

        #endregion

        #region Products

        [TestMethod]
        public void DotProduct_And_Alias_Agree()
        {
            var a = new Vector2d(1, 2);
            var b = new Vector2d(3, 4);

            Vector2d.DotProduct(a, b).Should().Be(11);
            a.DotProduct(b).Should().Be(11);
            Vector2d.Dot(a, b).Should().Be(11);
            a.Dot(b).Should().Be(11);
        }

        [TestMethod]
        public void CrossProduct_IsSignedArea_PositiveCounterClockwise()
        {
            // X cross Y is +1 (CCW); Y cross X is -1 (CW); parallel is 0.
            Vector2d.CrossProduct(Vector2d.XAxis, Vector2d.YAxis).Should().Be(1);
            Vector2d.CrossProduct(Vector2d.YAxis, Vector2d.XAxis).Should().Be(-1);
            Vector2d.CrossProduct(new Vector2d(2, 2), new Vector2d(4, 4)).Should().Be(0);
            Vector2d.Cross(Vector2d.XAxis, Vector2d.YAxis).Should().Be(1); // alias
        }

        [TestMethod]
        public void Perpendicular_IsOrthogonal_And_RightHanded()
        {
            var v = new Vector2d(3, 1);
            v.Perpendicular().Should().Be(new Vector2d(-1, 3));      // 90° CCW
            v.PerpendicularCW().Should().Be(new Vector2d(1, -3));    // 90° CW
            v.DotProduct(v.Perpendicular()).Should().BeApproximately(0, Tol);
        }

        #endregion

        #region Normalisation edge cases

        [TestMethod]
        public void Normalize_ProducesUnitVector()
        {
            var n = new Vector2d(3, 4).Normalize();
            n.Magnitude.Should().BeApproximately(1, Tol);
            n.Equals(new Vector2d(0.6, 0.8), Tol).Should().BeTrue();
        }

        [TestMethod]
        public void Normalize_Zero_Throws()
        {
            Action act = () => Vector2d.Origin.Normalize();
            act.Should().Throw<NormalizeVectorException>();
        }

        [TestMethod]
        public void Normalize_NaN_Throws()
        {
            Action act = () => Vector2d.NaN.Normalize();
            act.Should().Throw<NormalizeVectorException>();
        }

        [TestMethod]
        public void NormalizeOrDefault_Zero_ReturnsOrigin_NaN_ReturnsNaN()
        {
            Vector2d.Origin.NormalizeOrDefault().Should().Be(Vector2d.Origin);
            Vector2d.NaN.NormalizeOrDefault().IsNaN().Should().BeTrue();
        }

        [TestMethod]
        public void Normalize_AxisAlignedInfinity_IsASpecialCase()
        {
            // (+inf, 0) has a well-defined direction: the +X axis.
            var n = new Vector2d(double.PositiveInfinity, 0).NormalizeOrDefault();
            n.Equals(Vector2d.XAxis, Tol).Should().BeTrue();
        }

        [TestMethod]
        public void Normalize_HalfInfiniteVector_Throws()
        {
            // (+inf, 5): the finite component vanishes against the infinite one — direction is undefined.
            Action act = () => new Vector2d(double.PositiveInfinity, 5).Normalize();
            act.Should().Throw<NormalizeVectorException>();
        }

        #endregion

        #region Interpolation

        [TestMethod]
        public void Interpolate_Midpoint()
        {
            Vector2d.Interpolate(new Vector2d(0, 0), new Vector2d(10, 20), 0.5)
                .Should().Be(new Vector2d(5, 10));
        }

        [TestMethod]
        public void Interpolate_OutOfRange_Throws_UnlessExtrapolationAllowed()
        {
            Action act = () => Vector2d.Interpolate(Vector2d.Origin, Vector2d.One, 1.5);
            act.Should().Throw<ArgumentOutOfRangeException>();

            Vector2d.Interpolate(Vector2d.Origin, new Vector2d(2, 2), 1.5, true)
                .Should().Be(new Vector2d(3, 3));
            Vector2d.Lerp(Vector2d.Origin, new Vector2d(2, 2), 1.5)
                .Should().Be(new Vector2d(3, 3));
        }

        [TestMethod]
        public void Slerp_QuarterTurn_TracksTheArc()
        {
            var mid = Vector2d.Slerp(Vector2d.XAxis, Vector2d.YAxis, 0.5);
            mid.Magnitude.Should().BeApproximately(1, Tol);
            var expected = new Vector2d(Math.Cos(Math.PI / 4), Math.Sin(Math.PI / 4));
            mid.Equals(expected, 1e-9).Should().BeTrue();
        }

        [TestMethod]
        public void Slerp_Antiparallel_FallsBackToLerp()
        {
            // sinθ ≈ 0, so the method must not divide by zero — it linearly interpolates instead.
            var mid = Vector2d.Slerp(Vector2d.XAxis, -Vector2d.XAxis, 0.5);
            mid.IsNaN().Should().BeFalse();
        }

        #endregion

        #region Angles

        [TestMethod]
        public void Angle_KnownCases()
        {
            Vector2d.Angle(Vector2d.XAxis, Vector2d.YAxis).Should().BeApproximately(Math.PI / 2, Tol);
            Vector2d.Angle(Vector2d.XAxis, Vector2d.XAxis).Should().Be(0);
            // Antiparallel must be exactly π, not NaN (the historic acos bug).
            Vector2d.Angle(Vector2d.XAxis, -Vector2d.XAxis).Should().BeApproximately(Math.PI, Tol);
        }

        [TestMethod]
        public void SignedAngle_IsOrientationAware()
        {
            Vector2d.SignedAngle(Vector2d.XAxis, Vector2d.YAxis).Should().BeApproximately(Math.PI / 2, Tol);
            Vector2d.SignedAngle(Vector2d.YAxis, Vector2d.XAxis).Should().BeApproximately(-Math.PI / 2, Tol);
        }

        #endregion

        #region Rotation

        [TestMethod]
        public void Rotate_QuarterTurn_MapsXToY()
        {
            new Vector2d(1, 0).Rotate(Math.PI / 2).Equals(Vector2d.YAxis, 1e-12).Should().BeTrue();
            new Vector2d(1, 0).Rotate(new Angle(Math.PI / 2)).Equals(Vector2d.YAxis, 1e-12).Should().BeTrue();
        }

        #endregion

        #region Projection, rejection, reflection

        [TestMethod]
        public void Projection_Plus_Rejection_Reconstructs_Original()
        {
            var v = new Vector2d(3, 4);
            var dir = new Vector2d(1, 1);
            (v.Projection(dir) + v.Rejection(dir)).Equals(v, Tol).Should().BeTrue();
            v.Rejection(dir).DotProduct(dir).Should().BeApproximately(0, Tol);
            v.Project(dir).Should().Be(v.Projection(dir)); // aliases
            v.Reject(dir).Should().Be(v.Rejection(dir));
        }

        [TestMethod]
        public void Reflect_AboutNormal_IsTheBounce()
        {
            // Travelling down-right, bouncing off the floor (normal +Y) flips the Y component.
            new Vector2d(1, -1).Reflect(Vector2d.YAxis).Equals(new Vector2d(1, 1), Tol).Should().BeTrue();
        }

        [TestMethod]
        public void Reflection_AboutVector_PreservesMagnitude()
        {
            var v = new Vector2d(3, 4);
            var mirror = new Vector2d(1, 0);
            var r = v.Reflection(mirror);
            r.Magnitude.Should().BeApproximately(v.Magnitude, 1e-9);
            // Mirroring (3,4) about the X axis gives (3,-4).
            r.Equals(new Vector2d(3, -4), 1e-9).Should().BeTrue();
        }

        #endregion

        #region Component-wise, clamp, move

        [TestMethod]
        public void ComponentMinMax_And_Clamp()
        {
            var a = new Vector2d(1, 5);
            var b = new Vector2d(4, 2);
            Vector2d.ComponentMin(a, b).Should().Be(new Vector2d(1, 2));
            Vector2d.ComponentMax(a, b).Should().Be(new Vector2d(4, 5));
            new Vector2d(5, -5).Clamp(Vector2d.Origin, new Vector2d(3, 3)).Should().Be(new Vector2d(3, 0));
        }

        [TestMethod]
        public void ClampMagnitude_CapsLength()
        {
            var v = new Vector2d(3, 4); // length 5
            v.ClampMagnitude(2.5).Magnitude.Should().BeApproximately(2.5, Tol);
            v.ClampMagnitude(10).Should().Be(v); // already shorter
        }

        [TestMethod]
        public void MoveTowards_NeverOvershoots()
        {
            var step = Vector2d.MoveTowards(Vector2d.Origin, new Vector2d(10, 0), 3);
            step.Should().Be(new Vector2d(3, 0));
            Vector2d.MoveTowards(Vector2d.Origin, new Vector2d(2, 0), 5)
                .Should().Be(new Vector2d(2, 0)); // arrives, no overshoot
        }

        [TestMethod]
        public void ComponentMaths()
        {
            var v = new Vector2d(-3, 4);
            v.SumComponents().Should().Be(1);
            v.SumComponentSqrs().Should().Be(25);
            v.AbsComponents().Should().Be(new Vector2d(3, 4));
            v.Abs().Should().Be(new Vector2d(3, 4));
            new Vector2d(2, 3).SqrComponents().Should().Be(new Vector2d(4, 9));
            new Vector2d(4, 9).SqrtComponents().Should().Be(new Vector2d(2, 3));
            new Vector2d(2, 3).PowComponents(2).Should().Be(new Vector2d(4, 9));
        }

        #endregion

        #region Rounding

        [TestMethod]
        public void Round_MidpointModes_Differ()
        {
            var v = new Vector2d(2.5, 3.5);
            Vector2d.Round(v, MidpointRounding.ToEven).Should().Be(new Vector2d(2, 4));
            Vector2d.Round(v, MidpointRounding.AwayFromZero).Should().Be(new Vector2d(3, 4));
            new Vector2d(1.2345, 6.789).Round(2).Should().Be(new Vector2d(1.23, 6.79));
        }

        #endregion

        #region Conversions

        [TestMethod]
        public void Conversions_WithFloatSibling_And_To3d()
        {
            Vector2d widened = new Vector2(1.5f, 2.5f); // implicit widening
            widened.Should().Be(new Vector2d(1.5, 2.5));

            var narrowed = (Vector2)new Vector2d(1.5, 2.5); // explicit narrowing
            narrowed.Should().Be(new Vector2(1.5f, 2.5f));

            new Vector2d(1, 2).ToVector3d(3).Should().Be(new Vector3d(1, 2, 3));

            var (x, y) = new Vector2d(7, 8);
            x.Should().Be(7);
            y.Should().Be(8);
        }

        #endregion

        #region Equality, hashing, comparison

        [TestMethod]
        public void Equals_TreatsNaNAsEqual_Unlike_Operator()
        {
            Vector2d.NaN.Equals(Vector2d.NaN).Should().BeTrue();
            new Vector2d(1, 2).Equals(new Vector2d(1.0000001, 2), 1e-3).Should().BeTrue();
            new Vector2d(1, 2).ApproximatelyEquals(new Vector2d(1.0000001, 2), 1e-3).Should().BeTrue();
        }

        [TestMethod]
        public void GetHashCode_IsStableForEqualVectors()
        {
            new Vector2d(1, 2).GetHashCode().Should().Be(new Vector2d(1, 2).GetHashCode());
        }

        [TestMethod]
        public void CompareTo_ByMagnitude_WithToleranceAndInfinity()
        {
            new Vector2d(1, 0).CompareTo(new Vector2d(0, 2)).Should().Be(-1);
            new Vector2d(3, 0).CompareTo(new Vector2d(0, 2)).Should().Be(1);

            // Two infinite-magnitude vectors compare equal under the tolerant overload.
            var infA = new Vector2d(double.PositiveInfinity, 0);
            var infB = new Vector2d(0, double.PositiveInfinity);
            infA.CompareTo(infB, 1e-6).Should().Be(0);
        }

        [TestMethod]
        public void CompareTo_NonVector_Throws()
        {
            Action act = () => new Vector2d(1, 2).CompareTo("not a vector");
            act.Should().Throw<ArgumentException>();
        }

        #endregion

        #region Decisions and constants

        [TestMethod]
        public void Decisions()
        {
            Vector2d.XAxis.IsUnitVector().Should().BeTrue();
            new Vector2d(1, 1).IsUnitVector(0.1).Should().BeFalse(); // |√2 − 1| ≈ 0.414 > 0.1
            Vector2d.XAxis.IsPerpendicular(Vector2d.YAxis).Should().BeTrue();
            Vector2d.NaN.IsNaN().Should().BeTrue();
            Vector2d.Origin.IsZero().Should().BeTrue();
            new Vector2d(1e-9, 0).IsZero(1e-6).Should().BeTrue();
        }

        [TestMethod]
        public void Constants_AreCorrect()
        {
            Vector2d.Zero.Should().Be(Vector2d.Origin);
            Vector2d.One.Should().Be(new Vector2d(1, 1));
            Vector2d.UnitX.Should().Be(Vector2d.XAxis);
            Vector2d.UnitY.Should().Be(Vector2d.YAxis);
        }

        #endregion
    }
}
