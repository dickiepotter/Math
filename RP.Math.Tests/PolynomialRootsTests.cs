namespace RP.Math.Tests
{
    using System;
    using System.Linq;


    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>Unit tests for the <see cref="PolynomialRoots"/> real-root solver.</summary>
    [TestClass]
    public class PolynomialRootsTests
    {
        private const double Tol = 1e-7;

        private static void ShouldContainRoots(double[] actual, params double[] expected)
        {
            foreach (double e in expected)
            {
                Assert.IsTrue(actual.Any(r => Math.Abs(r - e) < Tol), $"root {e} should be found in [{string.Join(", ", actual)}]");
            }

            Assert.AreEqual(expected.Length, actual.Length);
        }

        [TestMethod, TestCategory("Quadratic")]
        public void Quadratic_TwoRoots_Test()
        {
            ShouldContainRoots(PolynomialRoots.SolveQuadratic(1, -3, 2), 1, 2);
        }

        [TestMethod, TestCategory("Quadratic")]
        public void Quadratic_NoRealRoots_Test()
        {
            Assert.AreEqual(0, PolynomialRoots.SolveQuadratic(1, 0, 1).Length);
        }

        [TestMethod, TestCategory("Cubic")]
        public void Cubic_ThreeRealRoots_Test()
        {
            // (x-1)(x-2)(x-3) = x^3 - 6x^2 + 11x - 6
            ShouldContainRoots(PolynomialRoots.SolveCubic(1, -6, 11, -6), 1, 2, 3);
        }

        [TestMethod, TestCategory("Cubic")]
        public void Cubic_OneRealRoot_Test()
        {
            // x^3 + x + 1 = 0 has a single real root near -0.6823
            double[] roots = PolynomialRoots.SolveCubic(1, 0, 1, 1);
            Assert.AreEqual(1, roots.Length);
            Assert.AreEqual(-0.6823278, roots[0], 1e-6);
        }

        [TestMethod, TestCategory("Quartic")]
        public void Quartic_FourRealRoots_Test()
        {
            // (x-1)(x-2)(x-3)(x-4) = x^4 - 10x^3 + 35x^2 - 50x + 24
            ShouldContainRoots(PolynomialRoots.SolveQuartic(1, -10, 35, -50, 24), 1, 2, 3, 4);
        }

        [TestMethod, TestCategory("Quartic")]
        public void Quartic_Biquadratic_Test()
        {
            // (x^2-1)(x^2-4) = x^4 - 5x^2 + 4
            ShouldContainRoots(PolynomialRoots.SolveQuartic(1, 0, -5, 0, 4), -2, -1, 1, 2);
        }

        [TestMethod, TestCategory("Quartic")]
        public void Quartic_NoRealRoots_Test()
        {
            // (x^2+1)(x^2+4) = x^4 + 5x^2 + 4
            Assert.AreEqual(0, PolynomialRoots.SolveQuartic(1, 0, 5, 0, 4).Length);
        }
    }
}
