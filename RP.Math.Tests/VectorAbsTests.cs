namespace RP.Math.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests pinning the two distinct "absolute value" operations on <see cref="Vector"/>:
    /// <see cref="Vector.Abs()"/> is the magnitude (scalar norm), while
    /// <see cref="Vector.AbsComponents()"/> is the component-wise <c>(|x|, |y|, |z|)</c>.
    /// </summary>
    [TestClass]
    public class VectorAbsTests
    {
        private const double Tolerance = 1e-9;

        [TestMethod, TestCategory("Abs")]
        public void Abs_ReturnsTheMagnitude_NotComponentwise_Test()
        {
            var v = new Vector(3, -4, 0);
            Assert.AreEqual(5, v.Abs(), Tolerance);           // |v|, not a vector
            Assert.AreEqual(v.Magnitude, v.Abs());                          // alias for the property
            Assert.AreEqual(5, Vector.Abs(v), Tolerance);      // static form agrees
        }

        [TestMethod, TestCategory("AbsComponents")]
        public void AbsComponents_TakesTheAbsoluteValuePerAxis_Test()
        {
            var v = new Vector(-3, 4, -5);
            Assert.IsTrue(v.AbsComponents().Equals(new Vector(3, 4, 5), Tolerance));
            Assert.IsTrue(Vector.AbsComponents(v).Equals(new Vector(3, 4, 5), Tolerance));
        }

        [TestMethod, TestCategory("AbsComponents")]
        public void AbsComponents_OfAnAlreadyPositiveVector_IsUnchanged_Test()
        {
            var v = new Vector(1, 2, 3);
            Assert.IsTrue(v.AbsComponents().Equals(v, Tolerance));
        }
    }
}
