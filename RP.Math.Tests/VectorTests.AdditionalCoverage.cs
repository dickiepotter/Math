namespace RP.Math.Tests
{
    using System;
    using System.Globalization;


    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using RP.Math;

    /// <summary>
    /// Additional coverage for public members that previously had no unit tests:
    /// Interpolate, Min/Max, Distance, MixedProduct, IsBackFace, the component sums,
    /// the indexer / Array accessor, the scalar operators and the string formatters.
    /// </summary>
    public partial class VectorTests
    {
        #region Interpolate Tests

        [TestMethod, TestCategory("Interpolate")]
        public void Interpolate_AtControlZero_ShouldReturnTheFirstVector_Test()
        {
            var a = new Vector(1, 2, 3);
            var b = new Vector(7, 8, 9);

            var result = Vector.Interpolate(a, b, 0);

            Assert.AreEqual(a, result);
        }

        [TestMethod, TestCategory("Interpolate")]
        public void Interpolate_AtControlOne_ShouldReturnTheSecondVector_Test()
        {
            var a = new Vector(1, 2, 3);
            var b = new Vector(7, 8, 9);

            var result = Vector.Interpolate(a, b, 1);

            Assert.AreEqual(b, result);
        }

        [TestMethod, TestCategory("Interpolate")]
        public void Interpolate_AtControlHalf_ShouldReturnTheMidpoint_Test()
        {
            var a = new Vector(0, 0, 0);
            var b = new Vector(4, 8, 12);

            var result = a.Interpolate(b, 0.5);

            Assert.AreEqual(2, result.X);
            Assert.AreEqual(4, result.Y);
            Assert.AreEqual(6, result.Z);
        }

        [TestMethod, TestCategory("Interpolate")]
        public void Interpolate_WithControlAboveOneAndNoExtrapolation_ShouldThrow_Test()
        {
            var a = new Vector(0, 0, 0);
            var b = new Vector(2, 2, 2);

            Action act = () => Vector.Interpolate(a, b, 1.5);

            Assert.ThrowsException<ArgumentOutOfRangeException>(act);
        }

        [TestMethod, TestCategory("Interpolate")]
        public void Interpolate_StaticWithControlAboveOneAndExtrapolationAllowed_ShouldExtrapolate_Test()
        {
            var a = new Vector(0, 0, 0);
            var b = new Vector(2, 2, 2);

            var result = Vector.Interpolate(a, b, 1.5, true);

            Assert.AreEqual(3, result.X, "extrapolating to t = 1.5 along (0,0,0)->(2,2,2) gives (3,3,3)");
            Assert.AreEqual(3, result.Y);
            Assert.AreEqual(3, result.Z);
        }

        [TestMethod, TestCategory("Interpolate")]
        public void Interpolate_InstanceWithControlAboveOneAndExtrapolationAllowed_ShouldExtrapolate_Test()
        {
            var a = new Vector(0, 0, 0);
            var b = new Vector(2, 2, 2);

            // The instance overload should honour allowExtrapolation just like the static one.
            var result = a.Interpolate(b, 1.5, true);

            Assert.AreEqual(3, result.X);
            Assert.AreEqual(3, result.Y);
            Assert.AreEqual(3, result.Z);
        }

        #endregion

        #region Min and Max Tests

        [TestMethod, TestCategory("MinMax")]
        public void Max_ShouldReturnTheVectorWithTheGreaterMagnitude_Test()
        {
            var shorter = new Vector(1, 0, 0);   // magnitude 1
            var longer = new Vector(3, 4, 0);    // magnitude 5

            Assert.AreEqual(longer, Vector.Max(shorter, longer));
            Assert.AreEqual(longer, longer.Max(shorter));
        }

        [TestMethod, TestCategory("MinMax")]
        public void Min_ShouldReturnTheVectorWithTheLesserMagnitude_Test()
        {
            var shorter = new Vector(1, 0, 0);   // magnitude 1
            var longer = new Vector(3, 4, 0);    // magnitude 5

            Assert.AreEqual(shorter, Vector.Min(shorter, longer));
            Assert.AreEqual(shorter, longer.Min(shorter));
        }

        #endregion

        #region Distance Tests

        [TestMethod, TestCategory("Distance")]
        public void Distance_BetweenTwoVectors_ShouldApplyPythagoras_Test()
        {
            var a = new Vector(0, 0, 0);
            var b = new Vector(3, 4, 0);

            Assert.AreEqual(5, Vector.Distance(a, b));
            Assert.AreEqual(5, a.Distance(b));
        }

        [TestMethod, TestCategory("Distance")]
        public void Distance_FromAVectorToItself_ShouldBeZero_Test()
        {
            var a = new Vector(7, -2, 5);

            Assert.AreEqual(0, a.Distance(a));
        }

        #endregion

        #region Mixed (triple) Product Tests

        [TestMethod, TestCategory("Product")]
        public void MixedProduct_OfTheUnitAxes_ShouldBeOne_Test()
        {
            // X . (Y x Z) == 1 : the signed unit volume of the standard basis.
            Assert.AreEqual(1, Vector.MixedProduct(Vector.XAxis, Vector.YAxis, Vector.ZAxis));
            Assert.AreEqual(1, Vector.XAxis.MixedProduct(Vector.YAxis, Vector.ZAxis));
        }

        [TestMethod, TestCategory("Product")]
        public void MixedProduct_OfCoplanarVectors_ShouldBeZero_Test()
        {
            var a = new Vector(1, 0, 0);
            var b = new Vector(0, 1, 0);
            var c = new Vector(1, 1, 0); // lies in the XY plane with a and b

            Assert.AreEqual(0, Vector.MixedProduct(a, b, c), "coplanar vectors span no volume");
        }

        #endregion

        #region IsBackFace Tests

        [TestMethod, TestCategory("BackFace")]
        public void IsBackFace_WhereNormalFacesAwayFromTheLineOfSight_ShouldBeTrue_Test()
        {
            var normal = new Vector(0, 0, 1);
            var lineOfSight = new Vector(0, 0, -1); // normal . sight = -1 < 0

            Assert.IsTrue(Vector.IsBackFace(normal, lineOfSight));
            Assert.IsTrue(normal.IsBackFace(lineOfSight));
        }

        [TestMethod, TestCategory("BackFace")]
        public void IsBackFace_WhereNormalFacesTowardsTheLineOfSight_ShouldBeFalse_Test()
        {
            var normal = new Vector(0, 0, 1);
            var lineOfSight = new Vector(0, 0, 1); // normal . sight = 1, not < 0

            Assert.IsFalse(normal.IsBackFace(lineOfSight));
        }

        #endregion

        #region Component Sum Tests

        [TestMethod, TestCategory("Components")]
        public void SumComponents_ShouldAddTheThreeComponents_Test()
        {
            Assert.AreEqual(9, new Vector(2, 3, 4).SumComponents());
        }

        [TestMethod, TestCategory("Components")]
        public void SumComponentSqrs_ShouldAddTheSquaresOfTheComponents_Test()
        {
            Assert.AreEqual(29, new Vector(2, 3, 4).SumComponentSqrs()); // 4 + 9 + 16
        }

        #endregion

        #region Indexer and Array Accessor Tests

        [TestMethod, TestCategory("Accessors")]
        public void Indexer_ShouldMapZeroOneTwoToXYZ_Test()
        {
            var vector = new Vector(10, 20, 30);

            Assert.AreEqual(10, vector[0]);
            Assert.AreEqual(20, vector[1]);
            Assert.AreEqual(30, vector[2]);
        }

        [TestMethod, TestCategory("Accessors")]
        public void Indexer_WithAnOutOfRangeIndex_ShouldThrow_Test()
        {
            var vector = new Vector(10, 20, 30);

            Action act = () => { var unused = vector[3]; };

            Assert.ThrowsException<ArgumentException>(act);
        }

        [TestMethod, TestCategory("Accessors")]
        public void Array_ShouldReturnTheComponentsInXYZOrder_Test()
        {
            var vector = new Vector(10, 20, 30);

            CollectionAssert.AreEqual(new[] { 10d, 20d, 30d }, vector.Array);
        }

        #endregion

        #region Scalar Operator Tests

        [TestMethod, TestCategory("Operator")]
        public void MultiplyByScalar_ShouldBeCommutativeAndScaleEachComponent_Test()
        {
            var vector = new Vector(1, 2, 3);

            Assert.AreEqual(new Vector(2, 4, 6), (vector * 2));
            Assert.AreEqual(new Vector(2, 4, 6), (2 * vector), "scalar multiplication is commutative");
        }

        [TestMethod, TestCategory("Operator")]
        public void DivideByScalar_ShouldDivideEachComponent_Test()
        {
            var vector = new Vector(2, 4, 6);

            var result = vector / 2;

            Assert.AreEqual(1, result.X);
            Assert.AreEqual(2, result.Y);
            Assert.AreEqual(3, result.Z);
        }

        #endregion

        #region ToString Tests

        [TestMethod, TestCategory("Formatting")]
        public void ToString_WithNoFormat_ShouldListTheComponents_Test()
        {
            Assert.AreEqual("(1, 2, 3)", new Vector(1, 2, 3).ToString());
        }

        [TestMethod, TestCategory("Formatting")]
        public void ToString_WithAComponentSelector_ShouldReturnThatComponent_Test()
        {
            var vector = new Vector(1, 2, 3);

            Assert.AreEqual("1", vector.ToString("x", CultureInfo.InvariantCulture));
            Assert.AreEqual("2", vector.ToString("y", CultureInfo.InvariantCulture));
            Assert.AreEqual("3", vector.ToString("z", CultureInfo.InvariantCulture));
        }

        [TestMethod, TestCategory("Formatting")]
        public void ToVerbString_ShouldDescribeTheComponents_Test()
        {
            var description = new Vector(1, 2, 3).ToVerbString();

            StringAssert.Contains(description, "x=1");
            StringAssert.Contains(description, "y=2");
            StringAssert.Contains(description, "z=3");
        }

        #endregion

        #region Offset (arbitrary line) Rotation Tests

        // These cover the RotateX/Y/Z overloads with NON-ZERO offsets, i.e. rotation about a line
        // parallel to an axis but passing through the offset point (the zero-offset case is already
        // covered in the main Rotation Tests region).

        [TestMethod, TestCategory("Rotation")]
        public void RotateX_WithOffsets_ShouldOrbitTheOffsetLine_Test()
        {
            // Origin rotated 90° about the line parallel to X through (y=1, z=0).
            var result = Vector.RotateX(new Vector(0, 0, 0), 1, 0, Deg90AsRad);

            Assert.AreEqual(0, Math.Round(result.X, 6));
            Assert.AreEqual(1, Math.Round(result.Y, 6));
            Assert.AreEqual(-1, Math.Round(result.Z, 6));
        }

        [TestMethod, TestCategory("Rotation")]
        public void RotateY_WithOffsets_ShouldOrbitTheOffsetLine_Test()
        {
            // Origin rotated 90° about the line parallel to Y through (x=1, z=0).
            var result = Vector.RotateY(new Vector(0, 0, 0), 1, 0, Deg90AsRad);

            Assert.AreEqual(1, Math.Round(result.X, 6));
            Assert.AreEqual(0, Math.Round(result.Y, 6));
            Assert.AreEqual(1, Math.Round(result.Z, 6));
        }

        [TestMethod, TestCategory("Rotation")]
        public void RotateZ_WithOffsets_ShouldOrbitTheOffsetLine_Test()
        {
            // Origin rotated 90° about the line parallel to Z through (x=1, y=0).
            var result = Vector.RotateZ(new Vector(0, 0, 0), 1, 0, Deg90AsRad);

            Assert.AreEqual(1, Math.Round(result.X, 6));
            Assert.AreEqual(-1, Math.Round(result.Y, 6));
            Assert.AreEqual(0, Math.Round(result.Z, 6));
        }

        [TestMethod, TestCategory("Rotation")]
        public void RotateX_WhereThePointLiesOnTheOffsetLine_ShouldNotMove_Test()
        {
            // (·, 2, 3) lies on the line parallel to X through (y=2, z=3), so it is unchanged.
            var result = new Vector(5, 2, 3).RotateX(2, 3, Deg90AsRad);

            Assert.AreEqual(5, Math.Round(result.X, 6));
            Assert.AreEqual(2, Math.Round(result.Y, 6));
            Assert.AreEqual(3, Math.Round(result.Z, 6));
        }

        [TestMethod, TestCategory("Rotation")]
        public void RotateY_WhereThePointLiesOnTheOffsetLine_ShouldNotMove_Test()
        {
            // (5, ·, 3) lies on the line parallel to Y through (x=5, z=3), so it is unchanged.
            var result = new Vector(5, 2, 3).RotateY(5, 3, Deg90AsRad);

            Assert.AreEqual(5, Math.Round(result.X, 6));
            Assert.AreEqual(2, Math.Round(result.Y, 6));
            Assert.AreEqual(3, Math.Round(result.Z, 6));
        }

        [TestMethod, TestCategory("Rotation")]
        public void RotateZ_WhereThePointLiesOnTheOffsetLine_ShouldNotMove_Test()
        {
            // (3, 4, ·) lies on the line parallel to Z through (x=3, y=4), so it is unchanged.
            var result = new Vector(3, 4, 7).RotateZ(3, 4, Deg90AsRad);

            Assert.AreEqual(3, Math.Round(result.X, 6));
            Assert.AreEqual(4, Math.Round(result.Y, 6));
            Assert.AreEqual(7, Math.Round(result.Z, 6));
        }

        #endregion

        #region Reflect-about-normal Tests

        [TestMethod, TestCategory("Reflect")]
        public void Reflect_OffAFlatSurface_ShouldMirrorThroughTheNormal_Test()
        {
            // A downward velocity bouncing off a floor whose normal points up.
            var velocity = new Vector(1, -1, 0);
            var up = new Vector(0, 1, 0);

            var result = Vector.Reflect(velocity, up);

            Assert.AreEqual(1, result.X);
            Assert.AreEqual(1, result.Y, "the component along the normal is reversed");
            Assert.AreEqual(0, result.Z);

            Assert.AreEqual(result, velocity.Reflect(up));
        }

        [TestMethod, TestCategory("Reflect")]
        public void Reflect_WithANonUnitNormal_ShouldStillNormaliseIt_Test()
        {
            var velocity = new Vector(1, -1, 0);
            var up = new Vector(0, 5, 0); // not unit length

            var result = Vector.Reflect(velocity, up);

            Assert.AreEqual(1, result.X);
            Assert.AreEqual(1, result.Y);
            Assert.AreEqual(0, result.Z);
        }

        #endregion

        #region Componentwise Min, Max and Clamp Tests

        [TestMethod, TestCategory("Componentwise")]
        public void ComponentMin_ShouldTakeTheSmallerOfEachComponent_Test()
        {
            var result = new Vector(1, 5, 3).ComponentMin(new Vector(4, 2, 6));

            Assert.AreEqual(new Vector(1, 2, 3), result);
        }

        [TestMethod, TestCategory("Componentwise")]
        public void ComponentMax_ShouldTakeTheLargerOfEachComponent_Test()
        {
            var result = new Vector(1, 5, 3).ComponentMax(new Vector(4, 2, 6));

            Assert.AreEqual(new Vector(4, 5, 6), result);
        }

        [TestMethod, TestCategory("Componentwise")]
        public void Clamp_ShouldConstrainEachComponentToTheRange_Test()
        {
            var result = new Vector(5, -5, 2).Clamp(new Vector(0, 0, 0), new Vector(3, 3, 3));

            Assert.AreEqual(new Vector(3, 0, 2), result);
        }

        #endregion

        #region Distance-squared, Clamp-magnitude and Move-towards Tests

        [TestMethod, TestCategory("Distance")]
        public void DistanceSquared_ShouldBeTheSquareOfTheDistance_Test()
        {
            Assert.AreEqual(25, Vector.DistanceSquared(new Vector(0, 0, 0), new Vector(3, 4, 0)));
        }

        [TestMethod, TestCategory("ClampMagnitude")]
        public void ClampMagnitude_WhereMagnitudeExceedsTheMax_ShouldShortenInTheSameDirection_Test()
        {
            var result = new Vector(3, 4, 0).ClampMagnitude(2.5); // magnitude 5 -> 2.5

            Assert.AreEqual(1.5, result.X);
            Assert.AreEqual(2, result.Y);
            Assert.AreEqual(0, result.Z);
            Assert.AreEqual(2.5, result.Magnitude);
        }

        [TestMethod, TestCategory("ClampMagnitude")]
        public void ClampMagnitude_WhereMagnitudeIsWithinTheMax_ShouldBeUnchanged_Test()
        {
            var vector = new Vector(3, 4, 0); // magnitude 5

            Assert.AreEqual(vector, vector.ClampMagnitude(10));
        }

        [TestMethod, TestCategory("MoveTowards")]
        public void MoveTowards_WithASmallStep_ShouldMovePartWay_Test()
        {
            var result = new Vector(0, 0, 0).MoveTowards(new Vector(10, 0, 0), 3);

            Assert.AreEqual(new Vector(3, 0, 0), result);
        }

        [TestMethod, TestCategory("MoveTowards")]
        public void MoveTowards_WithAStepBeyondTheTarget_ShouldStopOnTheTarget_Test()
        {
            var target = new Vector(10, 0, 0);

            Assert.AreEqual(target, new Vector(0, 0, 0).MoveTowards(target, 100));
        }

        #endregion

        #region Slerp Tests

        [TestMethod, TestCategory("Slerp")]
        public void Slerp_HalfwayBetweenTwoPerpendicularUnitVectors_ShouldBeOnTheArc_Test()
        {
            var result = Vector.XAxis.Slerp(Vector.YAxis, 0.5);

            var expected = Math.Sqrt(0.5); // ~0.70710678
            Assert.AreEqual(Math.Round(expected, 6), Math.Round(result.X, 6));
            Assert.AreEqual(Math.Round(expected, 6), Math.Round(result.Y, 6));
            Assert.AreEqual(0, Math.Round(result.Z, 6));
            Assert.AreEqual(1, Math.Round(result.Magnitude, 6), "slerp between unit vectors stays on the unit sphere");
        }

        [TestMethod, TestCategory("Slerp")]
        public void Slerp_AtTheEndpoints_ShouldReturnTheEndpoints_Test()
        {
            Assert.AreEqual(Vector.XAxis, Vector.XAxis.Slerp(Vector.YAxis, 0));
            Assert.AreEqual(Vector.YAxis, Vector.XAxis.Slerp(Vector.YAxis, 1));
        }

        [TestMethod, TestCategory("Slerp")]
        public void Slerp_BetweenParallelVectors_ShouldFallBackToLinearInterpolation_Test()
        {
            var result = new Vector(1, 0, 0).Slerp(new Vector(3, 0, 0), 0.5);

            Assert.AreEqual(2, result.X);
            Assert.AreEqual(0, result.Y);
            Assert.AreEqual(0, result.Z);
        }

        #endregion

        #region Squared magnitude, IsZero, Deconstruct and tuple Tests

        [TestMethod, TestCategory("Components")]
        public void MagnitudeSquared_ShouldEqualTheSumOfTheSquares_Test()
        {
            Assert.AreEqual(25, new Vector(3, 4, 0).MagnitudeSquared);
        }

        [TestMethod, TestCategory("Predicate")]
        public void IsZero_ForTheZeroVector_ShouldBeTrue_Test()
        {
            Assert.IsTrue(new Vector(0, 0, 0).IsZero());
            Assert.IsFalse(new Vector(0, 1, 0).IsZero());
        }

        [TestMethod, TestCategory("Predicate")]
        public void IsZero_WithinTolerance_ShouldTreatTinyVectorsAsZero_Test()
        {
            Assert.IsTrue(new Vector(0.05, 0, 0).IsZero(0.1));
            Assert.IsFalse(new Vector(0.5, 0, 0).IsZero(0.1));
        }

        [TestMethod, TestCategory("Conversion")]
        public void Deconstruct_ShouldYieldTheComponents_Test()
        {
            var (x, y, z) = new Vector(1, 2, 3);

            Assert.AreEqual(1, x);
            Assert.AreEqual(2, y);
            Assert.AreEqual(3, z);
        }

        [TestMethod, TestCategory("Conversion")]
        public void ImplicitTupleConversions_ShouldRoundTrip_Test()
        {
            Vector fromTuple = (1d, 2d, 3d);
            Assert.AreEqual(new Vector(1, 2, 3), fromTuple);

            (double x, double y, double z) toTuple = new Vector(4, 5, 6);
            Assert.AreEqual((4d, 5d, 6d), toTuple);
        }

        #endregion
    }
}
