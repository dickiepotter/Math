namespace RP.Math.Tests
{
    using System;
    using System.Diagnostics.CodeAnalysis;


    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using RP.Math;
    using RP.Math.Exceptions;

    /// <summary>
    /// Unit tests for the <see cref="Vector"/> class
    /// </summary>
    /// TODO Test What happens when we try to get the angle of vectors with (inf, n, n) and NaN components. We may need to add AnngleOrDefault but that seems wrong when getting scalar results. Should be NaN.
    /// TODO Rethink fix IsPerpendicular special case when near zero or infinty components.
    /// TODO Test the logic of Abs
    [TestClass]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public partial class VectorTests
    {
        private const double ArbitaryTestDouble = 109.005;

        private static readonly double Deg90AsRad = System.Math.PI / 2;

        #region Constructor tests

        [TestMethod, TestCategory("Construction")]
        public void Construction_WithXYZParameters_ShouldConstructCorrectly_Test()
        {
            var vector = new Vector(100, -100, 0.01);
            Assert.AreEqual(100d, vector.X);
            Assert.AreEqual(-100d, vector.Y);
            Assert.AreEqual(0.01d, vector.Z);
        }

        [TestMethod, TestCategory("Construction")]
        public void Construction_WithAVector3Parameter_ShouldConstructCorrectly_Test()
        {
            var vector = new Vector(new Vector(100, -100, 0));

            Assert.AreEqual(100, vector.X);
            Assert.AreEqual(-100, vector.Y);
            Assert.AreEqual(0, vector.Z);
        }

        [TestMethod, TestCategory("Construction")]
        public void Constructor_WithAnArrayOfXYZParameter_ShouldConstructCorrectly_Test()
        {
            var vector = new Vector(new[] { 100, -100, 0.01 });

            Assert.AreEqual(100, vector.X);
            Assert.AreEqual(-100, vector.Y);
            Assert.AreEqual(0.01, vector.Z);
        }

        [TestMethod, TestCategory("Construction")]
        public void Constructor_WithNoParameters_ShouldConstructCorrectly_Test()
        {
            var vector = new Vector();

            Assert.AreEqual(0, vector.X);
            Assert.AreEqual(0, vector.Y);
            Assert.AreEqual(0, vector.Z);
        }

        #endregion

        #region Abs Tests

        [TestMethod, TestCategory("Abs")]
        public void Abs_WhereVectorIsZero_Test()
        {
            var vector = new Vector(0, 0, 0);
            var result = vector.Abs();
            Assert.AreEqual(0, result);
        }

        #endregion

        #region Square, Square-Root and Pow Tests

        [TestMethod]
        public void SquareComponentsTest()
        {
            var vector = new Vector(2, 3, 4);
            var result = vector.SqrComponents();

            Assert.AreEqual(4, result.X);
            Assert.AreEqual(9, result.Y);
            Assert.AreEqual(16, result.Z);
        }

        [TestMethod]
        public void SquareRootComponentsTest()
        {
            var vector = new Vector(4, 9, 16);
            var result = vector.SqrtComponents();

            Assert.AreEqual(2, result.X);
            Assert.AreEqual(3, result.Y);
            Assert.AreEqual(4, result.Z);
        }

        [TestMethod]
        public void PowComponentsTest()
        {
            var vector = new Vector(2, 1, 3);
            var result = vector.PowComponents(2);

            Assert.AreEqual(4, result.X);
            Assert.AreEqual(1, result.Y);
            Assert.AreEqual(9, result.Z);
        }

        #endregion

        #region Add and Subtract tests

        [TestMethod]
        public void AddUsingWholeNumbersTest()
        {
            var a = new Vector(3, 7, 4);
            var b = new Vector(2, 9, 11);
            var result = a + b;

            Assert.AreEqual(5, result.X);
            Assert.AreEqual(16, result.Y);
            Assert.AreEqual(15, result.Z);
        }

        [TestMethod]
        public void SubtractUingWholeNumbersTest()
        {
            var a = new Vector(1, 2, 3);
            var b = new Vector(3, 3, 3);
            var result = a - b;

            Assert.AreEqual(-2, result.X);
            Assert.AreEqual(-1, result.Y);
            Assert.AreEqual(-0, result.Z);
        }

        #endregion

        #region Cross and dot product tests

        [TestMethod, TestCategory("Product")]
        public void DotProduct_WhereWholeNumbers_ShouldResultInTheCorrectDotProduct_Test()
        {
            var a = new Vector(12, 20, 0);
            var b = new Vector(16, -5, 0);

            Assert.AreEqual(92, a.DotProduct(b));
        }

        [TestMethod, TestCategory("Product")]
        public void DotProduct_WhereV1XIsPositiveInfinity_ShouldResultInPositiveInfinity_Test()
        {
            var v1 = new Vector(double.PositiveInfinity, 0, 0);
            var v2 = new Vector(1, 0, 0);

            var result = v1.DotProduct(v2);

            Assert.AreEqual(double.PositiveInfinity, result);
        }

        [TestMethod, TestCategory("Product")]
        public void DotProduct_WhereV2XIsPositiveInfinity_ShouldResultInPositiveInfinity_Test()
        {
            var v1 = new Vector(1, 0, 0);
            var v2 = new Vector(double.PositiveInfinity, 0, 0);

            var result = v1.DotProduct(v2);

            Assert.AreEqual(double.PositiveInfinity, result);
        }

        [TestMethod, TestCategory("Product")]
        public void DotProduct_WhereXIsPositiveInfinity_ShouldResultInPositiveInfinity_Test()
        {
            var v1 = new Vector(double.PositiveInfinity, 0, 0);
            var v2 = new Vector(double.PositiveInfinity, 0, 0);

            var result = v1.DotProduct(v2);

            Assert.AreEqual(double.PositiveInfinity, result);
        }

        [TestMethod, TestCategory("Product")]
        public void DotProduct_WhereV1XIsNegativeInfinity_ShouldResultInNegativeInfinity_Test()
        {
            var v1 = new Vector(double.NegativeInfinity, 0, 0);
            var v2 = new Vector(1, 0, 0);

            var result = v1.DotProduct(v2);

            Assert.AreEqual(double.NegativeInfinity, result);
        }

        [TestMethod, TestCategory("Product")]
        public void DotProduct_WhereV2XIsNegativeInfinity_ShouldResultInNegativeInfinity_Test()
        {
            var v1 = new Vector(1, 0, 0);
            var v2 = new Vector(double.NegativeInfinity, 0, 0);

            var result = v1.DotProduct(v2);

            Assert.AreEqual(double.NegativeInfinity, result);
        }

        [TestMethod, TestCategory("Product")]
        public void DotProduct_WhereXIsNegativeInfinity_ShouldResultInPositiveInfinity_Test()
        {
            var v1 = new Vector(double.NegativeInfinity, 0, 0);
            var v2 = new Vector(double.NegativeInfinity, 0, 0);

            var result = v1.DotProduct(v2);

            Assert.AreEqual(double.PositiveInfinity, result);
        }

        [TestMethod, TestCategory("Product")]
        public void DotProduct_WhereV1XIsNaN_ShouldResultInNaN_Test()
        {
            var v1 = new Vector(double.NaN, 0, 0);
            var v2 = new Vector(1, 0, 0);

            var result = v1.DotProduct(v2);

            Assert.AreEqual(double.NaN, result);
        }

        [TestMethod, TestCategory("Product")]
        public void DotProduct_WhereV1XIs10AndV2IsUnitVectorXAxis_ShouldResultIn10_Test()
        {
            var v1 = new Vector(10, 0, 0);
            var v2 = new Vector(1, 0, 0);

            var result = v1.DotProduct(v2);

            Assert.AreEqual(10, result);
        }

        [TestMethod, TestCategory("Product")]
        public void DotProduct_WhereV1XIsNegative10AndV2IsNegativeUnitVectorXAxis_ShouldResultInPositive10_Test()
        {
            var v1 = new Vector(-10, 0, 0);
            var v2 = new Vector(-1, 0, 0);

            var result = v1.DotProduct(v2);

            Assert.AreEqual(10, result);
        }

        [TestMethod, TestCategory("Product")]
        public void DotProduct_WhereV1XIsNegative10AndV2IsPositiveUnitVectorXAxis_ShouldResultInNegative10_Test()
        {
            var v1 = new Vector(-10, 0, 0);
            var v2 = new Vector(1, 0, 0);

            var result = v1.DotProduct(v2);

            Assert.AreEqual(-10, result);
        }

        [TestMethod, TestCategory("Product")]
        public void CrossProduct_WhereWholeNumbers_ShouldResultInTheCorrectCrossProduct_Test()
        {
            var a = new Vector(4, 1, 0);
            var b = new Vector(-5, 6, 0);
            var result = a.CrossProduct(b);

            Assert.AreEqual(0, result.X);
            Assert.AreEqual(0, result.Y);
            Assert.AreEqual(29, result.Z);
        }

        #endregion

        #region Magnitude tests

        [TestMethod, TestCategory("Magnitude")]
        public void Magnitude_WithPositiveZeroVector_ShouldBe0_Test()
        {
            var vector = new Vector(0, 0, 0);
            var magnitude = vector.Magnitude;

            Assert.AreEqual(0, magnitude);
        }

        [TestMethod, TestCategory("Magnitude")]
        public void Magnitude_WithNegativeZeroVector_ShouldBe0_Test()
        {
            var vector = new Vector(-0, -0, -0);
            var magnitude = vector.Magnitude;

            Assert.AreEqual(0, magnitude);
        }

        [TestMethod, TestCategory("Magnitude")]
        public void Magnitude_WithPositievAndNegativeZeroVector_ShouldBe0_Test()
        {
            var vector = new Vector(-0, +0, -0);
            var magnitude = vector.Magnitude;

            Assert.AreEqual(0, magnitude);
        }

        [TestMethod, TestCategory("Magnitude")]
        public void Magnitude_WithXNaN_ShouldBeNaN_Test()
        {
            var vector = new Vector(double.NaN, 0, 0);
            var magnitude = vector.Magnitude;

            Assert.AreEqual(double.NaN, magnitude);
        }

        [TestMethod, TestCategory("Magnitude")]
        public void Magnitude_WithZNaNAndXYOne_ShouldBeNaN_Test()
        {
            var vector = new Vector(1, 1, double.NaN);
            var magnitude = vector.Magnitude;

            Assert.AreEqual(double.NaN, magnitude);
        }

        [TestMethod, TestCategory("Magnitude")]
        public void Magnitude_WithXPositiveNumber_ShouldBeX_Test()
        {
            var vector = new Vector(10, 0, 0);
            var magnitude = vector.Magnitude;

            Assert.AreEqual(10, magnitude);
        }

        [TestMethod, TestCategory("Magnitude")]
        public void Magnitude_WithPositiveInfinityX_ShouldBePositiveInfinity_Test()
        {
            var vector = new Vector(double.PositiveInfinity, 0, 0);
            var magnitude = vector.Magnitude;

            Assert.AreEqual(double.PositiveInfinity, magnitude);
        }

        [TestMethod, TestCategory("Magnitude")]
        public void Magnitude_WithPositiveInfinityXAndOtherComponensAreWholeNumbers_ShouldBePositiveInfinity_Test()
        {
            var vector = new Vector(double.PositiveInfinity, 3, 6);
            var magnitude = vector.Magnitude;

            Assert.AreEqual(double.PositiveInfinity, magnitude);
        }

        [TestMethod, TestCategory("Magnitude")]
        public void Magnitude_WithNegativeInfinityX_ShouldBePositiveInfinity_Test()
        {
            var vector = new Vector(double.NegativeInfinity, 0, 0);
            var magnitude = vector.Magnitude;

            // Check .Net framework logic for ABS Infinity
            Assert.AreEqual(double.PositiveInfinity, (Math.Abs(double.NegativeInfinity)), "the.Net framework should find ABS Negative Infinity equal to Positive Infinity(if this assumption is wrong then the logic of this test is also wrong");

            Assert.AreEqual(double.PositiveInfinity, magnitude);
        }

        [TestMethod, TestCategory("Magnitude")]
        public void Magnitude_WithNegativeInfinityXAndOtherComponensAreWholeNumbers_ShouldBePositiveInfinity_Test()
        {
            var vector = new Vector(double.NegativeInfinity, 3, 6);
            var magnitude = vector.Magnitude;

            Assert.AreEqual(double.PositiveInfinity, magnitude);
        }

        [TestMethod, TestCategory("Magnitude")]
        public void Magnitude_WithPositiveAndNegativeWholeNumberParameters_ShouldResultInTheCorrectMagnitude_Test()
        {
            var vector = new Vector(3, 1, -1);
            var magnitude = vector.Magnitude;

            Assert.AreEqual(System.Math.Sqrt(11), magnitude);
        }

        [TestMethod, TestCategory("Magnitude")]
        public void Magnitude_WithPositiveWholeNumberParameters_ShouldResultInTheCorrectMagnitude_Test()
        {
            var vector = new Vector(2, 3, 4);
            var magnitude = vector.Magnitude;

            Assert.AreEqual(System.Math.Sqrt(29), magnitude);
        }

        /// <summary>
        /// Test scaling a vector with X component only
        /// </summary>
        [TestMethod, TestCategory("Magnitude")]
        public void Scale_WithXAsOne_ShouldScaleCorrectly_Test()
        {
            var vector = new Vector(1, 0, 0);
            var result = vector.Scale(10);

            Assert.AreEqual(10, result.X);
            Assert.AreEqual(0, result.Y);
            Assert.AreEqual(0, result.Z);
        }

        /// <summary>
        /// Test scaling a vector with Y component only
        /// </summary>
        [TestMethod, TestCategory("Magnitude")]
        public void Scale_WithYAsOne_ShouldScaleCorrectly_Test()
        {
            var vector = new Vector(0, 1, 0);
            var result = vector.Scale(10);

            Assert.AreEqual(0, result.X);
            Assert.AreEqual(10, result.Y);
            Assert.AreEqual(0, result.Z);
        }

        /// <summary>
        /// Test scaling a vector with X and Z components (checking the result to six decimal places)
        /// </summary>
        [TestMethod, TestCategory("Magnitude")]
        public void Scale_WithXZAsOne_ShouldScaleCorrectly_Test()
        {
            var vector = new Vector(1, 0, 1);
            var result = vector.Scale(10);

            var expected = System.Math.Round(5 * System.Math.Sqrt(2), 6);

            Assert.AreEqual(expected, System.Math.Round(result.X, 6));
            Assert.AreEqual(0, result.Y);
            Assert.AreEqual(expected, System.Math.Round(result.Z, 6));
        }

        #endregion

        #region Angle Tests

        [TestMethod, TestCategory("Angle")]
        public void Angel_WithTwoVectorsUsingWholeNumbers_ShouldResultIn90Deg_Test()
        {
            var a = new Vector(1, 0, 0);
            var b = new Vector(0, 1, 0);
            var angle = a.Angle(b);

            Assert.AreEqual(Deg90AsRad, angle, "vector (1,0,0) is perpendicular to vector (0,1,0)");
        }

        [TestMethod, TestCategory("Angle")]
        public void Angle_WhereVectorsInfinitePositiveXY_ShouldResultIn90Deg_Test()
        {
            Vector v1 = new Vector(double.PositiveInfinity, 0, 0);
            Vector v2 = new Vector(0, double.PositiveInfinity, 0);
            var result = v1.Angle(v2);

            Assert.AreEqual(Deg90AsRad, result, "vector (+inf,0,0) is perpendicular to vector (0,+inf,0)");
        }

        [TestMethod, TestCategory("Angle")]
        public void Angle_WhereVectorsAreEqualAndXIsNaN_ShouldResultInNaN_Test()
        {
            Vector v1 = new Vector(double.NaN, 3, 6);
            Vector v2 = new Vector(double.NaN, 3, 6);
            var result = v1.Angle(v2);

            Assert.AreEqual(double.NaN, result, "the angle between vector (+NaN,3,6) and (+NaN,3,6) should be NaN");
        }

        [TestMethod, TestCategory("Angle")]
        public void Angle_WhereVectorsAreNotEqualAndXIsNaN_ShouldResultInNaN_Test()
        {
            Vector v1 = new Vector(double.NaN, 0, 0);
            Vector v2 = new Vector(double.NaN, 1, 2);
            var result = v1.Angle(v2);

            Assert.AreEqual(double.NaN, result, "the angle between vector (+NaN,0,0) and (+NaN,1,2) should be NaN");
        }

        [TestMethod, TestCategory("Angle")]
        public void Angle_WhereVectorsAreEqualAndXIsInfinite_ShouldResultInZero_Test()
        {
            Vector v1 = new Vector(double.PositiveInfinity, 3, 6);
            Vector v2 = new Vector(double.PositiveInfinity, 3, 6);
            var result = v1.Angle(v2);

            Assert.AreEqual(0, result, "vector (+inf,3,6) equal to (+inf,3,6)");
        }

        [TestMethod, TestCategory("Angle")]
        public void Angle_WhereUnitVectorsPositiveYX_ShouldResultIn90Deg_Test()
        {
            Vector v1 = new Vector(0, 1, 0);
            Vector v2 = new Vector(1, 0, 0);
            var result = v1.Angle(v2);

            Assert.AreEqual(Deg90AsRad, result, "vector (0,1,0) is perpendicular to vector (1,0,0)");
        }

        [TestMethod, TestCategory("Angle")]
        public void Angle_WhereVectorsInfinitePositiveYX_ShouldResultIn90Deg_Test()
        {
            Vector v1 = new Vector(0, double.PositiveInfinity, 0);
            Vector v2 = new Vector(double.PositiveInfinity, 0, 0);
            var result = v1.Angle(v2);

            Assert.AreEqual(Deg90AsRad, result, "vector (0,+inf,0) is perpendicular to vector (+inf,0,0)");
        }

        [TestMethod, TestCategory("Angle")]
        public void Angle_WhereUnitVectorsPositiveXNegativeY_ShouldResultIn90Deg_Test()
        {
            Vector v1 = new Vector(1, 0, 0);
            Vector v2 = new Vector(0, -1, 0);
            var result = v1.Angle(v2);

            Assert.AreEqual(Deg90AsRad, result, "vector (1,0,0) is perpendicular to vector (0,-1,0)");
        }

        [TestMethod, TestCategory("Angle")]
        public void Angle_WhereVectorsInfinitePositiveXNegativeY_ShouldResultIn90Deg_Test()
        {
            Vector v1 = new Vector(double.PositiveInfinity, 0, 0);
            Vector v2 = new Vector(0, double.NegativeInfinity, 0);
            var result = v1.Angle(v2);

            Assert.AreEqual(Deg90AsRad, result, "vector (+inf,0,0) is perpendicular to vector (0,-inf,0)");
        }

        [TestMethod, TestCategory("Angle")]
        public void Angle_WhereUnitVectorsNegativeYPositiveX_SShouldResultIn90Deg_Test()
        {
            Vector v1 = new Vector(0, -1, 0);
            Vector v2 = new Vector(1, 0, 0);
            var result = v1.Angle(v2);

            Assert.AreEqual(Deg90AsRad, result, "vector (0,-1,0) is perpendicular to vector (1,0,0)");
        }

        [TestMethod, TestCategory("Angle")]
        public void Angle_WhereVectorsInfiniteNegativeYPositiveX_ShouldResultIn90Deg_Test()
        {
            Vector v1 = new Vector(0, double.NegativeInfinity, 0);
            Vector v2 = new Vector(double.PositiveInfinity, 0, 0);
            var result = v1.Angle(v2);

            Assert.AreEqual(Deg90AsRad, result, "vector (0,-inf,0) is perpendicular to vector (+inf,0,0)");
        }

        [TestMethod, TestCategory("Angle")]
        public void Angle_WhereUnitVectorsNegativeXY_ShouldResultIn90Deg_Test()
        {
            Vector v1 = new Vector(-1, 0, 0);
            Vector v2 = new Vector(0, -1, 0);
            var result = v1.Angle(v2);

            Assert.AreEqual(Deg90AsRad, result, "vector (-1,0,0) is perpendicular to vector (0,-1,0)");
        }

        [TestMethod, TestCategory("Angle")]
        public void Angle_WhereVectorsInfiniteNegativeXY_ShouldResultIn90Deg_Test()
        {
            Vector v1 = new Vector(double.NegativeInfinity, 0, 0);
            Vector v2 = new Vector(0, double.NegativeInfinity, 0);
            var result = v1.Angle(v2);

            Assert.AreEqual(Deg90AsRad, result, "vector (-inf,0,0) is perpendicular to vector (0,-inf,0)");
        }

        [TestMethod, TestCategory("Angle")]
        public void Angle_WhereUnitVectorsNegativeYX_ShouldResultIn90Deg_Test()
        {
            Vector v1 = new Vector(0, -1, 0);
            Vector v2 = new Vector(-1, 0, 0);
            var result = v1.Angle(v2);

            Assert.AreEqual(Deg90AsRad, result, "vector (0,-1,0) is perpendicular to vector (-1,0,0)");
        }

        [TestMethod, TestCategory("Angle")]
        public void Angle_WhereVectorsInfiniteNegativeYX_ShouldResultIn90Deg_Test()
        {
            Vector v1 = new Vector(0, double.NegativeInfinity, 0);
            Vector v2 = new Vector(double.NegativeInfinity, 0, 0);
            var result = v1.Angle(v2);

            Assert.AreEqual(Deg90AsRad, result, "vector (0,-inf,0) is perpendicular to vector (-inf,0,0)");
        }

        [TestMethod, TestCategory("Angle")]
        public void Angle_WhereUnitVectorsNegativeXPositiveY_ShouldResultIn90Deg_Test()
        {
            Vector v1 = new Vector(-1, 0, 0);
            Vector v2 = new Vector(0, 1, 0);
            var result = v1.Angle(v2);

            Assert.AreEqual(Deg90AsRad, result, "vector (-1,0,0) is perpendicular to vector (0,1,0)");
        }

        [TestMethod, TestCategory("Angle")]
        public void Angle_WhereVectorsInfiniteNegativeXPositiveY_ShouldResultIn90Deg_Test()
        {
            Vector v1 = new Vector(double.NegativeInfinity, 0, 0);
            Vector v2 = new Vector(0, double.PositiveInfinity, 0);
            var result = v1.Angle(v2);

            Assert.AreEqual(Deg90AsRad, result, "vector (-inf,0,0) is perpendicular to vector (0,+inf,0)");
        }

        [TestMethod, TestCategory("Angle")]
        public void Angle_WhereUnitVectorsPositiveYNegativeX_ShouldResultIn90Deg_Test()
        {
            Vector v1 = new Vector(0, 1, 0);
            Vector v2 = new Vector(-1, 0, 0);
            var result = v1.Angle(v2);

            Assert.AreEqual(Deg90AsRad, result, "vector (0,1,0) is perpendicular to vector (-1,0,0)");
        }

        [TestMethod, TestCategory("Angle")]
        public void Angle_WhereVectorsInfinitePositiveYNegativeX_ShouldResultIn90Deg_Test()
        {
            Vector v1 = new Vector(0, double.PositiveInfinity, 0);
            Vector v2 = new Vector(double.NegativeInfinity, 0, 0);
            var result = v1.Angle(v2);

            Assert.AreEqual(Deg90AsRad, result, "vector (0,+inf,0) is perpendicular to vector (-inf,0,0)");
        }

        [TestMethod, TestCategory("Angle")]
        public void Angle_WherePositiveXY_ShouldResultIn90Deg_Test()
        {
            Vector v1 = new Vector(ArbitaryTestDouble, 0, 0);
            Vector v2 = new Vector(0, ArbitaryTestDouble, 0);
            var result = v1.Angle(v2);

            Assert.AreEqual(Deg90AsRad, result, "vector (n,0,0) is perpendicular to vector (0,n,0)");
        }

        [TestMethod, TestCategory("Angle")]
        public void Angle_WherePositiveYX_ShouldResultIn90Deg_Test()
        {
            Vector v1 = new Vector(0, ArbitaryTestDouble, 0);
            Vector v2 = new Vector(ArbitaryTestDouble, 0, 0);
            var result = v1.Angle(v2);

            Assert.AreEqual(Deg90AsRad, result, "vector (0,n,0) is perpendicular to vector (n,0,0)");
        }

        [TestMethod, TestCategory("Angle")]
        public void Angle_WherePositiveXNegativeY_ShouldResultIn90Deg_Test()
        {
            Vector v1 = new Vector(ArbitaryTestDouble, 0, 0);
            Vector v2 = new Vector(0, -ArbitaryTestDouble, 0);
            var result = v1.Angle(v2);

            Assert.AreEqual(Deg90AsRad, result, "vector (n,0,0) is perpendicular to vector (0,-n,0)");
        }

        [TestMethod, TestCategory("Angle")]
        public void Angle_WhereNegativeYPositiveX_ShouldResultIn90Deg_Test()
        {
            Vector v1 = new Vector(0, -ArbitaryTestDouble, 0);
            Vector v2 = new Vector(ArbitaryTestDouble, 0, 0);
            var result = v1.Angle(v2);

            Assert.AreEqual(Deg90AsRad, result, "vector (0,-n,0) is perpendicular to vector (n,0,0)");
        }

        [TestMethod, TestCategory("Angle")]
        public void Angle_WhereNegativeXY_ShouldResultIn90Deg_Test()
        {
            Vector v1 = new Vector(-ArbitaryTestDouble, 0, 0);
            Vector v2 = new Vector(0, -ArbitaryTestDouble, 0);
            var result = v1.Angle(v2);

            Assert.AreEqual(Deg90AsRad, result, "vector (-n,0,0) is perpendicular to vector (0,-n,0)");
        }

        [TestMethod, TestCategory("Angle")]
        public void Angle_WhereNegativeYX_ShouldResultIn90Deg_Test()
        {
            Vector v1 = new Vector(0, -ArbitaryTestDouble, 0);
            Vector v2 = new Vector(-ArbitaryTestDouble, 0, 0);
            var result = v1.Angle(v2);

            Assert.AreEqual(Deg90AsRad, result, "vector (0,-n,0) is perpendicular to vector (-n,0,0)");
        }

        [TestMethod, TestCategory("Angle")]
        public void Angle_WhereNegativeXPositiveY_ShouldResultIn90Deg_Test()
        {
            Vector v1 = new Vector(-ArbitaryTestDouble, 0, 0);
            Vector v2 = new Vector(0, ArbitaryTestDouble, 0);
            var result = v1.Angle(v2);

            Assert.AreEqual(Deg90AsRad, result, "vector (-n,0,0) is perpendicular to vector (0,n,0)");
        }

        [TestMethod, TestCategory("Angle")]
        public void Angle_WherePositiveYNegativeX_ShouldResultIn90Deg_Test()
        {
            Vector v1 = new Vector(0, ArbitaryTestDouble, 0);
            Vector v2 = new Vector(-ArbitaryTestDouble, 0, 0);
            var result = v1.Angle(v2);

            Assert.AreEqual(Deg90AsRad, result, "vector (0,n,0) is perpendicular to vector (-n,0,0)");
        }

        [TestMethod, TestCategory("Angle")]
        public void Angel_WithTwoPerpendicularVectorsUsingWholeNumbers_ShouldBePIRad_Test()
        {
            var a = new Vector(1, 0, 0);
            var b = new Vector(-1, 0, 0);
            var angle = a.Angle(b);

            Assert.AreEqual(System.Math.PI, angle);
        }

        [TestMethod, TestCategory("Angle")]
        public void Angel_WithTwoVectorsUsingWholeNumbers_ShouldResultin180Deg_Test()
        {
            var a = new Vector(1, 0, 0);
            var b = new Vector(-1, 0, 0);
            var angle = a.Angle(b);

            Assert.AreEqual(System.Math.PI, angle);
        }

        [TestMethod, TestCategory("Angle")]
        public void Angle_WithIdenticalVectorsUsingWholeNumbers_ShouldBe0_Test()
        {
            var a = new Vector(1, 0, 0);
            var b = new Vector(1, 0, 0);
            var angle = a.Angle(b);

            Assert.AreEqual(0, angle);
        }

        [TestMethod, TestCategory("Angle")]
        public void Angle_WithIdenticalVectorsUsingLongFloatingPointNumbers_ShouldBe0_Test()
        {
            var a = new Vector(0.795271508195995f, -0.0612034045226753f, -0.603156175071185f);
            var b = new Vector(0.795271508195995f, -0.0612034045226753f, -0.603156175071185f);
            var angle = a.Angle(b);

            Assert.AreEqual(0, angle);
        }

        /// <summary>
        /// Check that the angle between two vectors does not result in NaN
        /// When using a floating point number with positive and negative values as data type float
        /// </summary>
        /// <acknowlagement>Based on an example issue and solution from Dennis E. Cox (In comments on CodeProject, http://www.codeproject.com/Articles/17425/A-Vector-Type-for-C )</acknowlagement>
        [TestMethod, TestCategory("Angle")]
        public void Angle_WithVectorsUsingLongFloatingPointNumbers_ShouldNotResultInNaN_Test()
        {
            var a = new Vector(0.795271508195995f, -0.0612034045226753f, -0.603156175071185f);
            var b = new Vector(0.795271508449802f, -0.0612033993276936f, -0.60315617526368f);
            var angle = a.Angle(b);

            Assert.AreNotEqual(float.NaN, angle);
            Assert.IsTrue((angle) < (1));
            Assert.IsTrue((angle) >= (0));
        }

        [TestMethod, TestCategory("Angle")]
        public void Angle_WithIdenticalVectorsUsingDoubleFloatingPointNumbers_ShouldBe0_Test()
        {
            var a = new Vector(0.795271508195995d, -0.0612034045226753d, -0.603156175071185d);
            var b = new Vector(0.795271508195995d, -0.0612034045226753d, -0.603156175071185d);
            var angle = a.Angle(b);

            Assert.AreEqual(0, angle);
        }

        /// <summary>
        /// Check that the angle between two vectors does not result in NaN
        /// When using a floating point number with positive and negative values as data type double
        /// </summary>
        /// <acknowlagement>Based on an example issue and solution from Dennis E. Cox (In comments on CodeProject, http://www.codeproject.com/Articles/17425/A-Vector-Type-for-C )</acknowlagement>
        [TestMethod, TestCategory("Angle")]
        public void Angle_WithVectorsUsingDoubleFloatingPointNumbers_ShouldNotResultInNaN_Test()
        {
            var a = new Vector(0.795271508195995d, -0.0612034045226753d, -0.603156175071185d);
            var b = new Vector(0.795271508449802d, -0.0612033993276936d, -0.60315617526368d);
            var angle = a.Angle(b);

            Assert.AreNotEqual(float.NaN, angle);
            Assert.IsTrue((angle) < (1));
            Assert.IsTrue((angle) >= (0));
        }

        [TestMethod, TestCategory("Angle")]
        public void Angle_WithOfIdenticalVectorsUsingMaxAndMinFloatingPointNumbers_ShouldBe0_Test()
        {
            var a = new Vector(float.MaxValue, float.MinValue, 0);
            var b = new Vector(float.MaxValue, float.MinValue, 0);
            var angle = a.Angle(b);

            Assert.AreEqual(0, angle);
        }

        [TestMethod, TestCategory("Angle")]
        public void Angle_WithDifferentWholeNumbers_ShouldResultInAFractionalNumber_Test()
        {
            Vector s0 = new Vector(7719, 0, 38);
            Vector v1 = new Vector(38, 0, 7719);
            var result = s0.Angle(v1);

            Assert.AreEqual(1.560950571379345, result, "this was the result worked out by Pex");
            Assert.AreEqual(1.561d, Math.Round(result, 3, MidpointRounding.AwayFromZero), @"this is what http://calculator.tutorvista.com/angle-between-two-vectors-calculator.html worked out the result to be");
        }

        #endregion

        #region Normalize

        [TestMethod, TestCategory("Normalize")]
        public void NormalizeOrDefault_WithPositiveZeroVector_ShouldBe0_Test()
        {
            var vector = new Vector(0, 0, 0);
            var result = Vector.NormalizeOrDefault(vector);

            Assert.AreEqual(new Vector(0, 0, 0), result);
        }

        [TestMethod, TestCategory("Normalize")]
        public void Normalize_WithPositiveZeroVector_ShouldThrowException_Test()
        {
            var vector = new Vector(0, 0, 0);
            Action action = () => vector.Normalize();

            Assert.ThrowsException<NormalizeVectorException>(action);
        }

        [TestMethod, TestCategory("Normalize")]
        public void NormalizeOrDefault_WithNegativeZeroVector_ShouldBe0_Test()
        {
            var vector = new Vector(-0, -0, -0);
            var result = Vector.NormalizeOrDefault(vector);

            Assert.AreEqual(new Vector(0, 0, 0), result);
        }

        [TestMethod, TestCategory("Normalize")]
        public void Normalize_WithNegativeZeroVector_ShouldThrowException_Test()
        {
            var vector = new Vector(-0, -0, -0);
            Action action = () => vector.Normalize();

            Assert.ThrowsException<NormalizeVectorException>(action);
        }

        [TestMethod, TestCategory("Normalize")]
        public void NormalizeOrDefault_WithPositievAndNegativeZeroVector_ShouldBe0_Test()
        {
            var vector = new Vector(-0, +0, -0);
            var result = Vector.NormalizeOrDefault(vector);

            Assert.AreEqual(new Vector(0, 0, 0), result);
        }

        [TestMethod, TestCategory("Normalize")]
        public void Normalize_WithPositievAndNegativeZeroVector_ShouldThrowException_Test()
        {
            var vector = new Vector(-0, +0, -0);
            Action action = () => vector.Normalize();

            Assert.ThrowsException<NormalizeVectorException>(action);
        }

        [TestMethod, TestCategory("Normalize")]
        public void NormalizeOrDefault_WithPositievAndNegativeZeroVector_ShouldThrowException_Test()
        {
            var vector = new Vector(-0, +0, -0);
            var result = Vector.NormalizeOrDefault(vector);

            Assert.AreEqual(new Vector(-0, +0, -0), result);
        }

        [TestMethod, TestCategory("Normalize")]
        public void Normalize_WithUnitVectorXIsOne_ShouldNotChangeDuringNormalization_Test()
        {
            var vector = new Vector(1, 0, 0);
            var result = vector.Normalize();

            Assert.AreEqual(1, result.X);
            Assert.AreEqual(0, result.Y);
            Assert.AreEqual(0, result.Z);
        }

        [TestMethod, TestCategory("Normalize")]
        public void NormalizeOrDefault_WithUnitVectorXIsOne_ShouldNotChangeDuringNormalization_Test()
        {
            var vector = new Vector(1, 0, 0);
            var result = Vector.NormalizeOrDefault(vector);

            Assert.AreEqual(1, result.X);
            Assert.AreEqual(0, result.Y);
            Assert.AreEqual(0, result.Z);
        }

        [TestMethod, TestCategory("Normalize")]
        public void Normalize_WithUnitVectorXIsNegativeOne_ShouldNotChangeDuringNormalization_Test()
        {
            var vector = new Vector(-1, 0, 0);
            var result = vector.Normalize();

            Assert.AreEqual(-1, result.X);
            Assert.AreEqual(0, result.Y);
            Assert.AreEqual(0, result.Z);
        }

        [TestMethod, TestCategory("Normalize")]
        public void NormalizeOrDefault_WithUnitVectorXIsNegativeOne_ShouldNotChangeDuringNormalization_Test()
        {
            var vector = new Vector(-1, 0, 0);
            var result = Vector.NormalizeOrDefault(vector);

            Assert.AreEqual(-1, result.X);
            Assert.AreEqual(0, result.Y);
            Assert.AreEqual(0, result.Z);
        }

        [TestMethod, TestCategory("Normalize")]
        public void Normalize_WithUnitVectorXIsPositiveInfinity_ShouldResultInXBeingOne_Test()
        {
            var vector = new Vector(double.PositiveInfinity, 0, 0);
            var result = vector.Normalize();

            Assert.AreEqual(1, result.X);
            Assert.AreEqual(0, result.Y);
            Assert.AreEqual(0, result.Z);
        }

        [TestMethod, TestCategory("Normalize")]
        public void NormalizeOrDefault_WithUnitVectorXIsPositiveInfinity_ShouldResultInXBeingOne_Test()
        {
            var vector = new Vector(double.PositiveInfinity, 0, 0);
            var result = Vector.NormalizeOrDefault(vector);

            Assert.AreEqual(1, result.X);
            Assert.AreEqual(0, result.Y);
            Assert.AreEqual(0, result.Z);
        }

        [TestMethod, TestCategory("Normalize")]
        public void Normalize_WithVectorXIsPositiveInfinityAndOtherComponentsAreNotNull_ShouldThrowException_Test()
        {
            var vector = new Vector(double.PositiveInfinity, 3, 6);
            Action action = () => vector.Normalize();

            Assert.ThrowsException<NormalizeVectorException>(action);
        }

        [TestMethod, TestCategory("Normalize")]
        public void NormalizeOrDefault_WithVectorXIsPositiveInfinityAndOtherComponentsAreNotNull_ShouldResultInNaN_Test()
        {
            var vector = new Vector(double.PositiveInfinity, 3, 6);
            var result = Vector.NormalizeOrDefault(vector);

            Assert.AreEqual(double.NaN, result.X);
            Assert.AreEqual(double.NaN, result.Y);
            Assert.AreEqual(double.NaN, result.Z);
        }

        [TestMethod, TestCategory("Normalize")]
        public void Normalize_WithUnitVectorXIsNegativeInfinity_ShouldResultInXBeingOne_Test()
        {
            var vector = new Vector(double.NegativeInfinity, 0, 0);
            var result = vector.Normalize();

            Assert.AreEqual(-1, result.X);
            Assert.AreEqual(0, result.Y);
            Assert.AreEqual(0, result.Z);
        }

        [TestMethod, TestCategory("Normalize")]
        public void Normalize_WithVectorXIsNegativeInfinityAndOtherComponentsAreNotNull_ShouldThrowException_Test()
        {
            var vector = new Vector(double.NegativeInfinity, 3, 6);
            Action action = () => vector.Normalize();

            Assert.ThrowsException<NormalizeVectorException>(action);
        }

        [TestMethod, TestCategory("Normalize")]
        public void NormalizeOrDefault_WithVectorXIsNegativeInfinityAndOtherComponentsAreNotNull_ShouldResultInNaN_Test()
        {
            var vector = new Vector(double.NegativeInfinity, 3, 6);
            var result = Vector.NormalizeOrDefault(vector);

            Assert.AreEqual(double.NaN, result.X);
            Assert.AreEqual(double.NaN, result.Y);
            Assert.AreEqual(double.NaN, result.Z);
        }

        [TestMethod, TestCategory("Normalize")]
        public void NormalizeOrDefault_WithUnitVectorXIsNegativeInfinity_ShouldResultInXBeingOne_Test()
        {
            var vector = new Vector(double.NegativeInfinity, 0, 0);
            var result = Vector.NormalizeOrDefault(vector);

            Assert.AreEqual(-1, result.X);
            Assert.AreEqual(0, result.Y);
            Assert.AreEqual(0, result.Z);
        }

        [TestMethod, TestCategory("Normalize")]
        public void Normalize_WithUnitVectorXIsPositiveNumber_ShouldResultInXBeingOne_Test()
        {
            var vector = new Vector(10, 0, 0);
            var result = vector.Normalize();

            Assert.AreEqual(1, result.X);
            Assert.AreEqual(0, result.Y);
            Assert.AreEqual(0, result.Z);
        }

        [TestMethod, TestCategory("Normalize")]
        public void NormalizeOrDefault_WithUnitVectorXIsPositiveNumber_ShouldResultInXBeingOne_Test()
        {
            var vector = new Vector(10, 0, 0);
            var result = Vector.NormalizeOrDefault(vector);

            Assert.AreEqual(1, result.X);
            Assert.AreEqual(0, result.Y);
            Assert.AreEqual(0, result.Z);
        }

        /// <summary>
        /// Test that vectors containing a NaN component will throw an exception when being Normalized
        /// </summary>
        [TestMethod, TestCategory("Normalize")]
        public void Normalize_WithUnitVectorXIsNaN_ShouldThrowException_Test()
        {
            var vector = new Vector(double.NaN, 0, 0);

            Action act = () => vector.Normalize();

            Assert.ThrowsException<NormalizeVectorException>(act, "you should not be able to normalize NaN values");
        }

        [TestMethod, TestCategory("Normalize")]
        public void NormalizeOrDefault_WithUnitVectorXIsNaN_ShouldReturnXYZOfNaN_Test()
        {
            var vector = new Vector(double.NaN, 0, 0);
            var result = Vector.NormalizeOrDefault(vector);

            Assert.AreEqual(double.NaN, result.X);
            Assert.AreEqual(double.NaN, result.Y);
            Assert.AreEqual(double.NaN, result.Z);
        }

        /// <summary>
        /// Test the normalization of a vector
        /// </summary>
        /// <acknowlagement>Example from http://www.fundza.com/vectors/normalize/ </acknowlagement>
        [TestMethod, TestCategory("Normalize")]
        public void Normalize_WithPositiveWholeNumbers_ShouldBeCorrect_Test()
        {
            var vector = new Vector(3, 1, 2);
            var result = vector.Normalize();

            Assert.IsTrue((result.X) >= (0.8014) && (result.X) <= (0.8026));
            Assert.IsTrue((result.Y) >= (0.2664) && (result.Y) <= (0.2676));
            Assert.IsTrue((result.Z) >= (0.5334) && (result.Z) <= (0.5346));
        }

        /// <summary>
        /// Test the normalization of a vector
        /// </summary>
        [TestMethod, TestCategory("Normalize")]
        public void NormalizeOrDefault_WithPositiveWholeNumbers_ShouldBeCorrect_Test()
        {
            var vector = new Vector(3, 1, 2);
            var result = Vector.NormalizeOrDefault(vector);

            Assert.IsTrue((result.X) >= (0.8014) && (result.X) <= (0.8026));
            Assert.IsTrue((result.Y) >= (0.2664) && (result.Y) <= (0.2676));
            Assert.IsTrue((result.Z) >= (0.5334) && (result.Z) <= (0.5346));
        }

        #endregion

        #region Yaw, Pitch, Roll Tests

        /// <summary>
        /// Yaw 90 degrees (checking the result to six decimal places)
        /// </summary>
        [TestMethod]
        public void YawTest()
        {
            var vector = new Vector(1, 0, 0);
            var result = vector.Yaw(new Angle(Deg90AsRad), OrthogonalAxes.DirectX);

            Assert.AreEqual(0, System.Math.Round(result.X, 6));
            Assert.AreEqual(0, System.Math.Round(result.Y, 6));
            Assert.AreEqual(-1, System.Math.Round(result.Z, 6));
        }

        /// <summary>
        /// Pitch 90 degrees (checking the result to six decimal places)
        /// </summary>
        [TestMethod]
        public void PitchTest()
        {
            var vector = new Vector(0, 1, 0);
            var result = vector.Pitch(new Angle(Deg90AsRad), OrthogonalAxes.DirectX);

            Assert.AreEqual(0, System.Math.Round(result.X, 6));
            Assert.AreEqual(0, System.Math.Round(result.Y, 6));
            Assert.AreEqual(1, System.Math.Round(result.Z, 6));
        }

        /// <summary>
        /// Roll 90 degrees (checking the result to six decimal places)
        /// </summary>
        [TestMethod]
        public void RollTest()
        {
            var vector = new Vector(1, 0, 0);
            var result = vector.Roll(new Angle(Deg90AsRad), OrthogonalAxes.DirectX);

            Assert.AreEqual(0, System.Math.Round(result.X, 6));
            Assert.AreEqual(1, System.Math.Round(result.Y, 6));
            Assert.AreEqual(0, System.Math.Round(result.Z, 6));
        }

        #endregion

        #region Project, Rejection and Reflection Tests

        /// <summary>
        /// Test the projection of one vector onto another
        /// </summary>
        /// <acknowlagement>Example from http://www.vitutor.com/geometry/vec/vector_projection.html </acknowlagement>
        [TestMethod]
        public void ProjectionTest()
        {
            var a = new Vector(2, 1, 0);
            var b = new Vector(-3, 4, 0);
            var result = a.Projection(b);

            Assert.AreEqual(6d / 25d, result.X, "X should be 6/25");
            Assert.AreEqual(-(8d / 25d), result.Y, "Y should be -(8/25)");
            Assert.AreEqual(0, result.Z, "Z should be 0");
        }

        /// <summary>
        /// Test the projection of one vector onto another at 90 deg is 0
        /// </summary>
        /// <acknowlagement>Example from http://en.wikipedia.org/wiki/Vector_projection </acknowlagement>
        [TestMethod]
        public void ProjectionWhereVectorsAreAt90DegTest()
        {
            var a = new Vector(1, 0, 0);
            var b = new Vector(0, 1, 0);
            var result = a.Projection(b);

            Assert.AreEqual(0, result.X, "X should be 0");
            Assert.AreEqual(0, result.Y, "Y should be 0");
            Assert.AreEqual(0, result.Z, "Z should be 0");
        }

        /// <summary>
        /// Test the rejection of one vector onto another
        /// </summary>
        [TestMethod]
        public void RejectionTest()
        {
            var a = new Vector(2, 1, 0);
            var b = new Vector(-3, 4, 0);
            var result = a.Rejection(b);

            Assert.AreEqual(2d - (6d / 25d), result.X, "X should be 2-(6/25)");
            Assert.AreEqual(1d + (8d / 25d), result.Y, "Y should be 1+(8/25)");
            Assert.AreEqual(0, result.Z, "Z should be 0");
        }

        [TestMethod]
        public void Reflection_WhereXis1AboutVectorWhereYisOne_ShouldBeVectorWhereXisNegativeOne_Test()
        {
            var a = new Vector(1, 0, 0);
            var b = new Vector(0, 1, 0);
            var result = a.Reflection(b);

            Assert.AreEqual(-1, result.X, "X should be -1");
            Assert.AreEqual(0, result.Y, "Y should be 0");
            Assert.AreEqual(0, result.Z, "Z should be 0");
        }

        [TestMethod]
        public void Reflection_WhereV1IsXNegative10Yis10AboutYAxisUnitVector_ShouldBeX10Y10_Test()
        {
            var a = new Vector(-10, 10, 0);
            var b = new Vector(0, 1, 0); // about y unit vector
            var result = a.Reflection(b);

            Assert.AreEqual(10, result.X, "X should be 10");
            Assert.AreEqual(10, result.Y, "Y should be 10");
            Assert.AreEqual(0, result.Z, "Z should be 0");
        }

        [TestMethod]
        public void Reflection_WhereV1IsY10AboutX5Y5_ShouldBeX10_Test()
        {
            var a = new Vector(0, 10, 0);
            var b = new Vector(5, 5, 0);
            var result = a.Reflection(b);

            Assert.AreEqual(10, result.X, "X should be 10");

            // Epsilon is not tolerant enough
            //result.Y.Should().BeInRange(-double.Epsilon, double.Epsilon, "Y should be 0 with a tolerance (double.epsilon)");
            //result.Z.Should().BeInRange(-double.Epsilon, double.Epsilon, "Z should be 0 with a tolerance (double.epsilon)");

            Assert.IsTrue((result.Y) >= (-1E-10) && (result.Y) <= (1E-10), "Y should be 0 with a tolerance");
            Assert.IsTrue((result.Z) >= (-1E-10) && (result.Z) <= (1E-10), "Z should be 0 with a tolerance");
        }

        #endregion

        #region Rounding Tests

        [TestMethod]
        public void RoundingTest()
        {
            var vector = new Vector(0.1, 0.5, 0.9);
            var result = vector.Round(MidpointRounding.AwayFromZero);

            Assert.AreEqual(0, result.X, "X should be rounded down");
            Assert.AreEqual(1, result.Y, "Y should be rounded up");
            Assert.AreEqual(1, result.Z, "Z should be rounded up");
        }

        #endregion

        #region Rotation Tests

        [TestMethod]
        public void RotateArroundYAxisShouldBeTheSameAsYawTest()
        {
            var vector = new Vector(1, 0, 0);
            var yaw = vector.Yaw(new Angle(Deg90AsRad), OrthogonalAxes.DirectX);
            var rotate = vector.RotateY(Deg90AsRad);

            Assert.IsTrue(rotate.Equals(yaw, 1e-9));
        }

        /// <summary>
        /// Pitch 90 degrees (checking the result to six decimal places)
        /// </summary>
        [TestMethod]
        public void RotateArroundXAxisShouldBeTheSameAsPitchTest()
        {
            var vector = new Vector(0, 1, 0);
            var pitch = vector.Pitch(new Angle(Deg90AsRad), OrthogonalAxes.DirectX);
            var rotate = vector.RotateX(Deg90AsRad);

            Assert.AreEqual(0, System.Math.Round(pitch.X, 6));
            Assert.AreEqual(0, System.Math.Round(pitch.Y, 6));
            Assert.AreEqual(1, System.Math.Round(pitch.Z, 6));

            Assert.IsTrue(rotate.Equals(pitch, 1e-9));
        }

        /// <summary>
        /// Roll 90 degrees (checking the result to six decimal places)
        /// </summary>
        [TestMethod]
        public void RotateArroundZShouldBeTheSameAsRollTest()
        {
            var vector = new Vector(1, 0, 0);
            var roll = vector.Roll(new Angle(Deg90AsRad), OrthogonalAxes.DirectX);
            var rotate = vector.RotateZ(Deg90AsRad);

            Assert.IsTrue(rotate.Equals(roll, 1e-9));
        }

        [TestMethod]
        public void RotateArroundYWith0OffsetParametersAxisShouldBeTheSameAsYawTest()
        {
            var vector = new Vector(1, 0, 0);
            var yaw = vector.Yaw(new Angle(Deg90AsRad), OrthogonalAxes.DirectX);
            var rotate = vector.RotateY(0, 0, Deg90AsRad);

            Assert.IsTrue(rotate.Equals(yaw, 1e-9));
        }

        /// <summary>
        /// Pitch 90 degrees (checking the result to six decimal places)
        /// </summary>
        [TestMethod]
        public void RotateArroundXAxis0OffsetParametersShouldBeTheSameAsPitchTest()
        {
            var vector = new Vector(0, 1, 0);
            var pitch = vector.Pitch(new Angle(Deg90AsRad), OrthogonalAxes.DirectX);
            var rotate = vector.RotateX(0, 0, Deg90AsRad);

            Assert.AreEqual(0, System.Math.Round(pitch.X, 6));
            Assert.AreEqual(0, System.Math.Round(pitch.Y, 6));
            Assert.AreEqual(1, System.Math.Round(pitch.Z, 6));

            Assert.IsTrue(rotate.Equals(pitch, 1e-9));
        }

        /// <summary>
        /// Roll 90 degrees (checking the result to six decimal places)
        /// </summary>
        [TestMethod]
        public void RotateArroundZ0OffsetParametersShouldBeTheSameAsRollTest()
        {
            var vector = new Vector(1, 0, 0);
            var roll = vector.Roll(new Angle(Deg90AsRad), OrthogonalAxes.DirectX);
            var rotate = vector.RotateZ(0, 0, Deg90AsRad);

            Assert.IsTrue(rotate.Equals(roll, 1e-9));
        }

        #endregion

        #region Comparison Tests

        [TestMethod, TestCategory("CompareTo")]
        public void CompareTo_WhereWeAreTestingGreaterObjectEquivenenceAndLess_Test()
        {
            // Magnitude 8555.6321215910166084530447384188
            Vector s0 = new Vector(1796, 0, 8365);
            object box = (object)(s0);

            // Magnitude 2404.1643038694339619441734523204
            Vector v1 = new Vector(449, 2282, 609);

            var result = s0.CompareTo(v1);
            Assert.AreEqual(1, result, "magnitude of s0 is bigger than magnitude of s1");

            result = v1.CompareTo(s0);
            Assert.AreEqual(-1, result, "magnitude of s1 is less than magnitude of s0");

            result = v1.CompareTo(box);
            Assert.AreEqual(-1, result, "magnitude of s1 is less than magnitude of s0 even when s0 has been cast to an object");
        }

        [TestMethod, TestCategory("CompareTo")]
        public void CompareTo_WhereZComponentsArePositiveInfinity_ShouldBe0_Test()
        {
            Vector v1 = new Vector(0, 0, double.PositiveInfinity);
            Vector v2 = new Vector(0, 0, double.PositiveInfinity);

            var zComponentResult = v1.Z.CompareTo(v2.Z);
            var result = v1.CompareTo(v2);

            // Test our assumption about the .Net framework expectations
            Assert.AreEqual(0, double.PositiveInfinity.CompareTo(double.PositiveInfinity), "the .Net framework should find double positive infinty equal to positive infinity (if this assumption is wrong then the logic of this test is also wrong)");

            // Test that the component operation matches our assumption about the .Net framework
            Assert.AreEqual(0, zComponentResult, "z components of positive infinty and positive infinty should be equal");

            // Test that our result matches the assumption
            Assert.AreEqual(0, result, "positive infinty and positive infinty should be equal, as should the other components");
        }

        [TestMethod, TestCategory("CompareTo")]
        public void CompareTo_WhereZComponentsAreNaN_ShouldBe0_Test()
        {
            Vector v1 = new Vector(0, 0, double.NaN);
            Vector v2 = new Vector(0, 0, double.NaN);

            var zComponentResult = v1.Z.CompareTo(v2.Z);
            var result = v1.CompareTo(v2);

            // Test our assumption about the .Net framework expectations
            Assert.AreEqual(0, double.NaN.CompareTo(double.NaN), "the .Net framework should find double NaN equal to NaN (if this assumption is wrong then the logic of this test is also wrong)");

            // Test that the component operation matches our assumption about the .Net framework
            Assert.AreEqual(0, zComponentResult, "z components of NaN and NaN should be equal");

            // Test that our result matches the assumption
            Assert.AreEqual(0, result, "NaN and NaN should be equal, as should the other components");
        }

        [TestMethod, TestCategory("CompareTo")]
        public void CompareTo_WhereOneZIsNaNAndTheOtherXIsNaN_ShouldBe0_Test()
        {
            Vector v1 = new Vector(0, 0, double.NaN);
            Vector v2 = new Vector(double.NaN, 0, 0);

            var result = v1.CompareTo(v2);

            Assert.AreEqual(0, result, "both magnitudes should be NaN so comparison should find the vectors equivelent");
        }

        [TestMethod, TestCategory("CompareTo")]
        public void CompareTo_WhereOneZIsNaNAndTheOtherXIsNaNAndTheYComponentsDoNotMatch_ShouldBe0_Test()
        {
            Vector v1 = new Vector(0, 10, double.NaN);
            Vector v2 = new Vector(double.NaN, -3, 0);

            var result = v1.CompareTo(v2);

            Assert.AreEqual(0, result, "both magnitudes should be NaN when any component is NaN so comparison should find the vectors equivelent");
        }

        [TestMethod, TestCategory("CompareTo")]
        public void CompareTo_WhereOneXYIsNaNAndTheOtherXIsNaN_ShouldBe0_Test()
        {
            Vector v1 = new Vector(double.NaN, double.NaN, 0);
            Vector v2 = new Vector(double.NaN, 0, 0);

            var result = v1.CompareTo(v2);

            Assert.AreEqual(0, result, "both magnitudes should be NaN so comparison should find the vectors equivelent");
        }

        [TestMethod, TestCategory("CompareTo")]
        public void CompareTo_WhereZComponentsAreNegativeInfinity_ShouldBe0_Test()
        {
            Vector v1 = new Vector(0, 0, double.NegativeInfinity);
            Vector v2 = new Vector(0, 0, double.NegativeInfinity);

            var zComponentResult = v1.Z.CompareTo(v2.Z);
            var result = v1.CompareTo(v2);

            // Test our assumption about the .Net framework expectations
            Assert.AreEqual(0, double.NegativeInfinity.CompareTo(double.NegativeInfinity), "the .Net framework should find double negative infinty equal to negative infinity (if this assumption is wrong then the logic of this test is also wrong)");

            // Test that the component operation matches our assumption about the.Net framework
            Assert.AreEqual(0, zComponentResult, "z components of negative infinty and negative infinty should be equal");

            // Test that our result matches the assumption
            Assert.AreEqual(0, result, "negative infinty and negative infinty should be equal, as should the other components");
        }

        [TestMethod, TestCategory("CompareTo")]
        public void CompareTo_WhereZComponentsArePositiveInfinityAndNegativeInfinity_ShouldNotBe0_Test()
        {
            Vector v1 = new Vector(0, 0, double.PositiveInfinity);
            Vector v2 = new Vector(0, 0, double.NegativeInfinity);

            var s1s2Result = v1.CompareTo(v2);
            var s2s1Result = v2.CompareTo(v1);

            Assert.AreEqual(0, s1s2Result, "when comparing magnitude sign should not be important so positive infinty should be equal to negative infinty");
            Assert.AreEqual(0, s2s1Result, "when comparing magnitude sign should not be important so negative infinty should be equal to positive infinty");
        }

        [TestMethod, TestCategory("CompareTo")]
        public void CompareToWithTolerance_WhereTheZComponentsAreDifferentByLessThanTheTolerance_ShouldBe0_Test()
        {
            Vector v1 = new Vector(0, 0, 3);
            Vector v2 = new Vector(0, 0, 2.9999);

            var result = v1.CompareTo(v2, 0.0002);

            // Quick test to ensure that the double minus operation does not introduce an unexpected margin of error that is higher than our margin of error
            Assert.IsTrue(((3d - (2.9999d))) < (0.0002d), "the double calculations of 3 minus 2.9999 should be less than 0.0002 or our test is incorrect");

            // Assert our acual test
            Assert.AreEqual(0, result, "the tolerance should have been greter than the difference between the z components for s1 and s2");
        }

        [TestMethod, TestCategory("CompareTo")]
        public void CompareToWithTolerance_WhereZComponentDifferenceIsGreaterThanTheTolerance_ShouldBe1_Test()
        {
            Vector v1 = new Vector(0, 0, 3);
            Vector v2 = new Vector(0, 0, 2.9999);

            var result = v1.CompareTo(v2, 0.00009);

            // Quick test to ensure that the double minus operation does not introduce an unexpected margin of error
            Assert.IsTrue(((3d - 2.9999d)) > (0.00009d), "the double calculations of 3 minus 2.9999 should be greater than 0.00009 or our test is incorrect");

            Assert.AreEqual(1, result, "the tolerance should be less than the difference between the z components for s1 and s2, so 3 should be greater than 2.9999");
        }

        [TestMethod, TestCategory("CompareTo")]
        public void CompareToWithTolerance_WhereZComponentDifferenceIsGreaterThanTheTolerance_ShouldBeNegative1_Test()
        {
            Vector v1 = new Vector(0, 0, 2.9999);
            Vector v2 = new Vector(0, 0, 3);
            
            var result = v1.CompareTo(v2, 0.00009);

            // Quick test to ensure that the double minus operation does not introduce an unexpected margin of error
            Assert.IsTrue(((3d - 2.9999d)) > (0.00009d), "the double calculations of 3 minus 2.9999 should be greater than 0.00009 or our test is incorrect");

            Assert.AreEqual(-1, result, "the tolerance should be less than the difference between the z components for s1 and s2, so 2.9999 should be less than 3");
        }

        [TestMethod, TestCategory("CompareTo")]
        public void CompareToWithTolerance_WhereZComponentsArePositiveInfinity_ShouldBe0_Test()
        {
            Vector v1 = new Vector(0, 0, double.PositiveInfinity);
            Vector v2 = new Vector(0, 0, double.PositiveInfinity);

            var result = v1.CompareTo(v2, 0.00001);

            Assert.AreEqual(0, result, "positive infinty and positive infinty should be equal regardless of tolerance, as should the other components");
        }

        [TestMethod, TestCategory("CompareTo")]
        public void CompareToWithTolerance_WhereZComponentsAreNegativeInfinity_ShouldBe0_Test()
        {
            Vector v1 = new Vector(0, 0, double.NegativeInfinity);
            Vector v2 = new Vector(0, 0, double.NegativeInfinity);

            var result = v1.CompareTo(v2, 0.00001);

            Assert.AreEqual(0, result, "negative infinty and negative infinty should be equal regardless of tolerance, as should the other components");
        }

        [TestMethod, TestCategory("CompareTo")]
        public void CompareToWithTolerance_WhereZComponentsArePositiveInfinityAndNegativeInfinity_ShouldNotBe0_Test()
        {
            Vector v1 = new Vector(0, 0, double.PositiveInfinity);
            Vector v2 = new Vector(0, 0, double.NegativeInfinity);

            var s1s2Result = v1.CompareTo(v2, 0.00001);
            var s2s1Result = v2.CompareTo(v1, 0.00001);

            Assert.AreEqual(0, s1s2Result, "when comparing magnitude sign should not be important so positive infinty should be equal to negative infinty");
            Assert.AreEqual(0, s2s1Result, "when comparing magnitude sign should not be important so negative infinty should be equal to positive infinty");
        }

        #endregion

        #region Equality

        [TestMethod, TestCategory("Equals")]
        public void EqualityOpererator_WhereTheComponentsAreEqual_ShouldBeTrue_Test()
        {
            Vector v1 = new Vector(1, 2, 3);
            Vector v2 = new Vector(1, 2, 3);

            var result = v1 == v2;

            Assert.AreEqual(true, result);
        }

        [TestMethod, TestCategory("Equals")]
        public void EqualityOpererator_WhereXYZOrderIsImportantAndTheComponentAreNotInTheCorrectOrder_ShouldBeFalse_Test()
        {
            Vector v1 = new Vector(1, 2, 3);
            Vector v2 = new Vector(3, 2, 1);

            var result = v1 == v2;

            Assert.AreEqual(false, result);
        }

        [TestMethod, TestCategory("Equals")]
        public void EqualityOpererator_WhereZComponentsArePositiveInfinity_ShouldBeTrue_Test()
        {
            Vector v1 = new Vector(1, 2, double.PositiveInfinity);
            Vector v2 = new Vector(1, 2, double.PositiveInfinity);

            var zComponentResult = v1.Z == v2.Z;
            var result = v1 == v2;

            // Test our assumption about the .Net framework expectations
            Assert.AreEqual(true, (double.PositiveInfinity == double.PositiveInfinity), "the .Net framework should find double positive infinty equal to positive infinity (if this assumption is wrong then the logic of this test is also wrong)");

            // Test that the component operation matches our assumption about the .Net framework
            Assert.AreEqual(true, zComponentResult, "z components of positive infinty and positive infinty should be equal");

            // Test that our result matches the assumption
            Assert.AreEqual(true, result, "positive infinty and positive infinty should be equal");
        }

        [TestMethod, TestCategory("Equals")]
        public void EqualityOpererator_WhereZComponentsAreNegativeInfinity_ShouldBeTrue_Test()
        {
            Vector v1 = new Vector(1, 2, double.NegativeInfinity);
            Vector v2 = new Vector(1, 2, double.NegativeInfinity);

            var zComponentResult = v1.Z == v2.Z;
            var result = v1 == v2;

            // Test our assumption about the .Net framework expectations
            Assert.AreEqual(true, (double.NegativeInfinity == double.NegativeInfinity), "the .Net framework should find double negative infinty equal to negative infinity (if this assumption is wrong then the logic of this test is also wrong)");

            // Test that the component operation matches our assumption about the.Net framework
            Assert.AreEqual(true, zComponentResult, "z components of negative infinty and negative infinty should be equal");

            // Test that our result matches the assumption
            Assert.AreEqual(true, result, "negative infinty and negative infinty should be equal");
        }

        [TestMethod, TestCategory("Equals")]
        public void EqualityOpererator_WhereOneZComponentIsNaN_ShouldBeFalse_Test()
        {
            Vector v1 = new Vector(1, 2, 3);
            Vector v2 = new Vector(1, 2, double.NaN);

            var zComponentResult = v1.Z == v2.Z;
            var result = v1 == v2;

            // Test our assumption about the .Net framework expectations
            Assert.AreEqual(false, (3 == double.NaN), "the .Net framework should find double 3 not equal to double NaN (if this assumption is wrong then the logic of this test is also wrong)");

            // Test that the component operation matches our assumption about the .Net framework
            Assert.AreEqual(false, zComponentResult, "3 and NaN should not be equal");

            // Test that our result matches the assumption
            Assert.AreEqual(false, result, "the two vectors should not be equal given one of the Z components is NaN");
        }

        [TestMethod, TestCategory("Equals")]
        public void EqualityOpererator_WhereZComponentsAreNaN_ShouldBeFalse_Test()
        {
            // Interestingly double.NaN == double.NaN is false while double.NaN.Equals(double.NaN) is true.
            // Lets be consistent with the .Net framework even if it isn't consistent with itself

            Vector v1 = new Vector(1, 2, double.NaN);
            Vector v2 = new Vector(1, 2, double.NaN);

            var zComponentResult = v1.Z == v2.Z;
            var result = v1 == v2;

            // Test our assumption about the .Net framework expectations
            Assert.AreEqual(false, (double.NaN == double.NaN), "the .Net framework should find double NaN not equal to double NaN using the == operator (if this assumption is wrong then the logic of this test is also wrong)");

            // Test that the component operation matches our assumption about the .Net framework
            Assert.AreEqual(false, zComponentResult, "NaN and NaN should not be equal when using the == operator");

            // Test that our result matches the assumption
            Assert.AreEqual(false, result, "the two vectors should be not equal given the z components are NaN");
        }

        [TestMethod, TestCategory("Equals")]
        public void Equality_WhereTheComponentsAreEqual_ShouldBeTrue_Test()
        {
            Vector v1 = new Vector(1, 2, 3);
            Vector v2 = new Vector(1, 2, 3);

            var result = v1.Equals(v2);

            Assert.AreEqual(true, result);
        }

        [TestMethod, TestCategory("Equals")]
        public void Equality_WhereXYZOrderIsImportantAndTheComponentAreNotInTheCorrectOrder_ShouldBeFalse_Test()
        {
            Vector v1 = new Vector(1, 2, 3);
            Vector v2 = new Vector(3, 2, 1);

            var result = v1.Equals(v2);

            Assert.AreEqual(false, result);
        }

        [TestMethod, TestCategory("Equals")]
        public void Equality_WhereZComponentsArePositiveInfinity_ShouldBeTrue_Test()
        {
            Vector v1 = new Vector(1, 2, double.PositiveInfinity);
            Vector v2 = new Vector(1, 2, double.PositiveInfinity);

            var zComponentResult = v1.Z.Equals(v2.Z);
            var result = v1.Equals(v2);

            // Test our assumption about the .Net framework expectations
            Assert.AreEqual(true, double.PositiveInfinity.Equals(double.PositiveInfinity), "the .Net framework should find double positive infinty equal to positive infinity (if this assumption is wrong then the logic of this test is also wrong)");

            // Test that the component operation matches our assumption about the .Net framework
            Assert.AreEqual(true, zComponentResult, "z components of positive infinty and positive infinty should be equal");

            // Test that our result matches the assumption
            Assert.AreEqual(true, result, "positive infinty and positive infinty should be equal");
        }

        [TestMethod, TestCategory("Equals")]
        public void Equality_WhereZComponentsAreNegativeInfinity_ShouldBeTrue_Test()
        {
            Vector v1 = new Vector(1, 2, double.NegativeInfinity);
            Vector v2 = new Vector(1, 2, double.NegativeInfinity);

            var zComponentResult = v1.Z.Equals(v2.Z);
            var result = v1.Equals(v2);

            // Test our assumption about the .Net framework expectations
            Assert.AreEqual(true, double.NegativeInfinity.Equals(double.NegativeInfinity), "the .Net framework should find double negative infinty equal to negative infinity (if this assumption is wrong then the logic of this test is also wrong)");

            // Test that the component operation matches our assumption about the.Net framework
            Assert.AreEqual(true, zComponentResult, "z components of negative infinty and negative infinty should be equal");

            // Test that our result matches the assumption
            Assert.AreEqual(true, result, "negative infinty and negative infinty should be equal");
        }

        [TestMethod, TestCategory("Equals")]
        public void Equality_WhereOneZComponentIsNaN_ShouldBeFalse_Test()
        {
            Vector v1 = new Vector(1, 2, 3);
            Vector v2 = new Vector(1, 2, double.NaN);

            var zComponentResult = v1.Z.Equals(v2.Z);
            var result = v1.Equals(v2);

            // Test our assumption about the .Net framework expectations
            Assert.AreEqual(false, 3.Equals(double.NaN), "the .Net framework should find double 3 not equal to double NaN (if this assumption is wrong then the logic of this test is also wrong)");

            // Test that the component operation matches our assumption about the .Net framework
            Assert.AreEqual(false, zComponentResult, "3 and NaN should not be equal");

            // Test that our result matches the assumption
            Assert.AreEqual(false, result, "the two vectors should not be equal given one of the Z components is NaN");
        }

        [TestMethod, TestCategory("Equals")]
        public void Equality_WhereZComponentsAreNaN_ShouldBeTrue_Test()
        {
            // Interestingly double.NaN == double.NaN is false while double.NaN.Equals(double.NaN) is true.
            // Lets be consistent with the .Net framework even if it isn't consistent with itself

            Vector v1 = new Vector(1, 2, double.NaN);
            Vector v2 = new Vector(1, 2, double.NaN);

            var zComponentResult = v1.Z.Equals(v2.Z);
            var result = v1.Equals(v2);

            // Test our assumption about the .Net framework expectations
            Assert.AreEqual(true, double.NaN.Equals(double.NaN), "the .Net framework should find double NaN equal to double NaN using the Equals method (if this assumption is wrong then the logic of this test is also wrong)");

            // Test that the component operation matches our assumption about the .Net framework
            Assert.AreEqual(true, zComponentResult, "NaN and NaN should be equal");

            // Test that our result matches the assumption
            Assert.AreEqual(true, result, "the two vectors should be equal");
        }

        [TestMethod, TestCategory("Equals")]
        public void EqualityWithTolerance_WhereTheZComponentsAreDifferentByLessThanTheTolerance_ShouldBeTrue_Test()
        {
            Vector v1 = new Vector(1, 2, 3);
            Vector v2 = new Vector(1, 2, 2.9999);

            var result = v1.Equals(v2, 0.0002);

            // Quick test to ensure that the double minus operation does not introduce an unexpected margin of error that is higher than our margin of error
            Assert.IsTrue(((3d - (2.9999d))) < (0.0002d), "the double calculations of 3 minus 2.9999 should be less than 0.0002 or our test is incorrect");
            
            // Assert our acual test
            Assert.AreEqual(true, result, "the tolerance should have been greter than the difference between the z components for s1 and s2");
        }

        [TestMethod, TestCategory("Equals")]
        public void EqualityWithTolerance_WhereZComponentDifferenceIsGreaterThanTheTolerance_ShouldBeFalse_Test()
        {
            Vector v1 = new Vector(1, 2, 3);
            Vector v2 = new Vector(1, 2, 2.9999);

            var result = v1.Equals(v2, 0.00009);

            // Quick test to ensure that the double minus operation does not introduce an unexpected margin of error
            Assert.IsTrue(((3d - 2.9999d)) > (0.00009d), "the double calculations of 3 minus 2.9999 should be greater than 0.00009 or our test is incorrect");

            Assert.AreEqual(false, result, "the tolerance should be less than the difference between the z components for s1 and s2");
        }

        [TestMethod, TestCategory("Equals")]
        public void EqualityWithTolerance_WhereZComponentsArePositiveInfinity_ShouldBeTrue_Test()
        {
            Vector v1 = new Vector(1, 2, double.PositiveInfinity);
            Vector v2 = new Vector(1, 2, double.PositiveInfinity);

            var zComponentResult = v1.Z.Equals(v2.Z);
            var result = v1.Equals(v2, 0.00001);

            // Test our assumption about the .Net framework expectations
            Assert.AreEqual(true, double.PositiveInfinity.Equals(double.PositiveInfinity), "the .Net framework should find double positive infinty equal to positive infinity (if this assumption is wrong then the logic of this test is also wrong)");

            // Test that the component operation matches our assumption about the .Net framework
            Assert.AreEqual(true, zComponentResult, "positive infinty and positive infinty should be equal regardless of tolerance");

            // Test that our result matches the assumption
            Assert.AreEqual(true, result, "positive infinty and positive infinty should be equal regardless of tolerance");
        }

        [TestMethod, TestCategory("Equals")]
        public void EqualityWithTolerance_WhereZComponentsAreNegativeInfinity_ShouldBeTrue_Test()
        {
            Vector v1 = new Vector(1, 2, double.NegativeInfinity);
            Vector v2 = new Vector(1, 2, double.NegativeInfinity);

            var zComponentResult = v1.Z.Equals(v2.Z);
            var result = v1.Equals(v2, 0.00001);

            // Test our assumption about the .Net framework expectations
            Assert.AreEqual(true, double.NegativeInfinity.Equals(double.NegativeInfinity), "the .Net framework should find double negative infinty equal to negative infinity (if this assumption is wrong then the logic of this test is also wrong)");

            // Test that the component operation matches our assumption about the .Net framework
            Assert.AreEqual(true, zComponentResult, "negative infinty and negative infinty should be equal regardless of tolerance");

            // Test that our result matches the assumption
            Assert.AreEqual(true, result, "the two vectors should be equal given negative infinty and negative infinty should be equal regardless of tolerance");
        }

        [TestMethod, TestCategory("Equals")]
        public void EqualityWithTolerance_WhereZComponentsArePositiveInfinityAndNegativeInfinity_ShouldBeFalse_Test()
        {
            Vector v1 = new Vector(1, 2, double.PositiveInfinity);
            Vector v2 = new Vector(1, 2, double.NegativeInfinity);

            var zComponentResult = v1.Z.Equals(v2.Z);
            var result = v1.Equals(v2, 0.00001);

            // Test our assumption about the .Net framework expectations
            Assert.AreEqual(false, double.PositiveInfinity.Equals(double.NegativeInfinity), "the .Net framework should find double positive infinty not equal to negative infinity (if this assumption is wrong then the logic of this test is also wrong)");

            // Test that the component operation matches our assumption about the .Net framework
            Assert.AreEqual(false, zComponentResult, "positive infinty and negative infinty should not be equal");

            // Test that our result matches the assumption
            Assert.AreEqual(false, result, "the two vectors should not be equal given positive infinty and negative infinty should not be equal");
        }

        [TestMethod, TestCategory("Equals")]
        public void EqualityWithTolerance_WhereOneZComponentIsNaN_ShouldBeFalse_Test()
        {
            Vector v1 = new Vector(1, 2, 3);
            Vector v2 = new Vector(1, 2, double.NaN);

            var zComponentResult = v1.Z.Equals(v2.Z);
            var result = v1.Equals(v2, 0.00001);

            // Test our assumption about the .Net framework expectations
            Assert.AreEqual(false, 3.Equals(double.NaN), "the .Net framework should find double 3 not equal to double NaN (if this assumption is wrong then the logic of this test is also wrong)");

            // Test that the component operation matches our assumption about the .Net framework
            Assert.AreEqual(false, zComponentResult, "3 and NaN should not be equal");

            // Test that our result matches the assumption
            Assert.AreEqual(false, result, "the two vectors should not be equal given one of the Z components is NaN");
        }

        [TestMethod, TestCategory("Equals")]
        public void EqualityWithTolerance_WhereZComponentsAreNaN_ShouldBeTrue_Test()
        {
            /* Interestingly double.NaN == double.NaN is false while double.NaN.Equals(double.NaN) is true.
               Lets be consistent with the .Net framework even if it isn't consistent with itself */

            Vector v1 = new Vector(1, 2, double.NaN);
            Vector v2 = new Vector(1, 2, double.NaN);

            var zComponentResult = v1.Z.Equals(v2.Z);
            var result = v1.Equals(v2, 0.00001);

            // Test our assumption about the .Net framework expectations
            Assert.AreEqual(true, double.NaN.Equals(double.NaN), "the .Net framework should find double NaN equal to double NaN using the Equals method (if this assumption is wrong then the logic of this test is also wrong)");

            // Test that the component operation matches our assumption about the .Net framework
            Assert.AreEqual(true, zComponentResult, "NaN and NaN should be equal");

            // Test that our result matches the assumption
            Assert.AreEqual(true, result, "the two vectors should be equal");
        }

        [TestMethod, TestCategory("Equals")]
        public void GetHashCode_WhereTheComponentsAreEqual_ShouldResultInEquivelentHashCodes_Test()
        {
            Vector v1 = new Vector(1, 2, 3);
            Vector v2 = new Vector(1, 2, 3);

            var result = v1.GetHashCode() == v2.GetHashCode();

            Assert.AreEqual(true, result);
        }

        [TestMethod, TestCategory("Equals")]
        public void GetHashCode_WhereXYZOrderIsImportantAndTheComponentAreNotInTheCorrectOrder_ShouldNotResultInEquivelentHashCodes_Test()
        {
            Vector v1 = new Vector(1, 2, 3);
            Vector v2 = new Vector(3, 2, 1);

            var result = v1.GetHashCode() == v2.GetHashCode();

            Assert.AreEqual(false, result);
        }

        [TestMethod, TestCategory("Equals")]
        public void GetHashCode_WhereZComponentsArePositiveInfinity_ShouldResultInEquivelentHashCodes_Test()
        {
            Vector v1 = new Vector(1, 2, double.PositiveInfinity);
            Vector v2 = new Vector(1, 2, double.PositiveInfinity);

            var zComponentResult = v1.Z.GetHashCode() == v2.Z.GetHashCode();
            var result = v1.GetHashCode() == v2.GetHashCode();

            // Test our assumption about the .Net framework expectations
            Assert.AreEqual(true, (double.PositiveInfinity.GetHashCode() == double.PositiveInfinity.GetHashCode()), "the .Net framework should find double positive infinty's hash code equal to positive infinity's hash code (if this assumption is wrong then the logic of this test is also wrong)");

            // Test that the component operation matches our assumption about the .Net framework
            Assert.AreEqual(true, zComponentResult, "z component hash codes of positive infinty and positive infinty should be equal");

            // Test that our result matches the assumption
            Assert.AreEqual(true, result, "positive infinty and positive infinty hash codes should be equal");
        }

        [TestMethod, TestCategory("Equals")]
        public void GetHashCode_WhereZComponentsAreNegativeInfinity_ShouldResultInEquivelentHashCodes_Test()
        {
            Vector v1 = new Vector(1, 2, double.NegativeInfinity);
            Vector v2 = new Vector(1, 2, double.NegativeInfinity);

            var zComponentResult = v1.Z.GetHashCode() == v2.Z.GetHashCode();
            var result = v1.GetHashCode() == v2.GetHashCode();

            // Test our assumption about the .Net framework expectations
            Assert.AreEqual(true, (double.NegativeInfinity.GetHashCode() == double.NegativeInfinity.GetHashCode()), "the .Net framework should find double negative infinty's has code equal to negative infinity's hash code (if this assumption is wrong then the logic of this test is also wrong)");

            // Test that the component operation matches our assumption about the.Net framework
            Assert.AreEqual(true, zComponentResult, "z component hash codes of negative infinty and negative infinty should be equal");

            // Test that our result matches the assumption
            Assert.AreEqual(true, result, "negative infinty and negative infinty hash codes should be equal");
        }

        [TestMethod, TestCategory("Equals")]
        public void GetHashCode_WhereOneZComponentIsNaN_ShouldNotResultInEquivelentHashCodes_Test()
        {
            Vector v1 = new Vector(1, 2, 3);
            Vector v2 = new Vector(1, 2, double.NaN);

            var result = v1.GetHashCode() == v2.GetHashCode();

            Assert.AreEqual(false, result, "the two vectors' hash codes should not be equal given one of the Z components is NaN");
        }

        [TestMethod, TestCategory("Equals")]
        public void GetHashCode_WhereZComponentsAreNaN_ShouldResultInEquivelentHashCodes_Test()
        {
            // Interestingly double.NaN == double.NaN is false while double.NaN.Equals(double.NaN) is true.
            // I am testing the assumption that GetHashCode follows the logic of .Equals()

            Vector v1 = new Vector(1, 2, double.NaN);
            Vector v2 = new Vector(1, 2, double.NaN);

            var zComponentResult = v1.Z.GetHashCode() == v2.Z.GetHashCode();
            var result = v1.GetHashCode() == v2.GetHashCode();

            // Test our assumption about the .Net framework expectations
            Assert.AreEqual(true, (double.NaN.GetHashCode() == double.NaN.GetHashCode()), "the .Net framework should find the hash codes of double NaN and double NaN equal (if this assumption is wrong then the logic of this test is also wrong)");

            // Test that the component operation matches our assumption about the .Net framework
            Assert.AreEqual(true, zComponentResult, "the hash codes of NaN and NaN should be equal");

            // Test that our result matches the assumption
            Assert.AreEqual(true, result, "the two vectors' hashcodes should be equal given the z components are NaN");
        }

        #endregion

        #region Is Unit Vector Tests

        [TestMethod, TestCategory("IsUnitVector")]
        public void IsUnitVector_WhereXIsNaN_ShouldBeFalse_Test()
        {
            Vector v1 = new Vector(double.NaN, 0, 0);
            var result = v1.IsUnitVector();

            Assert.AreEqual(false, result, "vector (NaN,0,0) is not a unit vector");
        }

        [TestMethod, TestCategory("IsUnitVector")]
        public void IsUnitVector_x1y0z0_ShouldBeTrue_Test()
        {
            Vector v1 = new Vector(1, 0, 0);
            var result = v1.IsUnitVector();

            Assert.AreEqual(true, result, "vector (1,0,0) is a unit vector");
        }

        [TestMethod, TestCategory("IsUnitVector")]
        public void IsUnitVector_WhereXYZAreOne_ShouldBeFalse_Test()
        {
            Vector v1 = new Vector(1, 1, 1);
            var result = v1.IsUnitVector();

            Assert.AreEqual(false, result, "vector (1,1,1) is not a unit vector");
        }

        [TestMethod, TestCategory("IsUnitVector")]
        public void IsUnitVector_WhereZComponentIsPositiveInfinity_ShouldBeFalse_Test()
        {
            Vector v1 = new Vector(1, 2, double.PositiveInfinity);
            var result = v1.IsUnitVector();

            Assert.AreEqual(false, result, "vector (0,0, +Inf) is not a unit vector");
        }

        [TestMethod, TestCategory("IsUnitVector")]
        public void IsUnitVector_WhereZComponentIsNegativeInfinity_ShouldBeFalse_Test()
        {
            Vector v1 = new Vector(1, 2, double.NegativeInfinity);
            var result = v1.IsUnitVector();

            Assert.AreEqual(false, result, "vector (0,0, -Inf) is not a unit vector");
        }

        [TestMethod, TestCategory("IsUnitVector")]
        public void IsUnitVectorWithTolerance_x1y0z0_ShouldBeTrue_Test()
        {
            Vector v1 = new Vector(1, 0, 0);
            var result = v1.IsUnitVector(0.0002);

            Assert.AreEqual(true, result, "vector (1,0,0) is a unit vector");
        }

        [TestMethod, TestCategory("IsUnitVector")]
        public void IsUnitVectorWithTolerance_WhereTheZComponentIsNearlyOneByLessThanTheTolerance_ShouldBeTrue_Test()
        {
            Vector v1 = new Vector(0, 0, 0.9999);
            var result = v1.IsUnitVector(0.0002);

            // Quick test to ensure that the double minus operation does not introduce an unexpected margin of error that is higher than our margin of error
            Assert.IsTrue(((1d - 0.9999d)) < (0.0002d), "the double calculations of 1 minus 0.9999 should be less than 0.0002 or our test is incorrect");

            // Assert our acual test
            Assert.AreEqual(true, result, "the tolerance should have been greater than the difference between the z components and 1");
        }

        [TestMethod, TestCategory("IsUnitVector")]
        public void IsUnitVectorWithTolerance_WhereTheZComponentIsNearlyOneByMoreThanTheTolerance_ShouldBeFalse_Test()
        {
            Vector v1 = new Vector(0, 0, 0.9999);

            var result = v1.IsUnitVector(0.00009);

            // Quick test to ensure that the double minus operation does not introduce an unexpected margin of error
            Assert.IsTrue(((1d - 0.9999d)) > (0.00009d), "the double calculations of 3 minus 2.9999 should be greater than 0.00009 or our test is incorrect");

            Assert.AreEqual(false, result, "the tolerance should be less than the difference between the z component and 1");
        }

        #endregion

        #region Is Perpendicular Tests

        [TestMethod, TestCategory("IsPerpendicular")]
        public void IsPerpendicular_WhereUnitVectorsPositiveXY_ShouldBeTrue_Test()
        {
            Vector v1 = new Vector(1, 0, 0);
            Vector v2 = new Vector(0, 1, 0);
            var result = v1.IsPerpendicular(v2);

            Assert.AreEqual(true, result, "vector (1,0,0) is perpendicular to vector (0,1,0)");
        }

        [TestMethod, TestCategory("IsPerpendicular")]
        public void IsPerpendicular_WhereUnitVectorsInfinitePositiveXY_ShouldBeTrue_Test()
        {
            Vector v1 = new Vector(double.PositiveInfinity, 0, 0);
            Vector v2 = new Vector(0, double.PositiveInfinity, 0);
            var result = v1.IsPerpendicular(v2);

            Assert.AreEqual(true, result, "vector (+inf,0,0) is perpendicular to vector (0,+inf,0)");
        }

        [TestMethod, TestCategory("IsPerpendicular")]
        public void IsPerpendicular_WhereUnitVectorsPositiveYX_ShouldBeTrue_Test()
        {
            Vector v1 = new Vector(0, 1, 0);
            Vector v2 = new Vector(1, 0, 0);
            var result = v1.IsPerpendicular(v2);

            Assert.AreEqual(true, result, "vector (0,1,0) is perpendicular to vector (1,0,0)");
        }

        [TestMethod, TestCategory("IsPerpendicular")]
        public void IsPerpendicular_WhereUnitVectorsInfinitePositiveYX_ShouldBeTrue_Test()
        {
            Vector v1 = new Vector(0, double.PositiveInfinity, 0);
            Vector v2 = new Vector(double.PositiveInfinity, 0, 0);
            var result = v1.IsPerpendicular(v2);

            Assert.AreEqual(true, result, "vector (0,+inf,0) is perpendicular to vector (+inf,0,0)");
        }

        [TestMethod, TestCategory("IsPerpendicular")]
        public void IsPerpendicular_WhereUnitVectorsPositiveXNegativeY_ShouldBeTrue_Test()
        {
            Vector v1 = new Vector(1, 0, 0);
            Vector v2 = new Vector(0, -1, 0);
            var result = v1.IsPerpendicular(v2);

            Assert.AreEqual(true, result, "vector (1,0,0) is perpendicular to vector (0,-1,0)");
        }

        [TestMethod, TestCategory("IsPerpendicular")]
        public void IsPerpendicular_WhereUnitVectorsInfinitePositiveXNegativeY_ShouldBeTrue_Test()
        {
            Vector v1 = new Vector(double.PositiveInfinity, 0, 0);
            Vector v2 = new Vector(0, double.NegativeInfinity, 0);
            var result = v1.IsPerpendicular(v2);

            Assert.AreEqual(true, result, "vector (+inf,0,0) is perpendicular to vector (0,-inf,0)");
        }

        [TestMethod, TestCategory("IsPerpendicular")]
        public void IsPerpendicular_WhereUnitVectorsNegativeYPositiveX_ShouldBeTrue_Test()
        {
            Vector v1 = new Vector(0, -1, 0);
            Vector v2 = new Vector(1, 0, 0);
            var result = v1.IsPerpendicular(v2);

            Assert.AreEqual(true, result, "vector (0,-1,0) is perpendicular to vector (1,0,0)");
        }

        [TestMethod, TestCategory("IsPerpendicular")]
        public void IsPerpendicular_WhereUnitVectorsInfiniteNegativeYPositiveX_ShouldBeTrue_Test()
        {
            Vector v1 = new Vector(0, double.NegativeInfinity, 0);
            Vector v2 = new Vector(double.PositiveInfinity, 0, 0);
            var result = v1.IsPerpendicular(v2);

            Assert.AreEqual(true, result, "vector (0,-inf,0) is perpendicular to vector (+inf,0,0)");
        }

        [TestMethod, TestCategory("IsPerpendicular")]
        public void IsPerpendicular_WhereUnitVectorsNegativeXY_ShouldBeTrue_Test()
        {
            Vector v1 = new Vector(-1, 0, 0);
            Vector v2 = new Vector(0, -1, 0);
            var result = v1.IsPerpendicular(v2);

            Assert.AreEqual(true, result, "vector (-1,0,0) is perpendicular to vector (0,-1,0)");
        }

        [TestMethod, TestCategory("IsPerpendicular")]
        public void IsPerpendicular_WhereUnitVectorsInfiniteNegativeXY_ShouldBeTrue_Test()
        {
            Vector v1 = new Vector(double.NegativeInfinity, 0, 0);
            Vector v2 = new Vector(0, double.NegativeInfinity, 0);
            var result = v1.IsPerpendicular(v2);

            Assert.AreEqual(true, result, "vector (-inf,0,0) is perpendicular to vector (0,-inf,0)");
        }

        [TestMethod, TestCategory("IsPerpendicular")]
        public void IsPerpendicular_WhereUnitVectorsNegativeYX_ShouldBeTrue_Test()
        {
            Vector v1 = new Vector(0, -1, 0);
            Vector v2 = new Vector(-1, 0, 0);
            var result = v1.IsPerpendicular(v2);

            Assert.AreEqual(true, result, "vector (0,-1,0) is perpendicular to vector (-1,0,0)");
        }

        [TestMethod, TestCategory("IsPerpendicular")]
        public void IsPerpendicular_WhereUnitVectorsInfiniteNegativeYX_ShouldBeTrue_Test()
        {
            Vector v1 = new Vector(0, double.NegativeInfinity, 0);
            Vector v2 = new Vector(double.NegativeInfinity, 0, 0);
            var result = v1.IsPerpendicular(v2);

            Assert.AreEqual(true, result, "vector (0,-inf,0) is perpendicular to vector (-inf,0,0)");
        }

        [TestMethod, TestCategory("IsPerpendicular")]
        public void IsPerpendicular_WhereUnitVectorsNegativeXPositiveY_ShouldBeTrue_Test()
        {
            Vector v1 = new Vector(-1, 0, 0);
            Vector v2 = new Vector(0, 1, 0);
            var result = v1.IsPerpendicular(v2);

            Assert.AreEqual(true, result, "vector (-1,0,0) is perpendicular to vector (0,1,0)");
        }

        [TestMethod, TestCategory("IsPerpendicular")]
        public void IsPerpendicular_WhereUnitVectorsInfiniteNegativeXPositiveY_ShouldBeTrue_Test()
        {
            Vector v1 = new Vector(double.NegativeInfinity, 0, 0);
            Vector v2 = new Vector(0, double.PositiveInfinity, 0);
            var result = v1.IsPerpendicular(v2);

            Assert.AreEqual(true, result, "vector (-inf,0,0) is perpendicular to vector (0,+inf,0) but ");
        }

        [TestMethod, TestCategory("IsPerpendicular")]
        public void IsPerpendicular_WhereUnitVectorsPositiveYNegativeX_ShouldBeTrue_Test()
        {
            Vector v1 = new Vector(0, 1, 0);
            Vector v2 = new Vector(-1, 0, 0);
            var result = v1.IsPerpendicular(v2);

            Assert.AreEqual(true, result, "vector (0,1,0) is perpendicular to vector (-1,0,0)");
        }

        [TestMethod, TestCategory("IsPerpendicular")]
        public void IsPerpendicular_WhereUnitVectorsInfinitePositiveYNegativeX_ShouldBeTrue_Test()
        {
            Vector v1 = new Vector(0, double.PositiveInfinity, 0);
            Vector v2 = new Vector(double.NegativeInfinity, 0, 0);
            var result = v1.IsPerpendicular(v2);

            Assert.AreEqual(true, result, "vector (0,+inf,0) is perpendicular to vector (-inf,0,0)");
        }

        [TestMethod, TestCategory("IsPerpendicular")]
        public void IsPerpendicular_WherePositiveXY_ShouldBeTrue_Test()
        {
            Vector v1 = new Vector(ArbitaryTestDouble, 0, 0);
            Vector v2 = new Vector(0, ArbitaryTestDouble, 0);
            var result = v1.IsPerpendicular(v2);

            Assert.AreEqual(true, result, "vector (n,0,0) is perpendicular to vector (0,n,0)");
        }

        [TestMethod, TestCategory("IsPerpendicular")]
        public void IsPerpendicular_WherePositiveYX_ShouldBeTrue_Test()
        {
            Vector v1 = new Vector(0, ArbitaryTestDouble, 0);
            Vector v2 = new Vector(ArbitaryTestDouble, 0, 0);
            var result = v1.IsPerpendicular(v2);

            Assert.AreEqual(true, result, "vector (0,n,0) is perpendicular to vector (n,0,0)");
        }

        [TestMethod, TestCategory("IsPerpendicular")]
        public void IsPerpendicular_WherePositiveXNegativeY_ShouldBeTrue_Test()
        {
            Vector v1 = new Vector(ArbitaryTestDouble, 0, 0);
            Vector v2 = new Vector(0, -ArbitaryTestDouble, 0);
            var result = v1.IsPerpendicular(v2);

            Assert.AreEqual(true, result, "vector (n,0,0) is perpendicular to vector (0,-n,0)");
        }

        [TestMethod, TestCategory("IsPerpendicular")]
        public void IsPerpendicular_WhereUNegativeYPositiveX_ShouldBeTrue_Test()
        {
            Vector v1 = new Vector(0, -ArbitaryTestDouble, 0);
            Vector v2 = new Vector(ArbitaryTestDouble, 0, 0);
            var result = v1.IsPerpendicular(v2);

            Assert.AreEqual(true, result, "vector (0,-n,0) is perpendicular to vector (n,0,0)");
        }

        [TestMethod, TestCategory("IsPerpendicular")]
        public void IsPerpendicular_WhereNegativeXY_ShouldBeTrue_Test()
        {
            Vector v1 = new Vector(-ArbitaryTestDouble, 0, 0);
            Vector v2 = new Vector(0, -ArbitaryTestDouble, 0);
            var result = v1.IsPerpendicular(v2);

            Assert.AreEqual(true, result, "vector (-n,0,0) is perpendicular to vector (0,-n,0)");
        }

        [TestMethod, TestCategory("IsPerpendicular")]
        public void IsPerpendicular_WhereNegativeYX_ShouldBeTrue_Test()
        {
            Vector v1 = new Vector(0, -ArbitaryTestDouble, 0);
            Vector v2 = new Vector(-ArbitaryTestDouble, 0, 0);
            var result = v1.IsPerpendicular(v2);

            Assert.AreEqual(true, result, "vector (0,-n,0) is perpendicular to vector (-n,0,0)");
        }

        [TestMethod, TestCategory("IsPerpendicular")]
        public void IsPerpendicular_WhereNegativeXPositiveY_ShouldBeTrue_Test()
        {
            Vector v1 = new Vector(-ArbitaryTestDouble, 0, 0);
            Vector v2 = new Vector(0, ArbitaryTestDouble, 0);
            var result = v1.IsPerpendicular(v2);

            Assert.AreEqual(true, result, "vector (-n,0,0) is perpendicular to vector (0,n,0)");
        }

        [TestMethod, TestCategory("IsPerpendicular")]
        public void IsPerpendicular_WherePositiveYNegativeX_ShouldBeTrue_Test()
        {
            Vector v1 = new Vector(0, ArbitaryTestDouble, 0);
            Vector v2 = new Vector(-ArbitaryTestDouble, 0, 0);
            var result = v1.IsPerpendicular(v2);

            Assert.AreEqual(true, result, "vector (0,n,0) is perpendicular to vector (-n,0,0)");
        }

        [TestMethod, TestCategory("IsPerpendicular")]
        public void IsPerpendicular_WherePositiveXYAnd45deg_ShouldBeFalse_Test()
        {
            Vector v1 = new Vector(1, 0, 0);
            Vector v2 = new Vector(1, 1, 0);
            var result = v1.IsPerpendicular(v2);

            Assert.AreEqual(false, result, "vector (1,0,0) is not perpendicular to vector (1,1,0)");
        }

        [TestMethod, TestCategory("IsPerpendicular")]
        public void IsPerpendicular_WhereInfinitePositiveXYAnd45deg_ShouldBeFalse_Test()
        {
            Vector v1 = new Vector(double.PositiveInfinity, 0, 0);
            Vector v2 = new Vector(double.PositiveInfinity, double.PositiveInfinity, 0);
            var result = v1.IsPerpendicular(v2);

            Assert.AreEqual(false, result, "vector (+inf,0,0) is not perpendicular to vector (+inf,+inf,0)");
        }

        [TestMethod, TestCategory("IsPerpendicular")]
        public void IsPerpendicular_WherePositiveXAndNegativeXYAnd45deg_ShouldBeFalse_Test()
        {
            Vector v1 = new Vector(0, 1, 0);
            Vector v2 = new Vector(-1, -1, 0);
            var result = v1.IsPerpendicular(v2);

            Assert.AreEqual(false, result, "vector (1,0,0) is not perpendicular to vector (-1,-1,0)");
        }

        [TestMethod, TestCategory("IsPerpendicular")]
        public void IsPerpendicular_WhereInfinitePositiveXAndNegativeXYAnd45deg_ShouldBeFalse_Test()
        {
            Vector v1 = new Vector(0, double.PositiveInfinity, 0);
            Vector v2 = new Vector(double.NegativeInfinity, double.NegativeInfinity, 0);
            var result = v1.IsPerpendicular(v2);

            Assert.AreEqual(false, result, "vector (+inf,0,0) is not perpendicular to vector (-inf,-inf,0)");
        }

        [TestMethod, TestCategory("IsPerpendicular")]
        public void IsPerpendicular_WhereZeroAndZero_ShouldBeFalse_Test()
        {
            Vector v1 = new Vector(0, 0, 0);
            Vector v2 = new Vector(0, 0, 0);
            var result = v1.IsPerpendicular(v2);

            Assert.AreEqual(false, result, "vector (0,0,0) is not perpendicular to vector (0,0,0)");
        }

        [TestMethod, TestCategory("IsPerpendicular")]
        public void IsPerpendicularWithTolerance_WhereUnitVectorsPositiveXYAndThereIsNoError_ShouldBeTrue_Test()
        {
            Vector v1 = new Vector(0, 1, 0);
            Vector v2 = new Vector(1, 0, 0); // with an error in Y that should produce a slightly off 90deg angle (as radians)

            var result = v1.IsPerpendicular(v2, 0.0002); // remember the tolerance should be for the angle in Rad compared to 90deg (as a rad)

            Assert.AreEqual(true, result, "vector (0,1,0) is perpendicular to vector (1,0,0)");
        }

        [TestMethod, TestCategory("IsPerpendicular")]
        public void IsPerpendicularWithTolerance_WhereUnitVectorsInfinitePositiveXYAndThereIsNoError_ShouldBeTrue_Test()
        {
            Vector v1 = new Vector(0, double.PositiveInfinity, 0);
            Vector v2 = new Vector(double.PositiveInfinity, 0, 0); // with an error in Y that should produce a slightly off 90deg angle (as radians)

            var result = v1.IsPerpendicular(v2, 0.0002); // remember the tolerance should be for the angle in Rad compared to 90deg (as a rad)

            Assert.AreEqual(true, result, "vector (0,+inf,0) is perpendicular to vector (+inf,0,0)");
        }

        [TestMethod, TestCategory("IsPerpendicular")]
        public void IsPerpendicularWithTolerance_WhereUnitVectorsPositiveXYAndThereIsAnError_ShouldBeTrue_Test()
        {
            Vector v1 = new Vector(0, 1, 0);
            Vector v2 = new Vector(1, 0.001, 0); // with an error in Y that should produce a slightly off 90deg angle (as radians)

            var result = v1.IsPerpendicular(v2, 0.001); // remember the tolerance should be for the angle in Rad compared to 90deg (as a rad)

            // Check the tolerance on the angle
            var angle = v1.Angle(v2);
            Assert.AreEqual(true, angle.AlmostEqualsWithAbsTolerance(Deg90AsRad, 0.001), string.Format("the angle between v1 and v2 should be 90 deg within 0.001 radians (found {0} rad)", angle));

            // Check the actual result
            Assert.AreEqual(true, result, "vector (0,1,0) is perpendicular to vector (1,0.001,0)");
        }

        /// <summary>
        /// Check that the special case vectors use the tolerance when determinitng if it is a apecial case.
        /// Ignored: This should not work! The tolerance is for the dot product equality to 0 NOT for special case snap to zero.
        /// We could add another parameter, i.e. SpecialCaseSnapToZeroTolerance but we are making special cases more complicated.
        /// </summary>
        [TestMethod, TestCategory("IsPerpendicular"), Ignore]
        public void IsPerpendicularWithTolerance_WhereUnitVectorsInfinitePositiveXYAndThereIsAnError_ShouldBeTrue_Test()
        {
            Vector v1 = new Vector(0, double.PositiveInfinity, 0);
            Vector v2 = new Vector(double.PositiveInfinity, 0.001, 0); // with an error in Y that should produce a slightly off 90deg angle (as radians)

            var result = v1.IsPerpendicular(v2, 0.001); // remember the tolerance should be for the angle in Rad compared to 90deg (as a rad)

            // Check the tolerance on the angle after normalization
            var angle = v1.NormalizeOrDefault().Angle(v2.NormalizeOrDefault());
            Assert.AreEqual(true, angle.AlmostEqualsWithAbsTolerance(Deg90AsRad, 0.001), string.Format("the angle between v1 and v2 should be 90 deg within 0.001 radians (found {0} rad)", angle));

            // Check the actual result
            Assert.AreEqual(true, result, "vector (0,+inf,0) is perpendicular to vector (+inf,0.001,0)");
        }

        [TestMethod, TestCategory("IsPerpendicular")]
        public void IsPerpendicular_WhereOneVectorsXIsNaNAndTheOtherVectorsYIs1_ShouldBeFalse_Test()
        {
            Vector v1 = new Vector(double.NaN, 0, 0);
            Vector v2 = new Vector(0, 1, 0);
            var result = v1.IsPerpendicular(v2);

            Assert.AreEqual(false, result, "vector (NaN,0,0) is not perpendicular to vector (0,1,0)");
        }

        [TestMethod, TestCategory("IsPerpendicular")]
        public void IsPerpendicular_WhereOneVectorsXIsNaNAndTheOtherVectorsYIsNaN_ShouldBeFalse_Test()
        {
            Vector v1 = new Vector(double.NaN, 0, 0);
            Vector v2 = new Vector(0, double.NaN, 0);
            var result = v1.IsPerpendicular(v2);

            Assert.AreEqual(false, result, "vector (NaN,0,0) is not perpendicular to vector (0,NaN,0)");
        }

        [TestMethod, TestCategory("IsPerpendicular")]
        public void IsPerpendicularWithTolerance_WhereOneVectorsXIsNaNAndTheOtherVectorsYIs1_ShouldBeFalse_Test()
        {
            Vector v1 = new Vector(double.NaN, 0, 0);
            Vector v2 = new Vector(0, 1, 0);
            var result = v1.IsPerpendicular(v2, 1);

            Assert.AreEqual(false, result, "vector (NaN,0,0) is not perpendicular to vector (0,1,0) regardless of tolerance");
        }

        [TestMethod, TestCategory("IsPerpendicular")]
        public void IsPerpendicularWithTolerance_WhereOneVectorsXIsNaNAndTheOtherVectorsYIsNaN_ShouldBeFalse_Test()
        {
            Vector v1 = new Vector(double.NaN, 0, 0);
            Vector v2 = new Vector(0, double.NaN, 0);
            var result = v1.IsPerpendicular(v2, 1);

            Assert.AreEqual(false, result, "vector (NaN,0,0) is not perpendicular to vector (0,NaN,0) regardless of tolerance");
        }

        [TestMethod, TestCategory("IsPerpendicular")]
        public void IsPerpendicularWithTolerance_WhereZeroAndZero_ShouldBeFalse_Test()
        {
            Vector v1 = new Vector(0, 0, 0);
            Vector v2 = new Vector(0, 0, 0);
            var result = v1.IsPerpendicular(v2, 1);

            Assert.AreEqual(false, result, "vector (0,0,0) is not perpendicular to vector (0,0,0) regardless of tolerance");
        }

        #endregion

        #region IsNaN

        [TestMethod, TestCategory("IsNaN")]
        public void IsNaN_WithXIsNaN_ShouldBeTrue_Test()
        {
            Vector v1 = new Vector(double.NaN, 0, 0);
            var result = v1.IsNaN();

            Assert.AreEqual(true, result);
        }

        [TestMethod, TestCategory("IsNaN")]
        public void IsNaN_WithYIsNaN_ShouldBeTrue_Test()
        {
            Vector v1 = new Vector(0, double.NaN, 0);
            var result = v1.IsNaN();

            Assert.AreEqual(true, result);
        }

        [TestMethod, TestCategory("IsNaN")]
        public void IsNaN_WithZIsNaN_ShouldBeTrue_Test()
        {
            Vector v1 = new Vector(0, 0, double.NaN);
            var result = v1.IsNaN();

            Assert.AreEqual(true, result);
        }

        [TestMethod, TestCategory("IsNaN")]
        public void IsNaN_WithYZIsNaN_ShouldBeTrue_Test()
        {
            Vector v1 = new Vector(0, double.NaN, double.NaN);
            var result = v1.IsNaN();

            Assert.AreEqual(true, result);
        }

        [TestMethod, TestCategory("IsNaN")]
        public void IsNaN_WithXYZIsNaN_ShouldBeTrue_Test()
        {
            Vector v1 = new Vector(double.NaN, double.NaN, double.NaN);
            var result = v1.IsNaN();

            Assert.AreEqual(true, result);
        }

        [TestMethod, TestCategory("IsNaN")]
        public void IsNaN_WithXYZWholeNumbers_ShouldBeFalse_Test()
        {
            Vector v1 = new Vector(1, 2, 3);
            var result = v1.IsNaN();

            Assert.AreEqual(false, result);
        }

        #endregion
    }
}
