namespace RP.Math.Tests.Core
{
    using System;


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
            Assert.AreEqual(1, v.X);
            Assert.AreEqual(4, v.W);
            Assert.AreEqual(5, v.Magnitude, Tol);
            Assert.AreEqual(2, v[2]);
            Assert.AreEqual(4, v[3]);
            CollectionAssert.AreEqual(new[] { 1.0, 2.0, 2.0, 4.0 }, v.Array);
            Assert.AreEqual(new Vector3d(1, 2, 2), v.XYZ);
        }

        [TestMethod]
        public void Constructor_FromVector3dAndW()
        {
            Assert.AreEqual(new Vector4d(1, 2, 3, 4), new Vector4d(new Vector3d(1, 2, 3), 4));
        }

        [TestMethod]
        public void Indexer_OutOfRange_Throws()
        {
            var v = new Vector4d(1, 2, 3, 4);
            Action act = () => { var _ = v[4]; };
            Assert.ThrowsException<ArgumentException>(act);
        }

        [TestMethod]
        public void Array_Constructor_WrongLength_Throws()
        {
            Action act = () => new Vector4d(new[] { 1.0, 2.0, 3.0 });
            Assert.ThrowsException<ArgumentException>(act);
        }

        #endregion

        #region Operators and products

        [TestMethod]
        public void Arithmetic_And_Dot()
        {
            var a = new Vector4d(1, 2, 3, 4);
            var b = new Vector4d(4, 3, 2, 1);

            Assert.AreEqual(new Vector4d(5, 5, 5, 5), (a + b));
            Assert.AreEqual(new Vector4d(3, 1, -1, -3), (b - a));
            Assert.AreEqual(new Vector4d(2, 4, 6, 8), (a * 2));
            Assert.AreEqual(new Vector4d(4, 6, 6, 4), (a * b)); // component-wise
            Assert.AreEqual(4 + 6 + 6 + 4, Vector4d.DotProduct(a, b));
            Assert.AreEqual(20, a.Dot(b)); // alias
        }

        [TestMethod]
        public void Comparison_Operators_AreByMagnitude()
        {
            Assert.IsTrue((new Vector4d(1, 0, 0, 0) < new Vector4d(0, 0, 0, 2)));
            Assert.IsTrue((new Vector4d(3, 0, 0, 0) >= new Vector4d(0, 0, 0, 3)));
        }

        [TestMethod]
        public void EqualityOperator_IsExact_And_NaNIsNotEqualToItself()
        {
            Assert.IsTrue((new Vector4d(1, 2, 3, 4) == new Vector4d(1, 2, 3, 4)));
            Assert.IsFalse((Vector4d.NaN == Vector4d.NaN));
        }

        #endregion

        #region Normalisation edge cases

        [TestMethod]
        public void Normalize_ProducesUnitVector()
        {
            var n = new Vector4d(1, 2, 2, 4).Normalize();
            Assert.AreEqual(1, n.Magnitude, Tol);
            Assert.IsTrue(n.Equals(new Vector4d(0.2, 0.4, 0.4, 0.8), Tol));
        }

        [TestMethod]
        public void Normalize_Zero_Throws()
        {
            Action act = () => Vector4d.Origin.Normalize();
            Assert.ThrowsException<NormalizeVectorException>(act);
        }

        [TestMethod]
        public void NormalizeOrDefault_Zero_ReturnsOrigin_NaN_ReturnsNaN()
        {
            Assert.AreEqual(Vector4d.Origin, Vector4d.Origin.NormalizeOrDefault());
            Assert.IsTrue(Vector4d.NaN.NormalizeOrDefault().IsNaN());
        }

        [TestMethod]
        public void Normalize_AxisAlignedInfinity_IsASpecialCase()
        {
            var n = new Vector4d(0, double.PositiveInfinity, 0, 0).NormalizeOrDefault();
            Assert.IsTrue(n.Equals(Vector4d.YAxis, Tol));
        }

        [TestMethod]
        public void Normalize_HalfInfiniteVector_Throws()
        {
            Action act = () => new Vector4d(double.PositiveInfinity, 5, 0, 0).Normalize();
            Assert.ThrowsException<NormalizeVectorException>(act);
        }

        #endregion

        #region Interpolation

        [TestMethod]
        public void Interpolate_Midpoint_And_RangeCheck()
        {
            Assert.AreEqual(new Vector4d(1, 2, 3, 4), Vector4d.Interpolate(Vector4d.Origin, new Vector4d(2, 4, 6, 8), 0.5));

            Action act = () => Vector4d.Interpolate(Vector4d.Origin, Vector4d.One, -0.5);
            Assert.ThrowsException<ArgumentOutOfRangeException>(act);

            Assert.AreEqual(new Vector4d(4, 4, 4, 4), Vector4d.Lerp(Vector4d.Origin, new Vector4d(2, 2, 2, 2), 2)); // extrapolation
        }

        [TestMethod]
        public void Slerp_QuarterTurn_StaysUnit()
        {
            var mid = Vector4d.Slerp(Vector4d.XAxis, Vector4d.YAxis, 0.5);
            Assert.AreEqual(1, mid.Magnitude, Tol);
        }

        [TestMethod]
        public void Slerp_Antiparallel_FallsBackToLerp_NoNaN()
        {
            Assert.IsFalse(Vector4d.Slerp(Vector4d.XAxis, -Vector4d.XAxis, 0.5).IsNaN());
        }

        #endregion

        #region Angle (Kahan)

        [TestMethod]
        public void Angle_KnownCases_IncludingAntiparallel()
        {
            Assert.AreEqual(Math.PI / 2, Vector4d.Angle(Vector4d.XAxis, Vector4d.YAxis), Tol);
            Assert.AreEqual(0, Vector4d.Angle(Vector4d.XAxis, Vector4d.XAxis));
            // The Kahan form must return exactly π (no NaN) for opposite vectors.
            Assert.AreEqual(Math.PI, Vector4d.Angle(Vector4d.XAxis, -Vector4d.XAxis), Tol);
        }

        [TestMethod]
        public void Angle_NearlyParallel_IsAccurate()
        {
            // A tiny angle where the acos(dot) form would lose all precision; Kahan keeps it.
            var a = new Vector4d(1, 0, 0, 0);
            var b = new Vector4d(1, 1e-7, 0, 0);
            Assert.AreEqual(1e-7, Vector4d.Angle(a, b), 1e-12);
        }

        #endregion

        #region Projection, rejection, reflection

        [TestMethod]
        public void Projection_Plus_Rejection_Reconstructs_Original()
        {
            var v = new Vector4d(1, 2, 3, 4);
            var dir = new Vector4d(0, 1, 0, 0);
            Assert.AreEqual(new Vector4d(0, 2, 0, 0), v.Projection(dir));
            Assert.IsTrue((v.Projection(dir) + v.Rejection(dir)).Equals(v, Tol));
        }

        [TestMethod]
        public void Reflect_AboutNormal_FlipsTheNormalComponent()
        {
            Assert.IsTrue(new Vector4d(1, -1, 0, 0).Reflect(Vector4d.YAxis)
                .Equals(new Vector4d(1, 1, 0, 0), Tol));
        }

        #endregion

        #region Component-wise, clamp, move, rounding

        [TestMethod]
        public void ComponentMinMax_Clamp_ClampMagnitude_MoveTowards()
        {
            var a = new Vector4d(1, 5, 1, 5);
            var b = new Vector4d(4, 2, 4, 2);
            Assert.AreEqual(new Vector4d(1, 2, 1, 2), Vector4d.ComponentMin(a, b));
            Assert.AreEqual(new Vector4d(4, 5, 4, 5), Vector4d.ComponentMax(a, b));

            Assert.AreEqual(1, new Vector4d(2, 0, 0, 0).ClampMagnitude(1).Magnitude, Tol);
            Assert.AreEqual(new Vector4d(4, 0, 0, 0), Vector4d.MoveTowards(Vector4d.Origin, new Vector4d(10, 0, 0, 0), 4));
        }

        [TestMethod]
        public void Round_MidpointModes()
        {
            var v = new Vector4d(0.5, 1.5, 2.5, 3.5);
            Assert.AreEqual(new Vector4d(0, 2, 2, 4), Vector4d.Round(v, MidpointRounding.ToEven));
            Assert.AreEqual(new Vector4d(1, 2, 3, 4), Vector4d.Round(v, MidpointRounding.AwayFromZero));
        }

        #endregion

        #region Homogeneous coordinates

        [TestMethod]
        public void Dehomogenize_DoesThePerspectiveDivide()
        {
            Assert.AreEqual(new Vector3d(1, 2, 3), new Vector4d(2, 4, 6, 2).Dehomogenize());
        }

        [TestMethod]
        public void Dehomogenize_Direction_GivesInfinity()
        {
            // w == 0 is a point at infinity (a pure direction).
            var d = new Vector4d(1, 0, 0, 0).Dehomogenize();
            Assert.IsTrue(double.IsInfinity(d.X));
        }

        #endregion

        #region Conversions and equality

        [TestMethod]
        public void Conversions_WithFloatSibling()
        {
            Vector4d widened = new Vector4(1.5f, 2.5f, 3.5f, 4.5f); // implicit
            Assert.AreEqual(new Vector4d(1.5, 2.5, 3.5, 4.5), widened);

            var narrowed = (Vector4)new Vector4d(1.5, 2.5, 3.5, 4.5); // explicit
            Assert.AreEqual(new Vector4(1.5f, 2.5f, 3.5f, 4.5f), narrowed);

            var (x, y, z, w) = new Vector4d(5, 6, 7, 8);
            Assert.AreEqual((5.0, 6.0, 7.0, 8.0), (x, y, z, w));
        }

        [TestMethod]
        public void Equals_NaN_And_Tolerance_And_Hash()
        {
            Assert.IsTrue(Vector4d.NaN.Equals(Vector4d.NaN));
            Assert.IsTrue(new Vector4d(1, 2, 3, 4).Equals(new Vector4d(1, 2, 3, 4.0000001), 1e-3));
            Assert.AreEqual(new Vector4d(1, 2, 3, 4).GetHashCode(), new Vector4d(1, 2, 3, 4).GetHashCode());
        }

        [TestMethod]
        public void CompareTo_Tolerance_BothInfinite_IsZero()
        {
            var infA = new Vector4d(double.PositiveInfinity, 0, 0, 0);
            var infB = new Vector4d(0, 0, 0, double.PositiveInfinity);
            Assert.AreEqual(0, infA.CompareTo(infB, 1e-6));
        }

        #endregion

        #region Decisions and constants

        [TestMethod]
        public void Decisions_And_Constants()
        {
            Assert.IsTrue(Vector4d.WAxis.IsUnitVector());
            Assert.IsTrue(Vector4d.XAxis.IsPerpendicular(Vector4d.YAxis));
            Assert.IsTrue(Vector4d.NaN.IsNaN());
            Assert.IsTrue(Vector4d.Origin.IsZero());

            Assert.AreEqual(Vector4d.Origin, Vector4d.Zero);
            Assert.AreEqual(new Vector4d(1, 1, 1, 1), Vector4d.One);
            Assert.AreEqual(Vector4d.WAxis, Vector4d.UnitW);
        }

        #endregion
    }
}
