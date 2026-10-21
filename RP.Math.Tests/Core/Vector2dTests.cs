namespace RP.Math.Tests.Core
{
    using System;


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
            Assert.AreEqual(3, v.X);
            Assert.AreEqual(-4, v.Y);
            Assert.AreEqual(5, v.Magnitude, Tol);
            Assert.AreEqual(25, v.MagnitudeSquared, Tol);
            Assert.AreEqual(v.Magnitude, v.Length);
            Assert.AreEqual(v.MagnitudeSquared, v.LengthSquared);
            Assert.AreEqual(3, v[0]);
            Assert.AreEqual(-4, v[1]);
            CollectionAssert.AreEqual(new[] { 3.0, -4.0 }, v.Array);
        }

        [TestMethod]
        public void Indexer_OutOfRange_Throws()
        {
            var v = new Vector2d(1, 2);
            Action act = () => { var _ = v[2]; };
            Assert.ThrowsException<ArgumentException>(act);
        }

        [TestMethod]
        public void Array_Constructor_WrongLength_Throws()
        {
            Action act = () => new Vector2d(new[] { 1.0, 2.0, 3.0 });
            Assert.ThrowsException<ArgumentException>(act);
        }

        #endregion

        #region Operators

        [TestMethod]
        public void Arithmetic_Operators()
        {
            var a = new Vector2d(1, 2);
            var b = new Vector2d(3, 4);

            Assert.AreEqual(new Vector2d(4, 6), (a + b));
            Assert.AreEqual(new Vector2d(2, 2), (b - a));
            Assert.AreEqual(new Vector2d(2, 4), (a * 2));
            Assert.AreEqual(new Vector2d(2, 4), (2 * a));
            Assert.AreEqual(new Vector2d(3, 8), (a * b)); // component-wise
            Assert.AreEqual(new Vector2d(1.5, 2), (b / 2));
            Assert.AreEqual(new Vector2d(-1, -2), (-a));
            Assert.AreEqual(a, (+a));
        }

        [TestMethod]
        public void Comparison_Operators_AreByMagnitude()
        {
            var small = new Vector2d(1, 0);
            var big = new Vector2d(0, 3);

            Assert.IsTrue((small < big));
            Assert.IsTrue((big > small));
            Assert.IsTrue((small <= new Vector2d(0, 1))); // equal magnitude
            Assert.IsTrue((small >= new Vector2d(0, 1)));
        }

        [TestMethod]
        public void EqualityOperator_IsExact_And_NaNIsNotEqualToItself()
        {
            Assert.IsTrue((new Vector2d(1, 2) == new Vector2d(1, 2)));
            Assert.IsTrue((new Vector2d(1, 2) != new Vector2d(1, 3)));

            // Per IEEE-754, NaN != NaN — the == operator must reflect that even though Equals() does not.
            Assert.IsFalse((Vector2d.NaN == Vector2d.NaN));
        }

        #endregion

        #region Products

        [TestMethod]
        public void DotProduct_And_Alias_Agree()
        {
            var a = new Vector2d(1, 2);
            var b = new Vector2d(3, 4);

            Assert.AreEqual(11, Vector2d.DotProduct(a, b));
            Assert.AreEqual(11, a.DotProduct(b));
            Assert.AreEqual(11, Vector2d.Dot(a, b));
            Assert.AreEqual(11, a.Dot(b));
        }

        [TestMethod]
        public void CrossProduct_IsSignedArea_PositiveCounterClockwise()
        {
            // X cross Y is +1 (CCW); Y cross X is -1 (CW); parallel is 0.
            Assert.AreEqual(1, Vector2d.CrossProduct(Vector2d.XAxis, Vector2d.YAxis));
            Assert.AreEqual(-1, Vector2d.CrossProduct(Vector2d.YAxis, Vector2d.XAxis));
            Assert.AreEqual(0, Vector2d.CrossProduct(new Vector2d(2, 2), new Vector2d(4, 4)));
            Assert.AreEqual(1, Vector2d.Cross(Vector2d.XAxis, Vector2d.YAxis)); // alias
        }

        [TestMethod]
        public void Perpendicular_IsOrthogonal_And_RightHanded()
        {
            var v = new Vector2d(3, 1);
            Assert.AreEqual(new Vector2d(-1, 3), v.Perpendicular());      // 90° CCW
            Assert.AreEqual(new Vector2d(1, -3), v.PerpendicularCW());    // 90° CW
            Assert.AreEqual(0, v.DotProduct(v.Perpendicular()), Tol);
        }

        #endregion

        #region Normalisation edge cases

        [TestMethod]
        public void Normalize_ProducesUnitVector()
        {
            var n = new Vector2d(3, 4).Normalize();
            Assert.AreEqual(1, n.Magnitude, Tol);
            Assert.IsTrue(n.Equals(new Vector2d(0.6, 0.8), Tol));
        }

        [TestMethod]
        public void Normalize_Zero_Throws()
        {
            Action act = () => Vector2d.Origin.Normalize();
            Assert.ThrowsException<NormalizeVectorException>(act);
        }

        [TestMethod]
        public void Normalize_NaN_Throws()
        {
            Action act = () => Vector2d.NaN.Normalize();
            Assert.ThrowsException<NormalizeVectorException>(act);
        }

        [TestMethod]
        public void NormalizeOrDefault_Zero_ReturnsOrigin_NaN_ReturnsNaN()
        {
            Assert.AreEqual(Vector2d.Origin, Vector2d.Origin.NormalizeOrDefault());
            Assert.IsTrue(Vector2d.NaN.NormalizeOrDefault().IsNaN());
        }

        [TestMethod]
        public void Normalize_AxisAlignedInfinity_IsASpecialCase()
        {
            // (+inf, 0) has a well-defined direction: the +X axis.
            var n = new Vector2d(double.PositiveInfinity, 0).NormalizeOrDefault();
            Assert.IsTrue(n.Equals(Vector2d.XAxis, Tol));
        }

        [TestMethod]
        public void Normalize_HalfInfiniteVector_Throws()
        {
            // (+inf, 5): the finite component vanishes against the infinite one — direction is undefined.
            Action act = () => new Vector2d(double.PositiveInfinity, 5).Normalize();
            Assert.ThrowsException<NormalizeVectorException>(act);
        }

        #endregion

        #region Interpolation

        [TestMethod]
        public void Interpolate_Midpoint()
        {
            Assert.AreEqual(new Vector2d(5, 10), Vector2d.Interpolate(new Vector2d(0, 0), new Vector2d(10, 20), 0.5));
        }

        [TestMethod]
        public void Interpolate_OutOfRange_Throws_UnlessExtrapolationAllowed()
        {
            Action act = () => Vector2d.Interpolate(Vector2d.Origin, Vector2d.One, 1.5);
            Assert.ThrowsException<ArgumentOutOfRangeException>(act);

            Assert.AreEqual(new Vector2d(3, 3), Vector2d.Interpolate(Vector2d.Origin, new Vector2d(2, 2), 1.5, true));
            Assert.AreEqual(new Vector2d(3, 3), Vector2d.Lerp(Vector2d.Origin, new Vector2d(2, 2), 1.5));
        }

        [TestMethod]
        public void Slerp_QuarterTurn_TracksTheArc()
        {
            var mid = Vector2d.Slerp(Vector2d.XAxis, Vector2d.YAxis, 0.5);
            Assert.AreEqual(1, mid.Magnitude, Tol);
            var expected = new Vector2d(Math.Cos(Math.PI / 4), Math.Sin(Math.PI / 4));
            Assert.IsTrue(mid.Equals(expected, 1e-9));
        }

        [TestMethod]
        public void Slerp_Antiparallel_FallsBackToLerp()
        {
            // sinθ ≈ 0, so the method must not divide by zero — it linearly interpolates instead.
            var mid = Vector2d.Slerp(Vector2d.XAxis, -Vector2d.XAxis, 0.5);
            Assert.IsFalse(mid.IsNaN());
        }

        #endregion

        #region Angles

        [TestMethod]
        public void Angle_KnownCases()
        {
            Assert.AreEqual(Math.PI / 2, Vector2d.Angle(Vector2d.XAxis, Vector2d.YAxis), Tol);
            Assert.AreEqual(0, Vector2d.Angle(Vector2d.XAxis, Vector2d.XAxis));
            // Antiparallel must be exactly π, not NaN (the historic acos bug).
            Assert.AreEqual(Math.PI, Vector2d.Angle(Vector2d.XAxis, -Vector2d.XAxis), Tol);
        }

        [TestMethod]
        public void SignedAngle_IsOrientationAware()
        {
            Assert.AreEqual(Math.PI / 2, Vector2d.SignedAngle(Vector2d.XAxis, Vector2d.YAxis), Tol);
            Assert.AreEqual(-Math.PI / 2, Vector2d.SignedAngle(Vector2d.YAxis, Vector2d.XAxis), Tol);
        }

        #endregion

        #region Rotation

        [TestMethod]
        public void Rotate_QuarterTurn_MapsXToY()
        {
            Assert.IsTrue(new Vector2d(1, 0).Rotate(Math.PI / 2).Equals(Vector2d.YAxis, 1e-12));
            Assert.IsTrue(new Vector2d(1, 0).Rotate(new Angle(Math.PI / 2)).Equals(Vector2d.YAxis, 1e-12));
        }

        #endregion

        #region Projection, rejection, reflection

        [TestMethod]
        public void Projection_Plus_Rejection_Reconstructs_Original()
        {
            var v = new Vector2d(3, 4);
            var dir = new Vector2d(1, 1);
            Assert.IsTrue((v.Projection(dir) + v.Rejection(dir)).Equals(v, Tol));
            Assert.AreEqual(0, v.Rejection(dir).DotProduct(dir), Tol);
            Assert.AreEqual(v.Projection(dir), v.Project(dir)); // aliases
            Assert.AreEqual(v.Rejection(dir), v.Reject(dir));
        }

        [TestMethod]
        public void Reflect_AboutNormal_IsTheBounce()
        {
            // Travelling down-right, bouncing off the floor (normal +Y) flips the Y component.
            Assert.IsTrue(new Vector2d(1, -1).Reflect(Vector2d.YAxis).Equals(new Vector2d(1, 1), Tol));
        }

        [TestMethod]
        public void Reflection_AboutVector_PreservesMagnitude()
        {
            var v = new Vector2d(3, 4);
            var mirror = new Vector2d(1, 0);
            var r = v.Reflection(mirror);
            Assert.AreEqual(v.Magnitude, r.Magnitude, 1e-9);
            // Mirroring (3,4) about the X axis gives (3,-4).
            Assert.IsTrue(r.Equals(new Vector2d(3, -4), 1e-9));
        }

        #endregion

        #region Component-wise, clamp, move

        [TestMethod]
        public void ComponentMinMax_And_Clamp()
        {
            var a = new Vector2d(1, 5);
            var b = new Vector2d(4, 2);
            Assert.AreEqual(new Vector2d(1, 2), Vector2d.ComponentMin(a, b));
            Assert.AreEqual(new Vector2d(4, 5), Vector2d.ComponentMax(a, b));
            Assert.AreEqual(new Vector2d(3, 0), new Vector2d(5, -5).Clamp(Vector2d.Origin, new Vector2d(3, 3)));
        }

        [TestMethod]
        public void ClampMagnitude_CapsLength()
        {
            var v = new Vector2d(3, 4); // length 5
            Assert.AreEqual(2.5, v.ClampMagnitude(2.5).Magnitude, Tol);
            Assert.AreEqual(v, v.ClampMagnitude(10)); // already shorter
        }

        [TestMethod]
        public void MoveTowards_NeverOvershoots()
        {
            var step = Vector2d.MoveTowards(Vector2d.Origin, new Vector2d(10, 0), 3);
            Assert.AreEqual(new Vector2d(3, 0), step);
            Assert.AreEqual(new Vector2d(2, 0), Vector2d.MoveTowards(Vector2d.Origin, new Vector2d(2, 0), 5)); // arrives, no overshoot
        }

        [TestMethod]
        public void ComponentMaths()
        {
            var v = new Vector2d(-3, 4);
            Assert.AreEqual(1, v.SumComponents());
            Assert.AreEqual(25, v.SumComponentSqrs());
            Assert.AreEqual(new Vector2d(3, 4), v.AbsComponents());
            Assert.AreEqual(new Vector2d(3, 4), v.Abs());
            Assert.AreEqual(new Vector2d(4, 9), new Vector2d(2, 3).SqrComponents());
            Assert.AreEqual(new Vector2d(2, 3), new Vector2d(4, 9).SqrtComponents());
            Assert.AreEqual(new Vector2d(4, 9), new Vector2d(2, 3).PowComponents(2));
        }

        #endregion

        #region Rounding

        [TestMethod]
        public void Round_MidpointModes_Differ()
        {
            var v = new Vector2d(2.5, 3.5);
            Assert.AreEqual(new Vector2d(2, 4), Vector2d.Round(v, MidpointRounding.ToEven));
            Assert.AreEqual(new Vector2d(3, 4), Vector2d.Round(v, MidpointRounding.AwayFromZero));
            Assert.AreEqual(new Vector2d(1.23, 6.79), new Vector2d(1.2345, 6.789).Round(2));
        }

        #endregion

        #region Conversions

        [TestMethod]
        public void Conversions_WithFloatSibling_And_To3d()
        {
            Vector2d widened = new Vector2(1.5f, 2.5f); // implicit widening
            Assert.AreEqual(new Vector2d(1.5, 2.5), widened);

            var narrowed = (Vector2)new Vector2d(1.5, 2.5); // explicit narrowing
            Assert.AreEqual(new Vector2(1.5f, 2.5f), narrowed);

            Assert.AreEqual(new Vector3d(1, 2, 3), new Vector2d(1, 2).ToVector3d(3));

            var (x, y) = new Vector2d(7, 8);
            Assert.AreEqual(7, x);
            Assert.AreEqual(8, y);
        }

        #endregion

        #region Equality, hashing, comparison

        [TestMethod]
        public void Equals_TreatsNaNAsEqual_Unlike_Operator()
        {
            Assert.IsTrue(Vector2d.NaN.Equals(Vector2d.NaN));
            Assert.IsTrue(new Vector2d(1, 2).Equals(new Vector2d(1.0000001, 2), 1e-3));
            Assert.IsTrue(new Vector2d(1, 2).ApproximatelyEquals(new Vector2d(1.0000001, 2), 1e-3));
        }

        [TestMethod]
        public void GetHashCode_IsStableForEqualVectors()
        {
            Assert.AreEqual(new Vector2d(1, 2).GetHashCode(), new Vector2d(1, 2).GetHashCode());
        }

        [TestMethod]
        public void CompareTo_ByMagnitude_WithToleranceAndInfinity()
        {
            Assert.AreEqual(-1, new Vector2d(1, 0).CompareTo(new Vector2d(0, 2)));
            Assert.AreEqual(1, new Vector2d(3, 0).CompareTo(new Vector2d(0, 2)));

            // Two infinite-magnitude vectors compare equal under the tolerant overload.
            var infA = new Vector2d(double.PositiveInfinity, 0);
            var infB = new Vector2d(0, double.PositiveInfinity);
            Assert.AreEqual(0, infA.CompareTo(infB, 1e-6));
        }

        [TestMethod]
        public void CompareTo_NonVector_Throws()
        {
            Action act = () => new Vector2d(1, 2).CompareTo("not a vector");
            Assert.ThrowsException<ArgumentException>(act);
        }

        #endregion

        #region Decisions and constants

        [TestMethod]
        public void Decisions()
        {
            Assert.IsTrue(Vector2d.XAxis.IsUnitVector());
            Assert.IsFalse(new Vector2d(1, 1).IsUnitVector(0.1)); // |√2 − 1| ≈ 0.414 > 0.1
            Assert.IsTrue(Vector2d.XAxis.IsPerpendicular(Vector2d.YAxis));
            Assert.IsTrue(Vector2d.NaN.IsNaN());
            Assert.IsTrue(Vector2d.Origin.IsZero());
            Assert.IsTrue(new Vector2d(1e-9, 0).IsZero(1e-6));
        }

        [TestMethod]
        public void Constants_AreCorrect()
        {
            Assert.AreEqual(Vector2d.Origin, Vector2d.Zero);
            Assert.AreEqual(new Vector2d(1, 1), Vector2d.One);
            Assert.AreEqual(Vector2d.XAxis, Vector2d.UnitX);
            Assert.AreEqual(Vector2d.YAxis, Vector2d.UnitY);
        }

        #endregion
    }
}
