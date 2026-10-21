// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExpandedDoubleTests.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the ExpandedDoubleTests type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RP.Math.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ExpandedDoubleTests
    {
        #region IsNegative Tests

        [TestMethod, TestCategory("IsNegative")]
        public void Construction_WherePositiveWholeNumber_ShouldBePositive_Test()
        {
            double d = 100;
            var result = new ExpandedDouble(d);

            Assert.AreEqual(false, result.IsNegative, "100 is a positive number");
        }

        [TestMethod, TestCategory("IsNegative")]
        public void Construction_WhereNegativeWholeNumber_ShouldBeNagative_Test()
        {
            double d = -100;
            var result = new ExpandedDouble(d);

            Assert.AreEqual(true, result.IsNegative, "-100 is a negative number");
        }

        [TestMethod, TestCategory("IsNegative")]
        public void Construction_WherePositiveFloatingPointNumber_ShouldBePositive_Test()
        {
            double d = 100.005;
            var result = new ExpandedDouble(d);

            Assert.AreEqual(false, result.IsNegative, "100.005 is a positive number");
        }

        [TestMethod, TestCategory("IsNegative")]
        public void Construction_WhereNegativeFloatingPointNumber_ShouldBeNagative_Test()
        {
            double d = -100.005;
            var result = new ExpandedDouble(d);

            Assert.AreEqual(true, result.IsNegative, "-100.005 is a negative number");
        }

        [TestMethod, TestCategory("IsNegative")]
        public void Construction_WherePositiveInfinity_ShouldBePositive_Test()
        {
            double d = double.PositiveInfinity;
            var result = new ExpandedDouble(d);

            Assert.AreEqual(false, result.IsNegative, "+inf is a positive number");
        }

        [TestMethod, TestCategory("IsNegative")]
        public void Construction_WhereNegativeInfinity_ShouldBeNagative_Test()
        {
            double d = double.NegativeInfinity;
            var result = new ExpandedDouble(d);

            Assert.AreEqual(true, result.IsNegative, "-inf is a negative number");
        }

        [TestMethod, TestCategory("IsNegative")]
        public void Construction_WhereNaN_ShouldBePositive_Test()
        {
            // Todo: We should maybe throw an exception when testing the sign of NaN instead
            double d = double.NaN;
            var result = new ExpandedDouble(d);

            Assert.AreEqual(true, result.IsNegative, "-NaN is a negative number");
        }

        #endregion

        #region IsNaN Tests

        [TestMethod]
        public void IsNaN_WithNaN_ShouldBeTrue_Test()
        {
            double d = double.NaN;
            var result = new ExpandedDouble(d);

            Assert.AreEqual(true, result.IsNaN);
        }

        [TestMethod]
        public void IsNaN_WithOne_ShouldBeFalse_Test()
        {
            double d = 1d;
            var result = new ExpandedDouble(d);

            Assert.AreEqual(false, result.IsNaN);
        }

        [TestMethod]
        public void IsNaN_WithNegativeOne_ShouldBeFalse_Test()
        {
            double d = -1d;
            var result = new ExpandedDouble(d);

            Assert.AreEqual(false, result.IsNaN);
        }

        /// <summary>
        /// Check for Signalling NaN. Ignored: I haven't found a way of producing a SNaN.
        /// </summary>
        [TestMethod, Ignore]
        public void IsSignnallingNaN_WithSignallingNaN_ShouldBeTrue_Test()
        {
            double d = 0d / 0d; // Produces quite NaN
            var result = new ExpandedDouble(d);

            Assert.AreEqual(true, result.IsSignallingNaN);
        }

        [TestMethod]
        public void IsQuietNaN_WithSignallingNaN_ShouldBeTrue_Test()
        {
            double d = 0d / 0d;
            var result = new ExpandedDouble(d);

            Assert.AreEqual(true, result.IsQuietNaN);
        }

        #endregion

        #region IsInfinte

        [TestMethod]
        public void IsInfinite_WithPositiveInfinity_ShouldBeTrue_Test()
        {
            double d = double.PositiveInfinity;
            var result = new ExpandedDouble(d);

            Assert.AreEqual(true, result.IsInifinite);
        }

        [TestMethod]
        public void IsInfinite_WithNegativeInfinity_ShouldBeTrue_Test()
        {
            double d = double.NegativeInfinity;
            var result = new ExpandedDouble(d);

            Assert.AreEqual(true, result.IsInifinite);
        }

        [TestMethod]
        public void IsInfinite_WithOne_ShouldBeFalse_Test()
        {
            double d = 1d;
            var result = new ExpandedDouble(d);

            Assert.AreEqual(false, result.IsInifinite);
        }

        [TestMethod]
        public void IsPositiveInfinity_WithPositiveInfinity_ShouldBeTrue_Test()
        {
            double d = double.PositiveInfinity;
            var result = new ExpandedDouble(d);

            Assert.AreEqual(true, result.IsPositiveInfinity);
        }

        [TestMethod]
        public void IsPositiveInfinity_WithNegativeInfinity_ShouldBeFalse_Test()
        {
            double d = double.NegativeInfinity;
            var result = new ExpandedDouble(d);

            Assert.AreEqual(false, result.IsPositiveInfinity);
        }

        [TestMethod]
        public void IsPositiveInfinitye_WithOne_ShouldBeFalse_Test()
        {
            double d = 1d;
            var result = new ExpandedDouble(d);

            Assert.AreEqual(false, result.IsPositiveInfinity);
        }

        [TestMethod]
        public void IsNegativeInfinity_WithPositiveInfinity_ShouldBeFalse_Test()
        {
            double d = double.PositiveInfinity;
            var result = new ExpandedDouble(d);

            Assert.AreEqual(false, result.IsNegativeInfinity);
        }

        [TestMethod]
        public void IsNegativeInfinity_WithNegativeInfinity_ShouldBeTrue_Test()
        {
            double d = double.NegativeInfinity;
            var result = new ExpandedDouble(d);

            Assert.AreEqual(true, result.IsNegativeInfinity);
        }

        [TestMethod]
        public void IsNegativeInfinity_WithOne_ShouldBeFalse_Test()
        {
            double d = 1d;
            var result = new ExpandedDouble(d);

            Assert.AreEqual(false, result.IsNegativeInfinity);
        }

        #endregion

        #region Exponent

        [TestMethod]
        public void ExponentAsStored_WhereExponentIsNotZero_ShouldBeCorrect_Test()
        {
            double d = 75.5;
            var result = new ExpandedDouble(d);

            Assert.AreEqual(1029, result.ExponentAsStored, "The exponent of 75.5 should be 1029 before removing the bias");
        }

        [TestMethod]
        public void Exponent_WhereExponentIsNotZero_ShouldBeCorrect_Test()
        {
            double d = 75.5;
            var result = new ExpandedDouble(d);

            Assert.AreEqual(6, result.Exponent, "75.5 = 1.0010111 x 2^6, so the unbiased exponent is 6 (stored 1029 minus the IEEE-754 bias of 1023)");
        }

        [TestMethod]
        public void ExponentBitsAsStored_WhereExponentIsNotZero_ShouldBeCorrect_Test()
        {
            double d = 75.5;
            var result = new ExpandedDouble(d);

            CollectionAssert.AreEqual(new[] { 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 1 }, System.Linq.Enumerable.ToArray(result.ExponentBitsAsStored));
        }

        #endregion

        #region Mantissa

        [TestMethod]
        public void MantissaAsStored_WhereExponentIsNotZero_ShouldBeCorrect_Test()
        {
            double d = 75.5;
            var result = new ExpandedDouble(d);

            Assert.AreEqual(809240558043136UL, result.MantissaAsStored, "The mantissa of 75.5 should be 809240558043136 as it is stored in binary");
        }

        [TestMethod]
        public void Mantissa_WhereExponentIsNotZero_ShouldBeCorrectWithImpliedLeadingOne_Test()
        {
            double d = 75.5;
            var result = new ExpandedDouble(d);

            Assert.AreEqual(5312840185413632UL, result.Mantissa, "the mantissa of 75.5 is the stored 809240558043136 with the implicit leading one restored at bit 52 (+2^52)");
        }

        [TestMethod]
        public void MantissaBitsAsStored_WhereExponentIsNotZero_ShouldBeCorrect_Test()
        {
            double d = 75.5;
            var result = new ExpandedDouble(d);

            CollectionAssert.AreEqual(new[] { 0, 0, 1, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, System.Linq.Enumerable.ToArray(result.MantissaBitsAsStored));
        }

        #endregion
    }
}
