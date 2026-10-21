namespace RP.Math.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using RP.Math.Tolerance;
    using RP.Math.Tolerance.Double;

    /// <summary>
    /// Exercises the <see cref="ITolerance{T}"/> implementations now that they are wired up (they
    /// previously threw <see cref="System.NotImplementedException"/>).
    /// </summary>
    [TestClass]
    public class ToleranceTests
    {
        [TestMethod, TestCategory("Tolerance")]
        public void AbsoluteTolerance_AcceptsWithinBand_RejectsOutside()
        {
            var tol = new AbsoluteTolerance(0.5);

            Assert.IsTrue(tol.IsWithin(10.0, 10.4));
            Assert.IsFalse(tol.IsWithin(10.0, 10.6));
        }

        [TestMethod, TestCategory("Tolerance")]
        public void RelativeTolerance_ScalesWithMagnitude()
        {
            var tol = new RelativeTolerance(0.01); // 1%

            Assert.IsTrue(tol.IsWithin(1000.0, 1005.0), "5 is within 1% of 1000");
            Assert.IsFalse(tol.IsWithin(1000.0, 1020.0), "20 exceeds 1% of 1000");
            Assert.IsTrue(tol.IsWithin(0.0, 0.0), "two exact zeros are equal");
        }

        [TestMethod, TestCategory("Tolerance")]
        public void PercentageTolerance_IsMeasuredAgainstTheTarget()
        {
            var tol = new PercentageTolerance(5); // 5%

            Assert.IsTrue(tol.IsWithin(200.0, 205.0), "5 is within 5% of 200 (=10)");
            Assert.IsFalse(tol.IsWithin(200.0, 215.0), "15 exceeds 5% of 200 (=10)");
        }

        [TestMethod, TestCategory("Tolerance")]
        public void UlpsTolerance_AcceptsAdjacentRepresentableDoubles()
        {
            var tol = new UlpsTolerance(4);

            double a = 1.0;
            double oneUlpAway = System.BitConverter.Int64BitsToDouble(System.BitConverter.DoubleToInt64Bits(a) + 1);
            double farAway = System.BitConverter.Int64BitsToDouble(System.BitConverter.DoubleToInt64Bits(a) + 100);

            Assert.IsTrue(tol.IsWithin(a, oneUlpAway));
            Assert.IsFalse(tol.IsWithin(a, farAway));
        }

        [TestMethod, TestCategory("Tolerance")]
        public void Range_IsAnOffsetBandAroundTheTarget()
        {
            var tol = new Range(-1.0, 2.0); // band is [target - 1, target + 2] = [9, 12] for target 10

            Assert.IsTrue(tol.IsWithin(11.0, 10.0), "11 is inside [9, 12]");
            Assert.IsFalse(tol.IsWithin(8.5, 10.0), "8.5 is below target - 1 (=9)");
            Assert.IsFalse(tol.IsWithin(12.5, 10.0), "12.5 is above target + 2 (=12)");
        }

        [TestMethod, TestCategory("Tolerance")]
        public void Range_OrdersItsBounds()
        {
            // Constructed reversed; Min/Max are normalised so the band [9, 12] is the same either way.
            var tol = new Range(2.0, -1.0);

            Assert.IsTrue(tol.IsWithin(11.0, 10.0));
            Assert.IsFalse(tol.IsWithin(8.5, 10.0));
        }

        [TestMethod, TestCategory("Tolerance")]
        public void AbsoluteAndUlpsTolerance_PassesIfEitherBandPasses()
        {
            var tol = new AbsoluteAndUlpsTolerance(new UlpsTolerance(1), new AbsoluteTolerance(0.5));

            // Far apart in ULPs but inside the absolute band.
            Assert.IsTrue(tol.IsWithin(100.0, 100.3));
            Assert.IsFalse(tol.IsWithin(100.0, 101.0));
        }

        [TestMethod, TestCategory("Tolerance")]
        public void AbsoluteAndRelativeTolerance_PassesIfEitherBandPasses()
        {
            var tol = new AbsoluteAndRelativeTolerance(new AbsoluteTolerance(0.001), new RelativeTolerance(0.01));

            Assert.IsTrue(tol.IsWithin(1000.0, 1009.0), "within 1% relative");
            Assert.IsTrue(tol.IsWithin(0.0, 0.0005), "within the absolute band near zero");
            Assert.IsFalse(tol.IsWithin(1000.0, 1050.0));
        }

        [TestMethod, TestCategory("Tolerance")]
        public void TolerantEqualityComparer_UsesTheTolerance_AndHashesConsistently()
        {
            var comparer = new TolerantEqualityComparer(new AbsoluteTolerance(0.5));

            Assert.IsTrue(comparer.Equals(5.0, 5.4));
            Assert.IsFalse(comparer.Equals(5.0, 6.0));

            // Equal values (per the comparer) must share a hash code.
            Assert.AreEqual(comparer.GetHashCode(5.4), comparer.GetHashCode(5.0));
        }

        [TestMethod, TestCategory("Tolerance")]
        public void TolerantComparer_TreatsWithinToleranceAsEqual()
        {
            var comparer = new TolerantComparer(new AbsoluteTolerance(0.5));

            Assert.AreEqual(0, comparer.Compare(5.0, 5.4));
            Assert.AreEqual(-1, comparer.Compare(5.0, 6.0));
            Assert.AreEqual(1, comparer.Compare(6.0, 5.0));
        }
    }
}
