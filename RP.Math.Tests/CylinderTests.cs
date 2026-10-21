namespace RP.Math.Tests
{
    using System;


    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Math = System.Math;

    /// <summary>Unit tests for the conceptual (unpositioned) <see cref="Cylinder"/> type.</summary>
    [TestClass]
    public class CylinderTests
    {
        private const double Tol = 1e-9;

        [TestMethod, TestCategory("Construction")]
        public void Constructor_StoresRadiusAndHeight_Test()
        {
            var c = new Cylinder(2, 5);
            Assert.AreEqual(2, c.Radius);
            Assert.AreEqual(5, c.Height);
        }

        [TestMethod, TestCategory("Construction")]
        public void Constructor_NegativeRadius_ShouldThrow_Test()
        {
            Action act = () => new Cylinder(-2, 5);
            Assert.ThrowsException<ArgumentOutOfRangeException>(act);
        }

        [TestMethod, TestCategory("Measurement")]
        public void Measurements_ShouldMatchFormulae_Test()
        {
            var c = new Cylinder(2, 5);
            Assert.AreEqual(4, c.Diameter);
            Assert.AreEqual(Math.PI * 4, c.BaseArea, Tol);
            Assert.AreEqual(Math.PI * 4 * 5, c.Volume, Tol);
            Assert.AreEqual(2 * Math.PI * 2 * 5, c.LateralArea, Tol);
            Assert.AreEqual(2 * Math.PI * 2 * (2 + 5), c.SurfaceArea, Tol);
        }

        [TestMethod, TestCategory("Comparison")]
        public void Comparison_ShouldOrderByVolume_Test()
        {
            var small = new Cylinder(1, 1);
            var large = new Cylinder(2, 5);
            Assert.IsTrue((large > small));
            Assert.IsTrue((large.CompareTo(small)) > (0));
        }

        [TestMethod, TestCategory("Equality")]
        public void Equality_ShouldCompareRadiusAndHeight_Test()
        {
            Assert.IsTrue((new Cylinder(2, 5) == new Cylinder(2, 5)));
            Assert.IsTrue((new Cylinder(2, 5) != new Cylinder(2, 6)));
        }

        [TestMethod, TestCategory("Deconstruction")]
        public void Deconstruct_ShouldYieldRadiusAndHeight_Test()
        {
            var (r, h) = new Cylinder(2, 5);
            Assert.AreEqual(2, r);
            Assert.AreEqual(5, h);
        }
    }
}
