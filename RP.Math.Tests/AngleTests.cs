namespace RP.Math.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit tests for the <see cref="Angle"/> type.
    /// NOTE: RP.Math is in an early state; some of these document the intended behaviour and may fail.
    /// </summary>
    [TestClass]
    public class AngleTests
    {
        private const double Tolerance = 1e-9;

        [TestMethod, TestCategory("Angle")]
        public void Rad_FromRadianConstructor_ShouldRoundTrip_Test()
        {
            var a = new Angle(System.Math.PI);
            Assert.AreEqual(System.Math.PI, a.Rad, Tolerance);
        }

        [TestMethod, TestCategory("Angle")]
        public void Deg_FromDegreeConstructor_ShouldExposeThoseDegrees_Test()
        {
            var a = new Angle(90, AngleUnits.DEG);
            Assert.AreEqual(90, a.Deg, Tolerance);
        }

        [TestMethod, TestCategory("Angle")]
        public void Deg90_ShouldEqualHalfPiRadians_Test()
        {
            var a = new Angle(90, AngleUnits.DEG);
            Assert.AreEqual(System.Math.PI / 2, a.Rad, Tolerance);
        }

        [TestMethod, TestCategory("Angle")]
        public void Grad_FromGradianConstructor_ShouldExposeThoseGradians_Test()
        {
            var a = new Angle(200, AngleUnits.GRAD);
            Assert.AreEqual(200, a.Grad, Tolerance);
        }

        [TestMethod, TestCategory("Angle")]
        public void RightAngle_ShouldBeHalfPi_Test()
        {
            Assert.AreEqual(System.Math.PI / 2, Angle.Right_Angle.Rad, Tolerance);
        }

        [TestMethod, TestCategory("Angle")]
        public void StraightAngle_ShouldBePi_Test()
        {
            Assert.AreEqual(System.Math.PI, Angle.Strait_Angle.Rad, Tolerance);
        }

        [TestMethod, TestCategory("Angle")]
        public void Addition_OfTwoAngles_ShouldSumRadians_Test()
        {
            var sum = new Angle(1.0) + new Angle(0.5);
            Assert.AreEqual(1.5, sum.Rad, Tolerance);
        }

        [TestMethod, TestCategory("Angle")]
        public void Subtraction_OfTwoAngles_ShouldSubtractRadians_Test()
        {
            var diff = new Angle(1.0) - new Angle(0.25);
            Assert.AreEqual(0.75, diff.Rad, Tolerance);
        }

        [TestMethod, TestCategory("Angle")]
        public void ImplicitConversion_FromDouble_ShouldSetRadians_Test()
        {
            Angle a = 1.25;
            Assert.AreEqual(1.25, a.Rad, Tolerance);
        }

        [TestMethod, TestCategory("Angle")]
        public void ImplicitConversion_ToDouble_ShouldYieldRadians_Test()
        {
            double r = new Angle(0.5);
            Assert.AreEqual(0.5, r, Tolerance);
        }
    }
}
