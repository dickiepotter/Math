namespace RP.Math.Tests
{
    using System;


    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using RP.Math.Exceptions;

    /// <summary>
    /// Unit tests for the <see cref="Quaternion"/> type.
    /// </summary>
    [TestClass]
    public class QuaternionTests
    {
        private const double Tolerance = 1e-9;
        private const double LooseTolerance = 1e-6;

        #region Construction

        [TestMethod, TestCategory("Construction")]
        public void Construction_WithComponents_ShouldExposeThem_Test()
        {
            var q = new Quaternion(1, 2, 3, 4);
            Assert.AreEqual(1, q.X);
            Assert.AreEqual(2, q.Y);
            Assert.AreEqual(3, q.Z);
            Assert.AreEqual(4, q.W);
        }

        [TestMethod, TestCategory("Construction")]
        public void Construction_FromVectorAndScalar_ShouldSplitTheParts_Test()
        {
            var q = new Quaternion(new Vector(1, 2, 3), 4);
            Assert.AreEqual(new Vector(1, 2, 3), q.Xyz);
            Assert.AreEqual(4, q.Scalar);
        }

        [TestMethod, TestCategory("Construction")]
        public void Construction_FromArrayOfWrongLength_ShouldThrow_Test()
        {
            Action act = () => new Quaternion(new double[] { 1, 2, 3 });
            Assert.ThrowsException<ArgumentException>(act);
        }

        [TestMethod, TestCategory("Construction")]
        public void Identity_ShouldBeZeroVectorWithScalarOne_Test()
        {
            Assert.AreEqual(new Quaternion(0, 0, 0, 1), Quaternion.Identity);
        }

        #endregion

        #region Magnitude

        [TestMethod, TestCategory("Magnitude")]
        public void Magnitude_OfIdentity_ShouldBeOne_Test()
        {
            Assert.AreEqual(1, Quaternion.Identity.Magnitude, Tolerance);
        }

        [TestMethod, TestCategory("Magnitude")]
        public void MagnitudeSquared_ShouldBeSumOfSquares_Test()
        {
            Assert.AreEqual(30, new Quaternion(1, 2, 3, 4).MagnitudeSquared, Tolerance);
        }

        #endregion

        #region Operators and products

        [TestMethod, TestCategory("Operator")]
        public void Addition_ShouldBeComponentWise_Test()
        {
            var sum = new Quaternion(1, 2, 3, 4) + new Quaternion(5, 6, 7, 8);
            Assert.AreEqual(new Quaternion(6, 8, 10, 12), sum);
        }

        [TestMethod, TestCategory("Operator")]
        public void ScalarMultiplication_ShouldScaleEachComponent_Test()
        {
            Assert.AreEqual(new Quaternion(2, 4, 6, 8), (new Quaternion(1, 2, 3, 4) * 2));
        }

        [TestMethod, TestCategory("Product")]
        public void IdentityProduct_ShouldLeaveQuaternionUnchanged_Test()
        {
            var q = new Quaternion(1, 2, 3, 4);
            Assert.IsTrue((q * Quaternion.Identity).Equals(q, Tolerance));
            Assert.IsTrue((Quaternion.Identity * q).Equals(q, Tolerance));
        }

        [TestMethod, TestCategory("Product")]
        public void HamiltonProduct_ShouldFollowBasisRules_Test()
        {
            // i * j = k : (1,0,0,0) * (0,1,0,0) = (0,0,1,0)
            var i = new Quaternion(1, 0, 0, 0);
            var j = new Quaternion(0, 1, 0, 0);
            Assert.IsTrue((i * j).Equals(new Quaternion(0, 0, 1, 0), Tolerance));
        }

        [TestMethod, TestCategory("Product")]
        public void DotProduct_ShouldBeSumOfComponentProducts_Test()
        {
            Assert.AreEqual(70, Quaternion.DotProduct(new Quaternion(1, 2, 3, 4), new Quaternion(5, 6, 7, 8)), Tolerance);
        }

        #endregion

        #region Conjugate, inverse, normalize

        [TestMethod, TestCategory("Conjugate")]
        public void Conjugate_ShouldNegateTheVectorPart_Test()
        {
            Assert.AreEqual(new Quaternion(-1, -2, -3, 4), new Quaternion(1, 2, 3, 4).Conjugate());
        }

        [TestMethod, TestCategory("Inverse")]
        public void Inverse_TimesOriginal_ShouldBeIdentity_Test()
        {
            var q = new Quaternion(1, 2, 3, 4);
            Assert.IsTrue((q * q.Inverse()).Equals(Quaternion.Identity, LooseTolerance));
        }

        [TestMethod, TestCategory("Normalize")]
        public void Normalize_ShouldProduceUnitQuaternion_Test()
        {
            Assert.IsTrue(new Quaternion(1, 2, 3, 4).Normalize().IsUnit(Tolerance));
        }

        [TestMethod, TestCategory("Normalize")]
        public void Normalize_OfZero_ShouldThrow_Test()
        {
            Action act = () => Quaternion.Zero.Normalize();
            Assert.ThrowsException<NormalizeQuaternionException>(act);
        }

        [TestMethod, TestCategory("Normalize")]
        public void NormalizeOrDefault_OfZero_ShouldReturnIdentity_Test()
        {
            Assert.AreEqual(Quaternion.Identity, Quaternion.Zero.NormalizeOrDefault());
        }

        #endregion

        #region Rotation

        [TestMethod, TestCategory("Rotation")]
        public void FromAxisAngle_ShouldProduceUnitQuaternion_Test()
        {
            Assert.IsTrue(Quaternion.FromAxisAngle(new Vector(0, 0, 1), System.Math.PI / 2)
                .IsUnit(Tolerance));
        }

        [TestMethod, TestCategory("Rotation")]
        public void Rotate_NinetyDegreesAboutZ_ShouldTakeXToY_Test()
        {
            var q = Quaternion.FromAxisAngle(new Vector(0, 0, 1), System.Math.PI / 2);
            var rotated = q.Rotate(new Vector(1, 0, 0));
            Assert.IsTrue(rotated.Equals(new Vector(0, 1, 0), LooseTolerance));
        }

        [TestMethod, TestCategory("Rotation")]
        public void Rotate_AboutAxis_ShouldPreserveMagnitude_Test()
        {
            var q = Quaternion.FromAxisAngle(new Vector(1, 1, 1), 1.234);
            var v = new Vector(3, -2, 5);
            Assert.AreEqual(v.Magnitude, q.Rotate(v).Magnitude, LooseTolerance);
        }

        [TestMethod, TestCategory("Rotation")]
        public void ToMatrix_TimesVector_ShouldMatchRotate_Test()
        {
            var q = Quaternion.FromAxisAngle(new Vector(0.3, 0.5, 0.8), 0.9);
            var v = new Vector(1, 2, 3);

            var viaMatrix = q.ToMatrix() * v;
            var viaRotate = q.Rotate(v);

            Assert.IsTrue(viaMatrix.Equals(viaRotate, LooseTolerance));
        }

        [TestMethod, TestCategory("Rotation")]
        public void ToAxisAngle_ShouldRoundTripFromAxisAngle_Test()
        {
            var axis = new Vector(0, 0, 1);
            var q = Quaternion.FromAxisAngle(axis, System.Math.PI / 3);

            q.ToAxisAngle(out var outAxis, out var outAngle);

            Assert.AreEqual(System.Math.PI / 3, outAngle.Rad, LooseTolerance);
            Assert.IsTrue(outAxis.Equals(axis, LooseTolerance));
        }

        [TestMethod, TestCategory("Rotation")]
        public void ComposedRotations_ShouldEqualSingleRotationOfSumOfAngles_Test()
        {
            var z = new Vector(0, 0, 1);
            var q45 = Quaternion.FromAxisAngle(z, System.Math.PI / 4);
            var q90 = Quaternion.FromAxisAngle(z, System.Math.PI / 2);

            Assert.IsTrue((q45 * q45).Equals(q90, LooseTolerance));
        }

        #endregion

        #region Interpolation

        [TestMethod, TestCategory("Interpolation")]
        public void Slerp_AtZero_ShouldReturnTheFirstQuaternion_Test()
        {
            var a = Quaternion.FromAxisAngle(new Vector(0, 0, 1), 0.2);
            var b = Quaternion.FromAxisAngle(new Vector(0, 0, 1), 1.5);
            Assert.IsTrue(Quaternion.Slerp(a, b, 0).Equals(a, LooseTolerance));
        }

        [TestMethod, TestCategory("Interpolation")]
        public void Slerp_AtOne_ShouldReturnTheSecondQuaternion_Test()
        {
            var a = Quaternion.FromAxisAngle(new Vector(0, 0, 1), 0.2);
            var b = Quaternion.FromAxisAngle(new Vector(0, 0, 1), 1.5);
            Assert.IsTrue(Quaternion.Slerp(a, b, 1).Equals(b, LooseTolerance));
        }

        [TestMethod, TestCategory("Interpolation")]
        public void Slerp_Midpoint_ShouldBeHalfwayRotation_Test()
        {
            var z = new Vector(0, 0, 1);
            var a = Quaternion.FromAxisAngle(z, 0);
            var b = Quaternion.FromAxisAngle(z, System.Math.PI / 2);

            var mid = Quaternion.Slerp(a, b, 0.5);
            var expected = Quaternion.FromAxisAngle(z, System.Math.PI / 4);

            Assert.IsTrue(mid.Equals(expected, LooseTolerance));
        }

        #endregion

        #region Equality, formatting, deconstruction

        [TestMethod, TestCategory("Equality")]
        public void Equality_WithTolerance_ShouldIgnoreTinyDifferences_Test()
        {
            Assert.IsTrue(new Quaternion(1, 2, 3, 4)
                .Equals(new Quaternion(1 + 1e-12, 2, 3, 4), Tolerance));
        }

        [TestMethod, TestCategory("Formatting")]
        public void ToString_ShouldListFourComponents_Test()
        {
            Assert.AreEqual("(1.0, 2.0, 3.0, 4.0)", new Quaternion(1, 2, 3, 4)
                .ToString("0.0", System.Globalization.CultureInfo.InvariantCulture));
        }

        [TestMethod, TestCategory("Conversion")]
        public void Deconstruct_ShouldYieldComponents_Test()
        {
            var (x, y, z, w) = new Quaternion(1, 2, 3, 4);
            Assert.AreEqual(1, x);
            Assert.AreEqual(2, y);
            Assert.AreEqual(3, z);
            Assert.AreEqual(4, w);
        }

        [TestMethod, TestCategory("Conversion")]
        public void ImplicitTupleConversion_ShouldRoundTrip_Test()
        {
            Quaternion q = (1.0, 2.0, 3.0, 4.0);
            Assert.AreEqual(new Quaternion(1, 2, 3, 4), q);
        }

        #endregion
    }
}
